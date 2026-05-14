using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return Convert.ToHexString(hashBytes);
        }

        /// <summary>
        /// Compares a plain text password with a hashed password by hashing the input password and checking for a match.
        /// </summary>
        /// <param name="password">The plain text password to verify.</param>
        /// <param name="hashedPassword">The hashed password to compare against.</param>
        /// <returns>true if the password matches the hashed password; otherwise, false.</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashOfInput = HashPassword(password);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hashedPassword) == 0;
        }
    }
}