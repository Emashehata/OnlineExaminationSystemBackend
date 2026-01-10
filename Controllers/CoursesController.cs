using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineExaminationSystem.DTO.Courses;
using System.Data;
using System.Security.Claims;

namespace OnlineExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public CoursesController(IConfiguration config) => _config = config;

        private SqlConnection Conn() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        // 1) Admin Add Course
        // POST: /api/Courses
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(AddCourseRequestDto request)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdStr))
                return Unauthorized("Missing admin id in token.");

            int adminId = int.Parse(adminIdStr);

            using var con = Conn();

            var courseId = await con.QuerySingleAsync<int>(
                "sp_AddCourse",
                new
                {
                    AdminUserId = adminId,
                    request.TrackId,
                    request.CourseCode,
                    request.CourseName,
                    request.Description
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { CourseId = courseId });
        }

        // 2) Admin Update Course
        // PUT: /api/Courses/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateCourseRequestDto request)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdStr))
                return Unauthorized("Missing admin id in token.");

            int adminId = int.Parse(adminIdStr);

            using var con = Conn();

            var updated = await con.QuerySingleAsync<int>(
                "sp_UpdateCourse",
                new
                {
                    AdminUserId = adminId,
                    CourseId = id,
                    request.TrackId,
                    request.CourseCode,
                    request.CourseName,
                    request.Description
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { Updated = updated == 1 });
        }

        // 3) Get All Courses (Admin + Instructor + Student)
        // GET: /api/Courses
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var con = Conn();

            var data = await con.QueryAsync<CourseDto>(
                "sp_GetAllCourses",
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }

        // 4) Get Course By Id (Admin + Instructor + Student)
        // GET: /api/Courses/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            using var con = Conn();

            var course = await con.QueryFirstOrDefaultAsync<CourseDetailsDto>(
                "sp_GetCourseById",
                new { CourseId = id },
                commandType: CommandType.StoredProcedure
            );

            if (course == null) return NotFound("Course not found.");
            return Ok(course);
        }

        // DELETE: /api/Courses/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdStr))
                return Unauthorized("Missing admin id in token.");

            int adminId = int.Parse(adminIdStr);

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var deleted = await con.QuerySingleAsync<int>(
                "sp_DeleteCourse",
                new
                {
                    AdminUserId = adminId,
                    CourseId = id
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { Deleted = deleted == 1 });
        }

    }
}

