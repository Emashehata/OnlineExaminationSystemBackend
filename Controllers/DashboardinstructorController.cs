using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineExaminationSystem.DTO.InstructorDashboard;
using System.Data;

namespace OnlineExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = "Instructor")]
    public class DashboardController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DashboardController(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection CreateConnection()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        // ============================
        // 1️⃣ Summary Cards
        // ============================
        [HttpGet("summary/{instructorId}")]
        public async Task<IActionResult> GetSummary(int instructorId)
        {
            using var con = CreateConnection();

            var result = await con.QueryFirstOrDefaultAsync<InstructorDashboardSummaryDto>(
                "sp_InstructorDashboard_Summary",
                new { InstructorId = instructorId },
                commandType: CommandType.StoredProcedure
            );

            return Ok(result);
        }

        // ============================
        // 2️⃣ Exam Performance
        // ============================
        [HttpGet("exam-performance/{instructorId}")]
        public async Task<IActionResult> GetExamPerformance(int instructorId)
        {
            using var con = CreateConnection();

            var data = await con.QueryAsync<ExamPerformanceDto>(
                "sp_InstructorDashboard_ExamPerformance",
                new { InstructorId = instructorId },
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

        // ============================
        // 3️⃣ Enrollment Trend
        // ============================
        [HttpGet("enrollment-trend/{instructorId}")]
        public async Task<IActionResult> GetEnrollmentTrend(int instructorId)
        {
            using var con = CreateConnection();

            var data = await con.QueryAsync<EnrollmentTrendDto>(
                "sp_InstructorDashboard_EnrollmentTrend",
                new { InstructorId = instructorId },
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }
    }
}

