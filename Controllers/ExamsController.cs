// ExamsController.cs
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
    public class ExamsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ExamsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 21. Create exam
        [HttpPost("create")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamRequestDto request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var examId = await connection.QueryFirstOrDefaultAsync<int>(
                    "sp_CreateExam",
                    new
                    {
                        request.InstructorId,
                        request.CourseId,
                        request.Title,
                        request.DurationMinutes,
                        request.TotalMarks,
                        request.PassingScore,
                        request.IsPublished
                    },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new CreateExamResponseDto
                {
                    ExamId = examId,
                    Message = "Exam created successfully"
                });
            }
            catch (SqlException ex) when (ex.Number >= 75000 && ex.Number < 76000)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 22. Update exam
        [HttpPut("update")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateExam([FromBody] UpdateExamRequestDto request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<int>(
                    "sp_UpdateExam",
                    new
                    {
                        request.InstructorId,
                        request.ExamId,
                        request.Title,
                        request.DurationMinutes,
                        request.TotalMarks,
                        request.PassingScore,
                        request.IsPublished
                    },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new UpdateExamResponseDto
                {
                    Updated = result == 1,
                    Message = "Exam updated successfully"
                });
            }
            catch (SqlException ex) when (ex.Number >= 75100 && ex.Number < 75200)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 23. Delete exam
        [HttpDelete("delete")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteExam([FromBody] DeleteExamRequestDto request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<string>(
                    "sp_DeleteExam",
                    new
                    {
                        request.InstructorId,
                        request.ExamId
                    },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new DeleteExamResponseDto
                {
                    Result = result
                });
            }
            catch (SqlException ex) when (ex.Number >= 75200 && ex.Number < 75300)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 24. Get all exams
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllExams()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var exams = await connection.QueryAsync<ExamResponseDto>(
                    "sp_GetAllExams",
                    commandType: CommandType.StoredProcedure
                );

                return Ok(exams);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 25. Get exams by course
        [HttpGet("by-course/{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExamsByCourse(int courseId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var exams = await connection.QueryAsync<ExamResponseDto>(
                    "sp_GetExamsByCourse",
                    new { CourseId = courseId },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(exams);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("details/{examId}")]
        [Authorize]
        public async Task<IActionResult> GetExamDetails(int examId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                using var multi = await connection.QueryMultipleAsync(
                    "sp_GetExamDetails",
                    new { ExamId = examId },
                    commandType: CommandType.StoredProcedure
                );

                // Read exam header
                var exam = await multi.ReadFirstOrDefaultAsync<dynamic>();
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

                var questionData = await multi.ReadAsync<dynamic>();

                var examDetails = new ExamDetailsResponseDto
                {
                    ExamId = exam.ExamId,
                    Title = exam.Title,
                    DurationMinutes = exam.DurationMinutes,
                    TotalMarks = exam.TotalMarks,
                    PassingScore = exam.PassingScore,
                    IsPublished = exam.IsPublished,
                    CourseCode = exam.CourseCode,
                    CourseName = exam.CourseName,
                    CreatedByInstructor = exam.CreatedByInstructor
                };

                // Group questions and choices
                var questionsDict = new Dictionary<int, ExamQuestionDto>();
                foreach (var row in questionData)
                {
                    if (!questionsDict.TryGetValue((int)row.QuestionId, out var questionDto))
                    {
                        questionDto = new ExamQuestionDto
                        {
                            OrderNo = (int)row.OrderNo,
                            QuestionId = (int)row.QuestionId,
                            QuestionText = row.QuestionText,
                            QuestionType = row.QuestionType,
                            Points = (int)row.Points
                        };
                        questionsDict[(int)row.QuestionId] = questionDto;
                        examDetails.Questions.Add(questionDto);
                    }

                    if (row.ChoiceId != null)
                    {
                        questionDto.Choices.Add(new ChoiceDto
                        {
                            ChoiceId = (int)row.ChoiceId,
                            ChoiceText = row.ChoiceText
                        });
                    }
                }

                return Ok(examDetails);
            }
            catch (SqlException ex) when (ex.Number == 75301)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 27. Get exams by instructor
        [HttpGet("by-instructor/{instructorId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetExamsByInstructor(int instructorId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var exams = await connection.QueryAsync<ExamResponseDto>(
                    "sp_GetExamsByInstructor",
                    new { InstructorId = instructorId },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(exams);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 28. Get students' grades for a specific exam
        [HttpGet("grades/{examId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetExamGrades(int examId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var grades = await connection.QueryAsync<ExamGradeResponseDto>(
                    "sp_GetExamGrades",
                    new { ExamId = examId },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(grades);
            }
            catch (SqlException ex) when (ex.Number == 75401)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
        // 29. Add question to exam
        [HttpPost("add-to-exam")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddQuestionToExam([FromBody] AddQuestionToExamRequestDto request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                await connection.ExecuteAsync(
                    "AddQuestionToExam",
                    new
                    {
                        request.ExamId,
                        request.QuestionId,
                        request.OrderNo,
                        request.PointsOverride
                    },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new AddQuestionToExamResponseDto
                {
                    Success = true,
                    Message = $"Question {request.QuestionId} successfully added to exam {request.ExamId}",
                    ExamId = request.ExamId,
                    QuestionId = request.QuestionId,
                    OrderNo = request.OrderNo,
                    PointsOverride = request.PointsOverride
                });
            }
            catch (SqlException ex) when (ex.Number == 50000) 
            {
                var errorMessage = ex.Message;
                if (errorMessage.Contains("does not exist"))
                {
                    return NotFound(new { message = errorMessage });
                }
                else if (errorMessage.Contains("already in exam"))
                {
                    return Conflict(new { message = errorMessage });
                }
                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // Additional endpoint: Get instructor's own exams (using JWT claims)
        [HttpGet("my-exams")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyExams()
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorIdClaim) || !int.TryParse(instructorIdClaim, out int instructorId))
                return Unauthorized(new { message = "Invalid instructor ID in token" });

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var exams = await connection.QueryAsync<ExamResponseDto>(
                    "sp_GetExamsByInstructor",
                    new { InstructorId = instructorId },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(exams);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}