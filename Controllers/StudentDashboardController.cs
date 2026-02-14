using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineExaminationSystem.DTO.StudentsDto;
using System.Data;
using System.Security.Claims;



namespace OnlineExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/student-dashboard")]
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentDashboardController(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection CreateConnection()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                using var connection = CreateConnection();
                await connection.OpenAsync();

                // ================= SUMMARY =================
                using var multi = await connection.QueryMultipleAsync(
                    "sp_GetStudentDashboardSummary",
                    new { StudentId = studentId },
                    commandType: CommandType.StoredProcedure);

                var summary = await multi.ReadFirstOrDefaultAsync<StudentSummaryDto>();
                var upcoming = await multi.ReadFirstOrDefaultAsync<UpcomingDto>();

                // ================= PERFORMANCE =================
                var performance = (await connection.QueryAsync<PerformanceDto>(
                    "sp_GetStudentPerformanceBySubject",
                    new { StudentId = studentId },
                    commandType: CommandType.StoredProcedure)).ToList();

                // ================= SCORE TREND =================
                var trend = (await connection.QueryAsync<ScoreTrendDto>(
                    "sp_GetStudentScoreTrend",
                    new { StudentId = studentId },
                    commandType: CommandType.StoredProcedure)).ToList();

                // ================= RECENT RESULTS =================
                var recent = (await connection.QueryAsync<RecentResultDto>(
                    "sp_GetStudentRecentResults",
                    new { StudentId = studentId },
                    commandType: CommandType.StoredProcedure)).ToList();

                // ================= GRADE DISTRIBUTION =================
                var grades = await connection.QueryFirstOrDefaultAsync<GradeDistributionDto>(
                    "sp_GetStudentGradeDistribution",
                    new { StudentId = studentId },
                    commandType: CommandType.StoredProcedure);

                // ================= FINAL RESPONSE =================
                var dashboard = new StudentDashboardDto
                {
                    ExamsTaken = summary?.ExamsTaken ?? 0,
                    AverageScore = summary?.AverageScore ?? 0,
                    BestScore = summary?.BestScore ?? 0,
                    UpcomingExams = upcoming?.UpcomingExams ?? 0,
                    Performance = performance,
                    ScoreTrend = trend,
                    RecentResults = recent,
                    GradeDistribution = grades ?? new GradeDistributionDto()
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}