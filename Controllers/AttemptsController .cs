// AttemptsController.cs
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineExaminationSystem.DTOs;
using OnlineExaminationSystem.Services;
using System.Data;
using System.Security.Claims;

namespace OnlineExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttemptsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AttemptsController> _logger;
        private readonly IAttemptService _attemptService;

        public AttemptsController(
            IConfiguration configuration,
            ILogger<AttemptsController> logger,
            IAttemptService attemptService)
        {
            _configuration = configuration;
            _logger = logger;
            _attemptService = attemptService;
        }

        // Helper methods
        private async Task<IDbConnection> CreateConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("Invalid user ID in token");
            return userId;
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        private async Task<bool> ValidateAttemptOwnership(int attemptId, int studentId, IDbConnection connection)
        {
            var attemptCheck = await connection.QueryFirstOrDefaultAsync<int>(
                @"SELECT COUNT(*) FROM Attempts 
                  WHERE AttemptId = @AttemptId 
                  AND StudentId = @StudentId",
                new { AttemptId = attemptId, StudentId = studentId }
            );
            return attemptCheck > 0;
        }

        private async Task<bool> IsAttemptSubmitted(int attemptId, IDbConnection connection)
        {
            var attemptStatus = await connection.QueryFirstOrDefaultAsync<DateTime?>(
                @"SELECT SubmittedAt FROM Attempts 
                  WHERE AttemptId = @AttemptId",
                new { attemptId }
            );
            return attemptStatus.HasValue;
        }

        // 37. Create new attempt
        [HttpPost("create")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateAttempt([FromBody] CreateAttemptRequestDto request)
        {
            try
            {
                var studentIdFromToken = GetUserIdFromToken();

                // Ensure student can only create attempts for themselves
                if (studentIdFromToken != request.StudentId)
                    return Forbid();

                using var connection = await CreateConnection();
                var result = await _attemptService.CreateAttemptAsync(connection, request);

                return Ok(new CreateAttemptResponseDto
                {
                    AttemptId = result.AttemptId,
                    AttemptNo = result.AttemptNo,
                    StartedAt = DateTime.Now,
                    Message = "Attempt created successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (SqlException ex) when (ex.Number >= 77000 && ex.Number < 77100)
            {
                string errorMessage = ex.Number switch
                {
                    77001 => "Exam not found or not published",
                    77002 => "Invalid Student ID",
                    _ => ex.Message
                };
                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating attempt for Student {StudentId}, Exam {ExamId}",
                    request.StudentId, request.ExamId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // 38. Add student answer
        [HttpPost("answers/add")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AddStudentAnswer([FromBody] AddAnswerRequestDto request)
        {
            try
            {
                var studentId = GetUserIdFromToken();
                using var connection = await CreateConnection();

                // Validate ownership and attempt status
                if (!await ValidateAttemptOwnership(request.AttemptId, studentId, connection))
                    return Forbid();

                if (await IsAttemptSubmitted(request.AttemptId, connection))
                    return BadRequest(new { message = "Cannot add answers to a submitted attempt" });

                // Add the answer
                var result = await _attemptService.AddStudentAnswerAsync(connection, request);

                return Ok(new SaveAnswerResponseDto
                {
                    Saved = result.Saved == 1,
                    IsCorrect = result.IsCorrect,
                    EarnedMarks = result.EarnedMarks,
                    Message = "Answer saved successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (SqlException ex) when (ex.Number >= 77100 && ex.Number < 77200)
            {
                string errorMessage = GetSqlErrorMessage(ex.Number);
                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding student answer for Attempt {AttemptId}", request.AttemptId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // 39. Update student answer
        [HttpPut("answers/update")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateStudentAnswer([FromBody] UpdateAnswerRequestDto request)
        {
            try
            {
                var studentId = GetUserIdFromToken();
                using var connection = await CreateConnection();

                // Validate ownership and attempt status
                if (!await ValidateAttemptOwnership(request.AttemptId, studentId, connection))
                    return Forbid();

                if (await IsAttemptSubmitted(request.AttemptId, connection))
                    return BadRequest(new { message = "Cannot update answers in a submitted attempt" });

                // Update the answer
                var result = await _attemptService.UpdateStudentAnswerAsync(connection, request);

                return Ok(new SaveAnswerResponseDto
                {
                    Saved = result.Updated == 1,
                    IsCorrect = result.IsCorrect,
                    EarnedMarks = result.EarnedMarks,
                    Message = "Answer updated successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (SqlException ex) when (ex.Number >= 77200 && ex.Number < 77300)
            {
                string errorMessage = GetSqlErrorMessage(ex.Number);
                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student answer for Attempt {AttemptId}", request.AttemptId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // 40. Get attempt answers
        [HttpGet("{attemptId}/answers")]
        [Authorize]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetAttemptAnswers(int attemptId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var role = GetUserRole();
                using var connection = await CreateConnection();

                // Check permission to view answers
                if (!await HasPermissionToViewAttempt(attemptId, userId, role, connection))
                    return Forbid();

                var answers = await _attemptService.GetAttemptAnswersAsync(connection, attemptId);
                return Ok(answers);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting answers for Attempt {AttemptId}", attemptId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // 41. Get student attempts
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Student,Instructor,Admin")]
        public async Task<IActionResult> GetStudentAttempts(int studentId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var role = GetUserRole();

                // Validate permissions
                await ValidateStudentAttemptsAccess(studentId, userId, role);

                using var connection = await CreateConnection();
                var attempts = await _attemptService.GetStudentAttemptsAsync(connection, studentId);
                return Ok(attempts);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ForbidException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attempts for Student {StudentId}", studentId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // 42. Get student score in exam
        [HttpGet("student/{studentId}/exam/{examId}/score")]
        [Authorize]
        public async Task<IActionResult> GetStudentScoreInExam(int studentId, int examId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var role = GetUserRole();

                // Validate permissions
                await ValidateScoreAccess(studentId, examId, userId, role);

                using var connection = await CreateConnection();
                var score = await _attemptService.GetStudentScoreInExamAsync(connection, studentId, examId);

                if (score == null)
                    return NotFound(new { message = "No attempts found for this student in this exam" });

                return Ok(score);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ForbidException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting score for Student {StudentId}, Exam {ExamId}", studentId, examId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // 43. Get exam summary (for instructors)
        [HttpGet("exam/{examId}/summary")]
        [Authorize(Roles = "Instructor,Admin")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetExamSummary(int examId)
        {
            try
            {
                var instructorId = GetUserIdFromToken();
                using var connection = await CreateConnection();

                // Check ownership (unless admin)
                if (!User.IsInRole("Admin") && !await ValidateExamOwnership(examId, instructorId, connection))
                    return Forbid();

                var summary = await _attemptService.GetExamSummaryAsync(connection, examId);

                if (summary == null)
                    return NotFound(new { message = "Exam not found" });

                return Ok(summary);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting summary for Exam {ExamId}", examId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // 44. Delete attempt
        [HttpDelete("{attemptId}")]
        [Authorize(Roles = "Student,Instructor,Admin")]
        public async Task<IActionResult> DeleteAttempt(int attemptId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var role = GetUserRole();
                using var connection = await CreateConnection();

                // Validate deletion permissions
                if (!await HasPermissionToDeleteAttempt(attemptId, userId, role, connection))
                    return Forbid();

                var result = await _attemptService.DeleteAttemptAsync(connection, attemptId);

                return Ok(new DeleteResponseDto
                {
                    Deleted = result > 0,
                    Message = result > 0 ? "Attempt deleted successfully" : "Attempt not found"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (SqlException ex) when (ex.Number == 77301)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Attempt {AttemptId}", attemptId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // Submit attempt
        [HttpPost("{attemptId}/submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitAttempt(int attemptId)
        {
            try
            {
                var studentId = GetUserIdFromToken();
                using var connection = await CreateConnection();

                // Validate ownership
                if (!await ValidateAttemptOwnership(attemptId, studentId, connection))
                    return Forbid();

                var result = await _attemptService.SubmitAttemptAsync(connection, attemptId);

                return Ok(new
                {
                    Success = true,
                    AttemptId = result.AttemptId,
                    Score = result.Score,
                    MaxMarks = result.MaxMarks,
                    PassingMarks = result.PassingMarks,
                    IsPassed = result.IsPassed,
                    Percentage = result.Percentage,
                    Message = "Attempt submitted successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (SqlException ex) when (ex.Number >= 77400 && ex.Number < 77500)
            {
                string errorMessage = ex.Number switch
                {
                    77401 => "Attempt not found",
                    77402 => "Attempt already submitted",
                    _ => ex.Message
                };
                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting Attempt {AttemptId}", attemptId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // Permission validation helper methods
        private async Task<bool> HasPermissionToViewAttempt(int attemptId, int userId, string role, IDbConnection connection)
        {
            if (role == "Student")
            {
                return await ValidateAttemptOwnership(attemptId, userId, connection);
            }
            else if (role == "Instructor")
            {
                var instructorCheck = await connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(*) 
                      FROM Attempts a
                      JOIN Exams e ON a.ExamId = e.ExamId
                      JOIN Courses c ON e.CourseId = c.CourseId
                      WHERE a.AttemptId = @AttemptId 
                      AND c.InstructorId = @InstructorId",
                    new { AttemptId = attemptId, InstructorId = userId }
                );
                return instructorCheck > 0;
            }
            return role == "Admin";
        }

        private async Task ValidateStudentAttemptsAccess(int targetStudentId, int userId, string role)
        {
            if (role == "Student" && userId != targetStudentId)
                throw new ForbidException();

            if (role == "Instructor")
            {
                using var connection = await CreateConnection();
                var hasAccess = await connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(*) 
                      FROM Students s
                      JOIN StudentCourses sc ON s.StudentId = sc.StudentId
                      JOIN Courses c ON sc.CourseId = c.CourseId
                      WHERE s.StudentId = @StudentId
                      AND c.InstructorId = @InstructorId",
                    new { StudentId = targetStudentId, InstructorId = userId }
                );

                if (hasAccess == 0)
                    throw new ForbidException();
            }
        }

        private async Task ValidateScoreAccess(int studentId, int examId, int userId, string role)
        {
            if (role == "Student" && userId != studentId)
                throw new ForbidException();

            if (role == "Instructor")
            {
                using var connection = await CreateConnection();
                var hasAccess = await connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(*) 
                      FROM Exams e
                      JOIN Courses c ON e.CourseId = c.CourseId
                      WHERE e.ExamId = @ExamId
                      AND c.InstructorId = @InstructorId",
                    new { ExamId = examId, InstructorId = userId }
                );

                if (hasAccess == 0)
                    throw new ForbidException();
            }
        }

        private async Task<bool> ValidateExamOwnership(int examId, int instructorId, IDbConnection connection)
        {
            var ownershipCheck = await connection.QueryFirstOrDefaultAsync<int>(
                @"SELECT COUNT(*) 
                  FROM Exams e
                  JOIN Courses c ON e.CourseId = c.CourseId
                  WHERE e.ExamId = @ExamId
                  AND c.InstructorId = @InstructorId",
                new { ExamId = examId, InstructorId = instructorId }
            );
            return ownershipCheck > 0;
        }

        private async Task<bool> HasPermissionToDeleteAttempt(int attemptId, int userId, string role, IDbConnection connection)
        {
            if (role == "Student")
            {
                return await ValidateAttemptOwnership(attemptId, userId, connection);
            }
            else if (role == "Instructor")
            {
                var instructorCheck = await connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(*) 
                      FROM Attempts a
                      JOIN Exams e ON a.ExamId = e.ExamId
                      JOIN Courses c ON e.CourseId = c.CourseId
                      WHERE a.AttemptId = @AttemptId 
                      AND c.InstructorId = @InstructorId",
                    new { AttemptId = attemptId, InstructorId = userId }
                );
                return instructorCheck > 0;
            }
            return role == "Admin";
        }

        private string GetSqlErrorMessage(int errorNumber)
        {
            return errorNumber switch
            {
                77101 => "Invalid Attempt ID",
                77102 => "Invalid Question ID",
                77103 => "Selected choice does not belong to this question",
                77104 => "Answer already exists for this question in this attempt",
                77201 => "Answer not found for this question in this attempt",
                77202 => "Selected choice does not belong to this question",
                _ => "An error occurred while processing your request"
            };
        }
    }

    // Custom exception for Forbid scenarios
    public class ForbidException : Exception
    {
        public ForbidException() : base("Access denied") { }
    }
}