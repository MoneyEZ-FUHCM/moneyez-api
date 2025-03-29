using System;
using System.Security.Cryptography;

namespace MoneyEz.Services.Utils
{
    public static class SecretKeyGenerator
    {
        public static string GenerateSecretKey(int length = 32)
        {
            // Use cryptographically secure random number generator
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            
            // Convert to Base64 and remove special characters
            return Convert.ToBase64String(bytes)
                .Replace("/", "")
                .Replace("+", "")
                .Replace("=", "")
                .Substring(0, length);
        }
    }
}
