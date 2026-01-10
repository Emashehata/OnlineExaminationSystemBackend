using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineExaminationSystem.DTO.Branches;
using System.Data;

namespace OnlineExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public BranchesController(IConfiguration config)
        {
            _config = config;
        }

        // Anyone logged in can read branches
        [HttpGet]
      
        public async Task<IActionResult> GetAll()
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var data = await con.QueryAsync<BranchDto>(
                "sp_GetAllBranches",
                commandType: CommandType.StoredProcedure
            );

            return Ok(data);
        }
    }
}

