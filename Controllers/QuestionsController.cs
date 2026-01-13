using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineExaminationSystem.DTOs;
using System.Data;
using System.Security.Claims;

namespace OnlineExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuestionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(IConfiguration configuration, ILogger<QuestionsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // 30. Update question (FIXED - Add ownership check)
        [HttpPut("update")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateQuestion([FromBody] UpdateQuestionRequestDto request)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorIdClaim) || !int.TryParse(instructorIdClaim, out int currentInstructorId))
                return Unauthorized(new { message = "Invalid instructor ID in token" });

            if (currentInstructorId != request.InstructorId)
                return Forbid();

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<int>(
                    "sp_UpdateQuestion",
                    new
                    {
                        request.InstructorId,
                        request.QuestionId,
                        request.QuestionText,
                        request.QuestionType,
                        request.DefaultMark
                    },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new UpdateResponseDto
                {
                    Updated = result == 1,
                    Message = result == 1 ? "Question updated successfully" : "Question not found or no changes made"
                });
            }
            catch (SqlException ex) when (ex.Number >= 76100 && ex.Number < 76200)
            {
                string errorMessage = ex.Number switch
                {
                    76101 => "Question not found",
                    76102 => "Only the creator instructor can update this question",
                    76103 => "Invalid question type",
                    76104 => "Default mark must be greater than 0",
                    _ => ex.Message
                };
                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating question");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 31. Get questions by course
        [HttpGet("by-course/{courseId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetQuestionsByCourse(int courseId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var questions = await connection.QueryAsync<QuestionResponseDto>(
                    "sp_GetQuestionsByCourse",
                    new { CourseId = courseId },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting questions by course");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 32. Get questions by exam
        [HttpGet("by-exam/{examId}")]
        [Authorize]
        public async Task<IActionResult> GetQuestionsByExam(int examId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                using var multi = await connection.QueryMultipleAsync(
                    "sp_GetQuestionsByExam",
                    new { ExamId = examId },
                    commandType: CommandType.StoredProcedure
                );

                var questionData = await multi.ReadAsync<dynamic>();

                var questionsDict = new Dictionary<int, ExamQuestionResponseDto>();
                foreach (var row in questionData)
                {
                    if (!questionsDict.TryGetValue((int)row.QuestionId, out var questionDto))
                    {
                        questionDto = new ExamQuestionResponseDto
                        {
                            OrderNo = (int)row.OrderNo,
                            QuestionId = (int)row.QuestionId,
                            QuestionText = row.QuestionText,
                            QuestionType = row.QuestionType,
                            Points = (int)row.Points
                        };
                        questionsDict[(int)row.QuestionId] = questionDto;
                    }

                    if (row.ChoiceId != null)
                    {
                        questionDto.Choices.Add(new ChoiceDto
                        {
                            ChoiceId = (int)row.ChoiceId,
                            ChoiceText = row.ChoiceText,
                            IsCorrect = (bool)row.IsCorrect
                        });
                    }
                }

                return Ok(questionsDict.Values.OrderBy(q => q.OrderNo));
            }
            catch (SqlException ex) when (ex.Number == 76301)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting questions by exam");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 33. Add questions with choices 
        [HttpPost("add-with-choices")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddQuestionWithChoices(
            [FromBody] AddQuestionWithChoicesRequestDto request)
        {
            // Validate input
            if (request == null)
                return BadRequest(new { message = "Request cannot be null" });

            if (request.Choices == null || request.Choices.Count == 0)
                return BadRequest(new { message = "No choices provided" });

            if (string.IsNullOrWhiteSpace(request.QuestionText))
                return BadRequest(new { message = "Question text is required" });

            // Quick validation
            if (request.QuestionType != "MCQ" && request.QuestionType != "TF")
                return BadRequest(new { message = "QuestionType must be 'MCQ' or 'TF'" });

            if (request.DefaultMark <= 0)
                return BadRequest(new { message = "Default mark must be greater than 0" });

            // Verify instructor ID from token matches request
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorIdClaim) || !int.TryParse(instructorIdClaim, out int currentInstructorId))
                return Unauthorized(new { message = "Invalid instructor ID in token" });

            if (currentInstructorId != request.InstructorId)
                return Forbid();

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                // Prepare parameters based on question type
                object parameters;

                if (request.QuestionType == "TF")
                {
                    if (request.Choices.Count != 2)
                        return BadRequest(new { message = "True/False questions require exactly 2 choices" });

                    // Validate TF choices content
                    var choiceTexts = request.Choices.Select(c => c.ChoiceText.Trim().ToLower()).ToList();
                    if (!choiceTexts.Contains("true") || !choiceTexts.Contains("false"))
                        return BadRequest(new { message = "TF choices must be 'True' and 'False'" });

                    var correctCount = request.Choices.Count(c => c.IsCorrect);
                    if (correctCount != 1)
                        return BadRequest(new { message = "True/False must have exactly 1 correct choice" });

                    parameters = new
                    {
                        request.InstructorId,
                        request.CourseId,
                        request.QuestionText,
                        request.QuestionType,
                        request.DefaultMark,
                        Choice1 = request.Choices[0].ChoiceText,
                        IsCorrect1 = request.Choices[0].IsCorrect,
                        Choice2 = request.Choices[1].ChoiceText,
                        IsCorrect2 = request.Choices[1].IsCorrect,
                        Choice3 = (string)null,
                        IsCorrect3 = (bool?)null,
                        Choice4 = (string)null,
                        IsCorrect4 = (bool?)null
                    };
                }
                else // MCQ
                {
                    if (request.Choices.Count != 4)
                        return BadRequest(new { message = "MCQ questions require exactly 4 choices" });

                    var correctCount = request.Choices.Count(c => c.IsCorrect);
                    if (correctCount != 1)
                        return BadRequest(new { message = "MCQ must have exactly 1 correct choice" });

                    // Check for duplicate choices
                    var distinctChoices = request.Choices
                        .Select(c => c.ChoiceText.Trim().ToLower())
                        .Distinct()
                        .Count();

                    if (distinctChoices != request.Choices.Count)
                        return BadRequest(new { message = "MCQ choices must be unique" });

                    parameters = new
                    {
                        request.InstructorId,
                        request.CourseId,
                        request.QuestionText,
                        request.QuestionType,
                        request.DefaultMark,
                        Choice1 = request.Choices[0].ChoiceText,
                        IsCorrect1 = request.Choices[0].IsCorrect,
                        Choice2 = request.Choices[1].ChoiceText,
                        IsCorrect2 = request.Choices[1].IsCorrect,
                        Choice3 = request.Choices[2].ChoiceText,
                        IsCorrect3 = request.Choices[2].IsCorrect,
                        Choice4 = request.Choices[3].ChoiceText,
                        IsCorrect4 = request.Choices[3].IsCorrect
                    };
                }

                // Execute the single stored procedure with typed result
                var result = await connection.QueryFirstOrDefaultAsync<QuestionCreationResult>(
                    "sp_AddQuestionWithChoices",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                    return BadRequest(new { message = "Failed to add question and choices" });

                return Ok(new AddQuestionWithChoicesResponseDto
                {
                    QuestionId = result.QuestionId,
                    Message = "Question and choices added successfully",
                    ChoicesAdded = result.ChoicesAdded
                });
            }
            catch (SqlException ex) when (ex.Number >= 76000 && ex.Number < 77000)
            {
                // Handle custom error codes from stored procedure
                string errorMessage = ex.Number switch
                {
                    76001 => "Invalid Instructor ID",
                    76002 => "Invalid Course ID",
                    76003 => "Question type must be MCQ or TF",
                    76004 => "Default mark must be greater than 0",
                    76405 => "True/False questions require exactly 2 choices",
                    76406 => "MCQ questions require exactly 4 choices",
                    _ => ex.Message
                };

                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding question with choices");
                return StatusCode(500, new
                {
                    message = "An error occurred while adding the question",
                    error = ex.Message
                });
            }
        }

        // 35. Update choice (FIXED - Add ownership check)
        [HttpPut("choices/update")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateChoice([FromBody] UpdateChoiceRequestDto request)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorIdClaim) || !int.TryParse(instructorIdClaim, out int currentInstructorId))
                return Unauthorized(new { message = "Invalid instructor ID in token" });

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                // First check if the instructor owns this choice
                var ownershipCheck = await connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(*) 
                      FROM Choices c 
                      INNER JOIN Questions q ON c.QuestionId = q.QuestionId
                      WHERE c.ChoiceId = @ChoiceId 
                      AND q.CreatedByInstructorId = @InstructorId",
                    new { request.ChoiceId, InstructorId = currentInstructorId }
                );

                if (ownershipCheck == 0)
                    return Forbid();

                var result = await connection.QueryFirstOrDefaultAsync<int>(
                    "sp_UpdateChoice",
                    new
                    {
                        request.ChoiceId,
                        request.ChoiceText,
                        request.IsCorrect
                    },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new UpdateResponseDto
                {
                    Updated = result == 1,
                    Message = result == 1 ? "Choice updated successfully" : "Choice not found"
                });
            }
            catch (SqlException ex) when (ex.Number >= 76500 && ex.Number < 76600)
            {
                string errorMessage = ex.Number switch
                {
                    76501 => "Choice not found",
                    76502 => "Invalid choice data",
                    _ => ex.Message
                };
                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating choice");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // Delete question (FIXED - Already correct)
        [HttpDelete("{questionId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorIdClaim) || !int.TryParse(instructorIdClaim, out int instructorId))
                return Unauthorized(new { message = "Invalid instructor ID in token" });

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var result = await connection.ExecuteAsync(
                    "sp_DeleteQuestion",
                    new
                    {
                        QuestionId = questionId,
                        InstructorId = instructorId
                    },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new
                {
                    Deleted = result > 0,
                    Message = result > 0 ? "Question deleted successfully" : "Question not found"
                });
            }
            catch (SqlException ex) when (ex.Number >= 76200 && ex.Number < 76300)
            {
                string errorMessage = ex.Number switch
                {
                    76201 => "Question not found",
                    76202 => "Only the creator instructor can delete this question",
                    76203 => "Cannot delete question: it is used in an exam",
                    _ => ex.Message
                };

                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question");
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

        // Get question by ID with choices (NEW - Fixed implementation)
        [HttpGet("{questionId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetQuestionById(int questionId)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorIdClaim) || !int.TryParse(instructorIdClaim, out int currentInstructorId))
                return Unauthorized(new { message = "Invalid instructor ID in token" });

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                // First get the question
                var question = await connection.QueryFirstOrDefaultAsync<QuestionWithChoicesDto>(
                    @"SELECT 
                        q.QuestionId,
                        q.QuestionText,
                        q.QuestionType,
                        q.DefaultMark,
                        q.CourseId,
                        q.CreatedByInstructorId
                      FROM Questions q
                      WHERE q.QuestionId = @QuestionId",
                    new { QuestionId = questionId }
                );

                if (question == null)
                    return NotFound(new { message = "Question not found" });

                // Check ownership (unless admin)
                if (!User.IsInRole("Admin") && question.CreatedByInstructorId != currentInstructorId)
                    return Forbid();

                // Then get choices
                var choices = await connection.QueryAsync<ChoiceDto>(
                    @"SELECT 
                        ChoiceId, 
                        ChoiceText, 
                        IsCorrect 
                      FROM Choices 
                      WHERE QuestionId = @QuestionId 
                      ORDER BY ChoiceId",
                    new { QuestionId = questionId }
                );

                question.Choices = choices.ToList();

                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting question by ID");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // Get choices by question ID
        [HttpGet("{questionId}/choices")]
        [Authorize]
        public async Task<IActionResult> GetChoicesByQuestion(int questionId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                // First check if question exists
                var questionExists = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM Questions WHERE QuestionId = @QuestionId",
                    new { QuestionId = questionId }
                );

                if (questionExists == 0)
                    return NotFound(new { message = "Question not found" });

                var choices = await connection.QueryAsync<ChoiceDto>(
                    @"SELECT ChoiceId, ChoiceText, IsCorrect 
                      FROM Choices 
                      WHERE QuestionId = @QuestionId 
                      ORDER BY ChoiceId",
                    new { QuestionId = questionId }
                );

                return Ok(choices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting choices by question");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 36. Get my questions (FIXED - Add pagination support)
        [HttpGet("my-questions")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyQuestions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorIdClaim) || !int.TryParse(instructorIdClaim, out int instructorId))
                return Unauthorized(new { message = "Invalid instructor ID in token" });

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                // Calculate offset for pagination
                int offset = (page - 1) * pageSize;

                var questions = await connection.QueryAsync<QuestionResponseDto>(
                    @"SELECT 
                        q.QuestionId,
                        q.QuestionText,
                        q.QuestionType,
                        q.DefaultMark,
                        q.CreatedByInstructorId,
                        (SELECT COUNT(*) FROM Choices c WHERE c.QuestionId = q.QuestionId) AS ChoicesCount
                      FROM Questions q
                      WHERE q.CreatedByInstructorId = @InstructorId
                      ORDER BY q.QuestionId DESC
                      OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                    new
                    {
                        InstructorId = instructorId,
                        Offset = offset,
                        PageSize = pageSize
                    }
                );

                // Get total count
                var totalCount = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM Questions WHERE CreatedByInstructorId = @InstructorId",
                    new { InstructorId = instructorId }
                );

                return Ok(new
                {
                    Questions = questions,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my questions");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // NEW: Check if user can modify a question
        [HttpGet("{questionId}/can-modify")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CanModifyQuestion(int questionId)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorIdClaim) || !int.TryParse(instructorIdClaim, out int instructorId))
                return Unauthorized(new { message = "Invalid instructor ID in token" });

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var canModify = await connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(*) 
                      FROM Questions 
                      WHERE QuestionId = @QuestionId 
                      AND CreatedByInstructorId = @InstructorId",
                    new { QuestionId = questionId, InstructorId = instructorId }
                );

                return Ok(new { CanModify = canModify > 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking question modify permission");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}