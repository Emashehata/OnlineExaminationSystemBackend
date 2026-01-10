using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineExaminationSystem.DTO.Tracks;
using System.Data;

namespace OnlineExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TracksController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TracksController(IConfiguration config)
        {
            _config = config;
        }

        // Anyone logged in can read branches
        [HttpGet]
        
        public async Task<IActionResult> GetAll()
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var data = await con.QueryAsync<TrackDto>(
                "sp_GetAllTracks",
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }
    }
}

