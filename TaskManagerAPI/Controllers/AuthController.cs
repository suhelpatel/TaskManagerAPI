using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagerAPI.Helpers;
using TaskManagerAPI.Interfaces;
using TaskManagerAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using Hangfire;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _repo;
        private readonly ILogger<AuthController> _logger;
        private readonly IMemoryCache _cache;

        public AuthController(IConfiguration configuration,IUserRepository repo, ILogger<AuthController> logger, IMemoryCache cache)
        {
            _configuration = configuration;
            _repo = repo;
            _logger = logger;
            _cache = cache;
        }

        // REGISTER (ADMIN ONLY)
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

            return Ok("User created");
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


        // CURRENT USER
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var username = User.Identity.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new { username, role });
        }

        // ADMIN ONLY
        [Authorize(Roles = "Admin")]
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            if(!_cache.TryGetValue("users",out List<User>  users))
            {
                users = await _repo.GetAllUsersAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set("users", users, cacheOptions);
            }

            return Ok(new
            {
                success = true,
                data = users
            });
        }


        [HttpGet("run-job")]
        public IActionResult RunJob()
        {
            BackgroundJob.Enqueue(() => Console.WriteLine("🔥 Job triggered from API"));

            return Ok("Job Started");
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
