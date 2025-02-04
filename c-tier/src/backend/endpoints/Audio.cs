using c_tier.src.backend.client;
using c_tier.src.backend.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.endpoints
{
    public class Audio : Endpoint
    {
        public override string destination
        {
            get { return ".audio"; }
            set { }
        }
        public override void Route(Socket clientSocket, string receivedText, Dictionary<Socket, User> users)
        {
            ServerFrontend.Log("SERVER:" + receivedText);
            try
            {
                // Server.UpdateClientsNoAuthor(receivedText,clientSocket); // route the packets to everyone besides the author, we dont want them listening to their own voice

                Server.UpdateClientsAndAuthor(receivedText,clientSocket);
            }
            catch (Exception e)
            {
                ServerFrontend.Log("SERVER: Audio packet failed to parse");
                return;
            }




        }
    }
}
