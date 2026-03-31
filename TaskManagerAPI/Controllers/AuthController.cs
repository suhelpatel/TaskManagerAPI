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
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration,IUserRepository repo, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _repo = repo;
            _logger = logger;
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
                return Unauthorized("User not found");

            var hashedPassword = PasswordHelper.HashPassword(model.Password);

            if (user.PasswordHash != hashedPassword)
            {
                _logger.LogWarning("Invalid login attempt");
                return Unauthorized("Invalid password");
            }

            var token = GenerateToken(user);

            var refreshToken = TokenHelper.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            _repo.UpdateUser(user);

            _logger.LogInformation("User logged in: {Username}", model.Username);

            return Ok(new { 
                token,
                refreshToken
            });
        }

        private string GenerateToken(User user)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var cerds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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
            catch (Exception ex)
            {
                _logger.LogError("Something failed in GenerateToken Method \n" + ex.Message);
                throw;
            }
            
        }

        [HttpPost("refresh")]
        public IActionResult Refresh(TokenModel model)
        {
            var user = _repo.GetUserByRefreshToken(model.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.Now)
                return Unauthorized("Invalid refresh token");

            var newToken = GenerateToken(user);
            var newRefreshToken = TokenHelper.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            _repo.UpdateUser(user);

            return Ok(new
            {
                token = newToken,
                refreshToken = newRefreshToken
            });
        }
    }
}
