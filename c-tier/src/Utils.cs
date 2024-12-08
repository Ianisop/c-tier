﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Security.Policy;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net.NetworkInformation;


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

        private static readonly char[] DefaultCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray(); // char set for string generation
        /// <summary>
        /// Serializes json into object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns> :shrug: </returns>
        public static T ReadFromFile<T>(string filePath, JsonSerializerOptions options)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(jsonContent, options);
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


