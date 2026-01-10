using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineExaminationSystem.DTO.Students;
using System.Data;
using System.Security.Claims;

namespace OnlineExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // must be logged in
    public class StudentsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        // GET: /api/Students
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetAll()
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var data = await con.QueryAsync<StudentListItemDto>(
                "sp_GetAllStudents",
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

        // GET: /api/Students/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetById(int id)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var student = await con.QueryFirstOrDefaultAsync<StudentListItemDto>(
                "sp_GetStudentById",
                new { StudentId = id },
                commandType: CommandType.StoredProcedure
            );

            if (student == null) return NotFound("Student not found.");
            return Ok(student);
        }

        // GET: /api/Students/by-branch/{branchId}
        [HttpGet("by-branch/{branchId:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetByBranch(int branchId)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var data = await con.QueryAsync<StudentListItemDto>(
                "sp_GetStudentsByBranch",
                new { BranchId = branchId },
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

        // GET: /api/Students/by-track/{trackId}
        [HttpGet("by-track/{trackId:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetByTrack(int trackId)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var data = await con.QueryAsync<StudentListItemDto>(
                "sp_GetStudentsByTrack",
                new { TrackId = trackId },
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

        // GET: /api/Students/search?name=eman
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var data = await con.QueryAsync<StudentListItemDto>(
                "sp_SearchStudentsByName",
                new { Name = name },
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

        // PUT: /api/Student
        [HttpPut("Update-Student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateMe(UpdateStudentRequestDto request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("Missing user id in token.");

            int studentId = int.Parse(userIdStr);

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var updated = await con.QueryFirstOrDefaultAsync<int>(
                "sp_UpdateStudent",
                new
                {
                    StudentId = studentId,
                    request.FullName,
                    request.Email,
                    request.PasswordHash,
                   
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { Updated = updated == 1 });
        }

        // DELETE: /api/Students/admin-delete
        [HttpDelete("admin-delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDelete(AdminDeleteStudentRequestDto request)
        {
            // AdminId from token
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier"))?.Value;
            if (string.IsNullOrWhiteSpace(adminIdClaim)) return Unauthorized("Missing admin id in token.");

            int adminId = int.Parse(adminIdClaim);

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var result = await con.QueryFirstOrDefaultAsync<dynamic>(
                "sp_AdminDeleteStudent",
                new { AdminUserId = adminId, StudentId = request.StudentId },
                commandType: CommandType.StoredProcedure
            );

            return Ok(result);
        }
    }
}
