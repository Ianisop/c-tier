using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.server
{
    internal class Auth
    {

        private const int SaltSize = 32;
        private const int Iterations = 100000;
        private const int HashSize = 64;

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];

            using (var rng = RandomNumberGenerator.Create()) 
            {
                rng.GetBytes(salt);
            }

            byte[] hash = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256).GetBytes(HashSize);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            string[] parts = storedHash.Split(':');
            if (parts.Length != 2)
                throw new FormatException("Password stored in invalid format.");

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[1]);
            byte[] hash = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256).GetBytes(HashSize);


            return CryptographicOperations.FixedTimeEquals(hash, storedHashBytes);
        }
    }
}
