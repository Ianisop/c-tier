using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Security.Policy;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.CodeAnalysis;
using Microsoft.CSharp;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;


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
                if (options != null) return JsonSerializer.Deserialize<T>(jsonContent, options);
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

        //method to load instances of an abstract class
        // Dynamic method to load instances of a specified base class or interface
        public static List<T> LoadAndCreateInstances<T>(string[] csFiles)
        {
            var syntaxTrees = csFiles.Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file))).ToList();

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location));

            var compilation = CSharpCompilation.Create(
                "DynamicAssembly",
                syntaxTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        Console.WriteLine(diagnostic.GetMessage());
                    }
                    throw new Exception("Compilation failed!");
                }

                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());

                // Find all types that inherit from or implement T
                var baseClassType = typeof(T);
                if (!baseClassType.IsClass && !baseClassType.IsInterface)
                {
                    throw new ArgumentException("T must be a class or interface");
                }

                var types = assembly.GetTypes().Where(t => baseClassType.IsAssignableFrom(t) && !t.IsAbstract);

                // Create instances of the found types
                var instances = new List<T>();
                foreach (var type in types)
                {
                    var instance = (T)Activator.CreateInstance(type);
                    instances.Add(instance);
                }

                return instances;
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

        public static string GetCpuUsage()
        {
            try
            {
                var cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
                cpuCounter.NextValue(); // init the counter
                System.Threading.Thread.Sleep(1000);
                float cpuUsage = cpuCounter.NextValue();
                return $"Current CPU Usage: {cpuUsage}%";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching CPU usage: {ex.Message}");
                return "Error fetching CPU usage";
            }
        }

        public static string GetNetworkUsage()
        {
            try
            {
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                StringBuilder networkStats = new StringBuilder();

                networkStats.AppendLine("---------------");
                foreach (var ni in networkInterfaces)
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        IPv4InterfaceStatistics stats = ni.GetIPv4Statistics();
                        networkStats.AppendLine($"Network Interface: {ni.Name}");
                        networkStats.AppendLine($"Bytes Sent: {stats.BytesSent / 1024} KB");
                        networkStats.AppendLine($"Bytes Received: {stats.BytesReceived / 1024} KB");
                        networkStats.AppendLine("---------------");
                    }
                }
                return networkStats.Length > 0 ? networkStats.ToString() : "No active network interfaces found.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching network usage: {ex.Message}");
                return "Error fetching network usage";
            }
        }

        public static string GetMemoryUsage()
        {
            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                long memoryUsage = currentProcess.WorkingSet64; // in bytes
                return $"Process Memory Usage: {(memoryUsage / (1024 * 1024))} MB"; // mb convertion
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching memory usage: {ex.Message}");
                return "Error fetching memory usage";
            }
        }

    }
}


