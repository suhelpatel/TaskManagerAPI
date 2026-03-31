using System.Security.Cryptography;

namespace TaskManagerAPI.Helpers
{
    public class TokenHelper
    {
        public static string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
