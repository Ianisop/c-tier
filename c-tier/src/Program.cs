using c_tier.src.backend.server;
using c_tier.src.backend.client;
using System;

namespace c_tier.src.program
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Select mode: Type 'server' to run as server, or 'client' to run as client.");
             string mode = Console.ReadLine()?.ToLower();
            //string mode = "client";

            if (mode == "server")
            {
                Console.WriteLine("Starting in server mode...");
                Server server = new Server(25366, true);
                Server.Start(); // Start the server
            }
            else if (mode == "client")
            {
                Console.Clear();
                Frontend.Init();
             
            }
            else
            {
                Console.WriteLine("Invalid input. Please type 'server' or 'client'.");
            }
        }

    }
}
