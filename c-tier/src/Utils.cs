using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Security.Policy;
using System.Security.Cryptography;

namespace c_tier.src
{
    public static class Utils
    {
        public static string NL = Environment.NewLine; // shortcut
        public static string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
        public static string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
        public static string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
        public static string YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";
        public static string BLUE = Console.IsOutputRedirected ? "" : "\x1b[94m";
        public static string MAGENTA = Console.IsOutputRedirected ? "" : "\x1b[95m";
        public static string CYAN = Console.IsOutputRedirected ? "" : "\x1b[96m";
        public static string GREY = Console.IsOutputRedirected ? "" : "\x1b[97m";
        public static string BOLD = Console.IsOutputRedirected ? "" : "\x1b[1m";
        public static string NOBOLD = Console.IsOutputRedirected ? "" : "\x1b[22m";
        public static string UNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[4m";
        public static string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
        public static string REVERSE = Console.IsOutputRedirected ? "" : "\x1b[7m";
        public static string NOREVERSE = Console.IsOutputRedirected ? "" : "\x1b[27m";
        public static JsonSerializerOptions defaultJsonSerializerOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
        };

        private static readonly char[] DefaultCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray(); // char set for string generation
        /// <summary>
        /// Serializes json into object, optionally pass JsonSerializerOptions if you dont want to use the default ones
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns> :shrug: </returns>
        public static T ReadFromFile<T>(string filePath, JsonSerializerOptions options = null)
        {
            try
            {
                
                string jsonContent = File.ReadAllText(filePath);
                if(options != null)return JsonSerializer.Deserialize<T>(jsonContent, options);
                return JsonSerializer.Deserialize<T>(jsonContent, defaultJsonSerializerOptions);
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error reading or deserializing file: {ex.Message}");
                Frontend.Log(ex.Message);
                return default;
            }
        }
        public static bool WriteToFile(object tempObj,string fileName)
        {
            try
            {

                string jsonString = JsonSerializer.Serialize(tempObj);
                File.WriteAllText(fileName, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error reading or deserializing file: {ex.Message}");
                Frontend.Log(ex.Message);
                return false;
            }
        }


        public static UInt64 GenerateID(int length)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            byte[] randomBytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            UInt64 randomPort = BitConverter.ToUInt64(randomBytes, 0);
            string combined = timestamp.ToString() + randomPort.ToString();
            using (var sha256 =  SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                UInt64 id = BitConverter.ToUInt64(hash, 0);
                string idString = id.ToString().Substring(0, Math.Min(length,id.ToString().Length));
                return UInt64.Parse(idString);
            }
        }
        
        public static string GenerateRandomString(int length)
        {
            if (length <= 0)
            {
                return "";
            }

            char[] characters = (new string(DefaultCharacters)).ToCharArray();
            StringBuilder result = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }

            return result.ToString();
        }

    }
}


