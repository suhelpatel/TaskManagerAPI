using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagerAPI.Helpers;
using TaskManagerAPI.Interfaces;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _repo;

        public AuthController(IConfiguration configuration,IUserRepository repo)
        {
            _configuration = configuration;
            _repo = repo;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public IActionResult Register(LoginModel model)
        {
            var user = new User
            {
                Username = model.Username,
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                Role = "User"
            };

            _repo.AddUser(user);

            return Ok("User registered");
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginModel model)
        {
            var user = _repo.GetUser(model.Username);

            if (user == null)
                return Unauthorized();

            var hashedPassword = PasswordHelper.HashPassword(model.Password);

            if (user.PasswordHash != hashedPassword)
                return Unauthorized();

            var token = GenerateToken(user);

            return Ok(new { token });
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var cerds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
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
