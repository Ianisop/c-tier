﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

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


        /// <summary>
        /// Serializes json into object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns> :shrug: </returns>
        public static T ReadFromFile<T>(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading or deserializing file: {ex.Message}");
                return default;
            }
        }

    }
}


