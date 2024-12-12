using c_tier.src.backend.server;
using c_tier.src.backend.client;


namespace c_tier.src.program
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Updater.CheckForUpdate(); // TODO: uncomment when we're actually using this
            Console.WriteLine("Select mode: Type 'server' to run as server, or 'client' to run as client.");
            string mode = Console.ReadLine()?.ToLower();
            //string mode = "client";

            if (mode == "server" || mode == "s")
            {
                Console.Clear();
                ServerFrontend.Init();
            }
            else if (mode == "client" || mode == "c")
            {
                Console.Clear();
                ClientFrontend.Init();

            }
            else
            {
                Console.WriteLine("Invalid input. Please type 'server' or 'client'.");
            }
        }

    }
}
