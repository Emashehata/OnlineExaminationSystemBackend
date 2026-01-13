using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineExaminationSystem.DTO.Instructors;
using System.Data;
using System.Security.Claims;

namespace OnlineExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // all endpoints require login
    public class InstructorsController : ControllerBase
    {
        private readonly IConfiguration _config;
        public InstructorsController(IConfiguration config) => _config = config;

        private SqlConnection Conn() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        // 1) Admin Add Instructor
        // POST: /api/Instructors/admin-add
        [HttpPost("admin-add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAdd(AdminAddInstructorRequestDto request)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdStr))
                return Unauthorized("Missing admin id in token.");

            int adminId = int.Parse(adminIdStr);

            using var con = Conn();

            // SP returns: SELECT @InstructorUserId AS InstructorId;
            var instructorId = await con.QuerySingleAsync<int>(
                "sp_AdminAddInstructor",
                new
                {
                    AdminUserId = adminId,
                    request.FullName,
                    request.Email,
                    request.PasswordHash,
                    request.BranchId,
                    request.TrackId
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { InstructorId = instructorId });
        }

        // 2) Update Instructor (Admin)
        // PUT: /api/Instructors/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateInstructorRequestDto request)
        {
            using var con = Conn();

            // SP returns: SELECT 1 AS Updated;
            var updated = await con.QuerySingleAsync<int>(
                "sp_UpdateInstructor",
                new
                {
                    InstructorId = id,
                    request.FullName,
                    request.Email,
                    request.PasswordHash,
                    request.BranchId,
                    request.TrackId
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { Updated = updated == 1 });
        }

        // 3) Delete Instructor (Admin)
        // DELETE: /api/Instructors/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            using var con = Conn();

            var result = await con.QuerySingleAsync<string>(
                "sp_DeleteInstructor",
                new { InstructorId = id },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { Result = result });
        }

        // 4) Get All Instructors (Admin + Instructor)
        // GET: /api/Instructors
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetAll()
        {
            using var con = Conn();

            var data = await con.QueryAsync<InstructorListDto>(
                "sp_GetAllInstructors",
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

        // 5) Get Instructor By Id (Admin + Instructor)
        // GET: /api/Instructors/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetById(int id)
        {
            using var con = Conn();

            var data = await con.QueryFirstOrDefaultAsync<InstructorDetailsDto>(
                "sp_GetInstructorById",
                new { InstructorId = id },
                commandType: CommandType.StoredProcedure
            );

            if (data == null) return NotFound("Instructor not found.");
            return Ok(data);
        }

        // 6) Get Instructors By Branch (Admin + Instructor)
        // GET: /api/Instructors/by-branch/{branchId}
        [HttpGet("by-branch/{branchId:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetByBranch(int branchId)
        {
            using var con = Conn();

            var data = await con.QueryAsync<InstructorListDto>(
                "sp_GetInstructorsByBranch",
                new { BranchId = branchId },
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

        // 7) Get Instructors By Track (Admin + Instructor)
        // GET: /api/Instructors/by-track/{trackId}
        [HttpGet("by-track/{trackId:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetByTrack(int trackId)
        {
            using var con = Conn();

            var data = await con.QueryAsync<InstructorListDto>(
                "sp_GetInstructorsByTrack",
                new { TrackId = trackId },
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

        // 8) Get Instructor Courses (Instructor Admin)
        [HttpGet("{id:int}/courses")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetInstructorCoursesById(int id)
        {
            using var con = Conn();

            var data = await con.QueryAsync<OnlineExaminationSystem.DTO.Instructors.InstructorCourseDto>(
                "sp_GetInstructorsCourses",
                new { InstructorId = id },
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

        [HttpGet("Instructor/courses")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyCourses()
        {
            var myIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(myIdStr))
                return Unauthorized("Missing instructor id in token.");

            int myId = int.Parse(myIdStr);

            using var con = Conn();

            var data = await con.QueryAsync<OnlineExaminationSystem.DTO.Instructors.InstructorCourseDto>(
                "sp_GetInstructorsCourses",
                new { InstructorId = myId },
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

    }
}

