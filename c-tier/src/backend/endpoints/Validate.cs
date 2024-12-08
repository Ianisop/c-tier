using c_tier.src.backend.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using c_tier.src.backend.server;

namespace c_tier.src.backend.endpoints
{
    public class Validate : Endpoint
    {
        public override string destination
        {
            get { return ".validate"; }
            set { }
        }

        public override void Route(Socket clientSocket, string receivedText, Dictionary<Socket, User> users)
        {
            string[] aux = receivedText.Split(' ');
            string token = aux[1];
            // Perform token validation logic
            if (users.TryGetValue(clientSocket, out User targetUser) && targetUser.sessionToken == token)
            {
                ServerFrontend.Log("SYSTEM: Token for " + targetUser.username + " validated successfully.");
                targetUser.validationCounter--;

            }
            else
            {
                ServerFrontend.Log("SYSTEM: Invalid token. Disconnecting client.");
                Server.SendResponse(clientSocket, "Error: Invalid session token.");
                clientSocket.Disconnect(true);

            }

        }
    }
}
