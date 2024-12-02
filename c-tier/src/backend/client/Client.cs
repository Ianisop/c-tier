using c_tier.src.backend.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace c_tier.src.backend.client
{
    public class Client
    {
        // Create a TCP socket
        private readonly Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPEndPoint remoteEndPoint;
        private int clientID;
        Random rnd = new Random();

        public Client()
        {
            //ServerInfo serverData = Utils.ReadFromFile<ServerInfo>("C:/Users/bocia/Documents/GitHub/c-tier/c-tier/src/secret.json"); // TODO: FIX THIS GARBAGE
            remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25366);
            clientID = DateTime.Now.TimeOfDay.GetHashCode();
        }
        

        public void Connect()
        {
            Console.WriteLine($"{Utils.YELLOW}CLIENT: TRYING CONNECTION TO SERVER.");
            clientSocket.Connect(remoteEndPoint);
            Console.WriteLine($"{Utils.GREEN}CLIENT: Connected to server");
            Speak(clientID.ToString());
            while (true)
            {
                string action = Console.ReadLine();
                Speak(action);

            }
        }

        public void Speak(string message)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(message); 
            clientSocket.Send(rawData);
        }
        

    }
}
