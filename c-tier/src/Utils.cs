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
                if (File.Exists(fileName)) File.Delete(fileName); // remove old entry
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
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        var stats = ni.GetIPv4Statistics();
                        sb.AppendLine($"Interface: {ni.Name}");
                        sb.AppendLine($"Bytes Sent: {stats.BytesSent / 1024} KB");
                        sb.AppendLine($"Bytes Received: {stats.BytesReceived / 1024} KB");
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

        public static string ConvertKeyToString(RSAParameters pubKey)
        {
            string pubKeyString;
            {
                //we need some buffer
                var sw = new System.IO.StringWriter();
                //we need a serializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, pubKey);
                //get the string from the stream
                pubKeyString = sw.ToString();
            }
            return pubKeyString;
        }

        public static RSAParameters ConvertStringToKey(string pubKeyString)
        {

            //get a stream from the string
            var sr = new System.IO.StringReader(pubKeyString);
            //we need a deserializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //get the object back from the stream
            var pubKey = (RSAParameters)xs.Deserialize(sr);
            return pubKey;

        }


        public static string Encrypt(string data, RSAParameters pubKey)
        {
            var csp = new RSACryptoServiceProvider();

            csp.ImportParameters(pubKey);


            //for encryption, always handle bytes...
            var bytesPlainTextData = Encoding.Unicode.GetBytes(data);

            //apply pkcs#1.5 padding and encrypt our data 
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

            //convert to base64 string
            var cypherText = Convert.ToBase64String(bytesCypherText);
            csp.Dispose();
            return cypherText;
        }

        public static string Decrypt(string data, RSAParameters privKey)
        {
            //first, get our bytes back from the base64 string ...
            var bytesCypherText = Convert.FromBase64String(data);

            //we want to decrypt, therefore we need a csp and load our private key
            var csp = new RSACryptoServiceProvider();

            csp.ImportParameters(privKey);

            //decrypt and strip pkcs#1.5 padding
            var bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

            //get our original plainText back...
            var plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);
            csp.Dispose();
            return plainTextData;
        }


        public static RSAParameters[] GenerateKeyPair()
        {
            //lets take a new CSP with a new 2048 bit rsa key pair
            var csp = new RSACryptoServiceProvider(2048);

            //how to get the private key
            var privKey = csp.ExportParameters(true);

            //and the public key ...
            var pubKey = csp.ExportParameters(false);

            RSAParameters[] array = [privKey, pubKey]; // make sure private key is first always please



            csp.Dispose(); // get rid of the old one
            return array;
            
        }
    }
}

