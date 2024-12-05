using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using c_tier.src.backend.client;

namespace c_tier.src.backend.server
{
    internal class Auth
    {

        private const int SaltSize = 32;
        private const int Iterations = 100000;
        private const int HashSize = 64;
        private const double SessionTTL = 120 * 60 * 1000; // TTL for sessions, 120 minutes * 60 seconds * 1000 millis.

        // dict to store active sessions
        private static readonly ConcurrentDictionary<string, (string Username, DateTime Expiry)> Sessions = new ConcurrentDictionary<string, (string, DateTime)>();

        private static readonly System.Timers.Timer SessionExpiryTimer;

        static Auth()
        {
            SessionExpiryTimer = new System.Timers.Timer(60000); // once every minute
            SessionExpiryTimer.Elapsed += (sender, e) => CleanupExpiredSessions();
            SessionExpiryTimer.AutoReset = true;
            SessionExpiryTimer.Start();
        }

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

        public static string CreateSession(string username, string password)
        {
            string hashedPassword = Database.GetHashedPassword(username);

            bool validPassword = VerifyPassword(password, hashedPassword);

            if (!validPassword) 
            {
                throw new InvalidDataException("Invalid password for: " + username);
            }
            string sessionToken = Utils.GenerateRandomString(64);
            DateTime expiry = DateTime.UtcNow.AddMilliseconds(SessionTTL);
            Sessions[sessionToken] = (username, expiry);

            return sessionToken;
        }

        public static bool IsValidSession(string sessionToken, string username)
        {
            if (Sessions.TryGetValue(sessionToken, out var sessionInfo))
            {
                if (sessionInfo.Expiry > DateTime.UtcNow && sessionInfo.Username == username)
                {
                    return true;
                }
                else if (sessionInfo.Expiry <= DateTime.UtcNow)
                {
                    Sessions.TryRemove(sessionToken, out _);
                }
            }
            return false;
        }

        public static void RemoveSession(string sessionToken)
        {
            Sessions.TryRemove(sessionToken, out _);
        }

        public static void CleanupExpiredSessions()
        {
            foreach (var session in Sessions)
            {
                if (session.Value.Expiry <= DateTime.UtcNow)
                {
                    Sessions.TryRemove(session.Key, out _);
                }
            }
        }
    }
}
