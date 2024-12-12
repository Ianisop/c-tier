using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace c_tier.src
{
    public static class Updater
    {
        public static readonly string VERSION = "0.1.0";

        // TODO: currently on test domains as the official CDN isnt running
        private static readonly string VERSIONURL = "https://cdn.rasmus.zip/latest_overlay.txt"; // "https://cdn.c-tier.com/latest_version";
        private static readonly string DOWNLOADURL = "https://cdn.rasmus.zip/c-tier.exe"; //Environment.OSVersion.Platform == PlatformID.Win32NT
           // ? "https://cdn.c-tier.com/latest_windows"
           // : "https://cdn.c-tier.con/latest_linux";


        public static void CheckForUpdate()
        {
            Console.WriteLine("Checking for updates");
            string currentExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
            if (currentExe.EndsWith(".dll")) // ignore update check when developing
            {
                Console.WriteLine("Running as a .dll. Skipping update check"); // this needs fixing later
                return;
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    string latestVersion = client.DownloadString(VERSIONURL).Trim();

                    if (string.Compare(VERSION, latestVersion) > 0)
                    {
                        Console.WriteLine("New version detected! Potentionally testing a new build");
                    }
                    else if (VERSION != latestVersion)
                    {
                        Console.WriteLine($"New update available.\nYou are running {VERSION} (newest available {latestVersion}).");
                        Console.Write("Update now ? [Y / N] : ");
                        string textinput = Console.ReadLine();

                        if (textinput?.ToUpper() == "N")
                        {
                            Console.WriteLine("Skipping update.");
                            return;
                        }
                        else if (textinput?.ToUpper() == "Y")
                        {
                            RunUpdate(currentExe);
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. Skipping update");
                        }
                    }

                    else
                    {
                        Console.WriteLine("No update available.");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not check for updates: {ex.Message}");
            }
        }

        private static void RunUpdate(string currentExe)
        {
            Console.WriteLine("Downloading update...");

            if (!currentExe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Console.WriteLine($"Manually appending .exe to {currentExe}");
                currentExe += ".exe";
            }

            string newExePath = currentExe + ".new";
            string backupPath = currentExe + ".backup";

            try
            {
                using (WebClient client = new WebClient())
                {
                    Console.WriteLine($"Downloading new executable to {newExePath}...");
                    client.DownloadFile(DOWNLOADURL, newExePath);
                }

                if (!File.Exists(newExePath))
                {
                    Console.WriteLine($"Error: New executable not found at {newExePath}.");
                    return;
                }

                if (File.Exists(backupPath))
                {
                    Console.WriteLine("Found leftover backup. Deleting...");
                    File.Delete(backupPath);
                }

                Console.WriteLine($"Paths - CurrentExe: {currentExe}, NewExePath: {newExePath}, BackupPath: {backupPath}");

                File.Move(currentExe, backupPath);
                File.Move(newExePath, currentExe);

                Console.WriteLine("Update applied. Restarting...");
                Process.Start(new ProcessStartInfo
                {
                    FileName = currentExe,
                    UseShellExecute = true
                });
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating: {ex.Message}");
                if (File.Exists(newExePath)) File.Delete(newExePath);
            }
        }
    }
}
