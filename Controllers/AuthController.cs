using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using OnlineExaminationSystem.DTOs;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_LoginWithProfile",
                new
                {
                    request.Email,
                    request.PasswordHash
                },
                commandType: CommandType.StoredProcedure
            );

            if (user == null)
                return Unauthorized("Invalid email or password");

            var token = GenerateJwtToken(user);

            return Ok(new LoginResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                RoleName = user.RoleName,
                BranchId = user.BranchId,
                TrackId = user.TrackId,
                Token = token
            });
        }

        [HttpPost("register-student")]
        public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentRequestDto request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            try
            {
                // sp_AddStudent returns StudentId only
                var studentId = await connection.QueryFirstOrDefaultAsync<int>(
                    "sp_AddStudent",
                    new
                    {
                        request.FullName,
                        request.Email,
                        request.PasswordHash,
                        request.BranchId,
                        request.TrackId,
                        CreatedByAdminId = (int?)null // Self register
                    },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new
                {
                    message = "Student registered successfully. Please login.",
                    studentId
                });
            }
            catch (SqlException ex)
            {
                // Email exists, invalid branch/track, etc.
                return BadRequest(new { message = ex.Message });
            }
        }



        private string GenerateJwtToken(dynamic user)
        {
            var jwt = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            );

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.RoleName)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    double.Parse(jwt["ExpiresMinutes"]!)
                ),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

