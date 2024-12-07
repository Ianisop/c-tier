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
    public class CreateAccount : Endpoint
    {
        public override string destination
        {
            get { return ".createaccount"; }
            set { }
        }


        public override void Route(Socket clientSocket, string receivedText, Dictionary<Socket, User> users)
        {
            Console.WriteLine(Utils.GREEN + "SERVER: Account creation request");

            //validate data
            string[] aux = receivedText.Split(" ");

            string username = aux[1];
            string password = aux[2];

            var user_id = Database.CreateUser(username, password);
            if (user_id == 0) Server.SendResponse(clientSocket, "Account request failed");
            else
            {
                Server.SendResponse(clientSocket, ".ACCOUNTOK");
                Console.WriteLine("SERVER: Account created for user " + username);
                Task.Run(() => clientSocket.Disconnect(true));
                Console.WriteLine("SERVER: Disconnecting client " + username);

            }

        }
    }
}
