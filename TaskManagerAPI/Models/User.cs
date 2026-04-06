using System.Security.Cryptography;

namespace TaskManagerAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        public int FailedAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }
}
