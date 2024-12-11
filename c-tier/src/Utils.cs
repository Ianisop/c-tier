using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

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

        private static readonly char[] DefaultCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        private static Dictionary<string, (long bytesSent, long bytesReceived)> previousStats = new();
        private static Dictionary<string, (long bytesSent, long bytesReceived)> totalStats = new();
        private static DateTime lastNetworkUpdate = DateTime.Now;

        public static T ReadFromFile<T>(string filePath, JsonSerializerOptions options = null)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(jsonContent, options ?? defaultJsonSerializerOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return default;
            }
        }

        public static bool WriteToFile(object tempObj, string fileName)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(tempObj, defaultJsonSerializerOptions);
                File.WriteAllText(fileName, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
                return false;
            }
        }

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

                var baseClassType = typeof(T);
                if (!baseClassType.IsClass && !baseClassType.IsInterface)
                {
                    throw new ArgumentException("T must be a class or interface");
                }

                var types = assembly.GetTypes().Where(t => baseClassType.IsAssignableFrom(t) && !t.IsAbstract);

                var instances = new List<T>();
                foreach (var type in types)
                {
                    var instance = (T)Activator.CreateInstance(type);
                    instances.Add(instance);
                }

                return instances;
            }
        }

        public static ulong GenerateID(int length)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var randomBytes = new byte[8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            ulong randomPart = BitConverter.ToUInt64(randomBytes, 0);
            string combined = timestamp.ToString() + randomPart;
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            ulong id = BitConverter.ToUInt64(hash, 0);
            return ulong.Parse(id.ToString().Substring(0, Math.Min(length, id.ToString().Length)));
        }

        public static string GenerateRandomString(int length)
        {
            if (length <= 0) return "";
            var random = new Random();
            return new string(Enumerable.Repeat(DefaultCharacters, length)
                                        .Select(chars => chars[random.Next(chars.Length)]).ToArray());
        }

        public static double GetCpuUsage()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var startCpuTime = Process.GetCurrentProcess().TotalProcessorTime;

                Thread.Sleep(500);

                var endCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
                stopwatch.Stop();

                var cpuUsage = (endCpuTime - startCpuTime).TotalMilliseconds / (Environment.ProcessorCount * stopwatch.ElapsedMilliseconds);
                return Math.Round(cpuUsage * 100, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching CPU usage: {ex.Message}");
                return -1;
            }
        }

        public static string GetNetworkUsage()
        {
            try
            {
                var sb = new StringBuilder();
                var currentTime = DateTime.Now;
                double sinceLast = (currentTime - lastNetworkUpdate).TotalSeconds;

                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        var stats = ni.GetIPv4Statistics();
                        long currentBytesSent = stats.BytesSent;
                        long currentBytesReceived = stats.BytesReceived;

                        if (!previousStats.ContainsKey(ni.Name))
                        {
                            previousStats[ni.Name] = (currentBytesSent, currentBytesReceived);
                            totalStats[ni.Name] = (currentBytesSent, currentBytesReceived);
                        }

                        var (prevSent, prevRecieved) = previousStats[ni.Name];
                        var (totalSent, totalRecieved) = totalStats[ni.Name];

                        sb.AppendLine($"Interface: {ni.Name}");
                        sb.AppendLine($"Recieved: {((currentBytesSent - prevSent) / sinceLast)/1024:F2} KB/s");
                        sb.AppendLine($"Transferred: {((currentBytesReceived - prevRecieved) / sinceLast) / 1024:F2} KB/s");

                        previousStats[ni.Name] = (currentBytesSent, currentBytesReceived);
                    }
                }
                return sb.ToString();
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
                using var currentProcess = Process.GetCurrentProcess();
                return $"{(currentProcess.WorkingSet64 / 1024 / 1024)} MB";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching memory usage: {ex.Message}");
                return "Error fetching memory usage";
            }
        }
    }
}
