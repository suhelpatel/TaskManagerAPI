using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginModel model)
        {
            // Hardcoded user (for now)
            if (model.Username == "admin" && model.Password == "admin")
            {
                var token = GenerateJwtToken(model.Username);
                return Ok(new { token });

            }
            return Unauthorized();
        }

        private string GenerateJwtToken(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var cerds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(ClaimTypes.Name,username)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: cerds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
