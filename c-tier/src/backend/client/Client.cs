using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.client
{
    public class Client
    {
        // Create a TCP socket
        private readonly Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public Client()
        {
            clientSocke
        }
        

        public void Connect()
        {
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse(serverAddress), port));
        }
        

    }
}
