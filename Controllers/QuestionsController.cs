// QuestionsController.cs
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

        public QuestionsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 29. Add question
        [HttpPost("add")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddQuestion([FromBody] AddQuestionRequestDto request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var questionId = await connection.QueryFirstOrDefaultAsync<int>(
                    "sp_AddQuestion",
                    new
                    {
                        request.InstructorId,
                        request.CourseId,
                        request.QuestionText,
                        request.QuestionType,
                        request.DefaultMark
                    },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new AddQuestionResponseDto
                {
                    QuestionId = questionId,
                    Message = "Question added successfully"
                });
            }
            catch (SqlException ex) when (ex.Number >= 76000 && ex.Number < 77000)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 30. Update question
        [HttpPut("update")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateQuestion([FromBody] UpdateQuestionRequestDto request)
        {
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
                    Message = "Question updated successfully"
                });
            }
            catch (SqlException ex) when (ex.Number >= 76100 && ex.Number < 76200)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
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
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 33. Add choice 
        [HttpPost("choices/add-batch")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddQuestionChoicesBatch([FromBody] List<AddChoiceRequestDto> requests)
        {
            // Validate input
            if (requests == null || requests.Count == 0)
                return BadRequest(new { message = "No choices provided" });

            // All choices must be for the same question
            var questionId = requests.First().QuestionId;
            if (requests.Any(r => r.QuestionId != questionId))
                return BadRequest(new { message = "All choices must be for the same question" });

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                // Get question type and owner
                var questionData = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT QuestionType, CreatedByInstructorId FROM Questions WHERE QuestionId = @QuestionId",
                    new { QuestionId = questionId }
                );

                if (questionData == null)
                    return BadRequest(new { message = "Question not found" });

                string questionType = questionData.QuestionType;
                int questionOwnerId = questionData.CreatedByInstructorId;

                // Check if current user owns the question
                var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(instructorIdClaim) || !int.TryParse(instructorIdClaim, out int currentInstructorId))
                    return Unauthorized(new { message = "Invalid instructor ID in token" });

                if (questionOwnerId != currentInstructorId)
                    return Forbid();

                // Validate based on question type
                if (questionType == "MCQ")
                {
                    if (requests.Count != 4)
                        return BadRequest(new { message = "MCQ questions require exactly 4 choices" });

                    var correctCount = requests.Count(c => c.IsCorrect);
                    if (correctCount != 1)
                        return BadRequest(new { message = "MCQ must have exactly 1 correct choice" });

                    // Prepare parameters for MCQ
                    var parameters = new
                    {
                        QuestionId = questionId,
                        Choice1 = requests[0].ChoiceText,
                        IsCorrect1 = requests[0].IsCorrect,
                        Choice2 = requests[1].ChoiceText,
                        IsCorrect2 = requests[1].IsCorrect,
                        Choice3 = requests[2].ChoiceText,
                        IsCorrect3 = requests[2].IsCorrect,
                        Choice4 = requests[3].ChoiceText,
                        IsCorrect4 = requests[3].IsCorrect
                    };

                    var choicesAdded = await connection.QueryFirstOrDefaultAsync<int>(
                        "sp_AddChoicesBatch",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        Success = true,
                        Message = "MCQ choices added successfully",
                        QuestionId = questionId,
                        QuestionType = questionType,
                        ChoicesAdded = choicesAdded,
                        Choices = requests.Select(c => new
                        {
                            c.ChoiceText,
                            c.IsCorrect
                        })
                    });
                }
                else if (questionType == "TF")
                {
                    if (requests.Count != 2)
                        return BadRequest(new { message = "True/False questions require exactly 2 choices" });

                    var correctCount = requests.Count(c => c.IsCorrect);
                    if (correctCount != 1)
                        return BadRequest(new { message = "True/False must have exactly 1 correct choice" });

                    // Validate that choices are "True" and "False"
                    var choiceTexts = requests.Select(r => r.ChoiceText.Trim().ToLower()).ToList();
                    var requiredChoices = new List<string> { "true", "false" };

                    // Check if both "true" and "false" are present (order doesn't matter)
                    if (!choiceTexts.Contains("true") || !choiceTexts.Contains("false"))
                        return BadRequest(new { message = "TF choices must be 'True' and 'False'" });

                    // For TF, pass NULL for choice3, choice4
                    var parameters = new
                    {
                        QuestionId = questionId,
                        Choice1 = requests[0].ChoiceText,
                        IsCorrect1 = requests[0].IsCorrect,
                        Choice2 = requests[1].ChoiceText,
                        IsCorrect2 = requests[1].IsCorrect,
                        Choice3 = (string)null,  
                        IsCorrect3 = (bool?)null, 
                        Choice4 = (string)null,   
                        IsCorrect4 = (bool?)null  
                    };

                    var choicesAdded = await connection.QueryFirstOrDefaultAsync<int>(
                        "sp_AddChoicesBatch",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        Success = true,
                        Message = "True/False choices added successfully",
                        QuestionId = questionId,
                        QuestionType = questionType,
                        ChoicesAdded = choicesAdded,
                        Choices = requests.Select(c => new
                        {
                            c.ChoiceText,
                            c.IsCorrect
                        })
                    });
                }
                else
                {
                    return BadRequest(new { message = $"Unsupported question type: {questionType}" });
                }
            }
            catch (SqlException ex) when (ex.Number == 76401)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (SqlException ex) when (ex.Number >= 76400 && ex.Number < 76700)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
        // 35. Update choice
        [HttpPut("choices/update")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateChoice([FromBody] UpdateChoiceRequestDto request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
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
                    Message = "Choice updated successfully"
                });
            }
            catch (SqlException ex) when (ex.Number >= 76500 && ex.Number < 76600)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
        // 36. Get my questions
        [HttpGet("my-questions")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyQuestions()
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorIdClaim) || !int.TryParse(instructorIdClaim, out int instructorId))
                return Unauthorized(new { message = "Invalid instructor ID in token" });

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
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
                      ORDER BY q.QuestionId",
                    new { InstructorId = instructorId },
                    commandType: CommandType.Text
                );

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}