using c_tier.src.backend.server;
using System;
using System.Net.Sockets;

namespace c_tier.src.program
{
    internal class Program
    {
        Server server = new Server(25366,true);
        static void Main(string[] args)
        {
            Server.Start();
        }
    }
}