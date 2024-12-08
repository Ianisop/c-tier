using c_tier.src.backend.client;
using c_tier.src.backend.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.endpoints
{
    public class MoveChannel : Endpoint
    {
        public override string destination
        {
            get { return ".mc"; }
            set { }
        }

        public override void Route(Socket clientSocket, string receivedText, Dictionary<Socket, User> users)
        {
            Console.WriteLine(Utils.GREEN + $"SYSTEM: Attempting channel moving");

            string channelName = receivedText.Substring(4);
                

                // Try to get the user associated with the clientSocket
                if (users.TryGetValue(clientSocket, out var user))
                {
                    // Find the channel by name
                    var channel = Server.channels.Find(a => a.channelName == channelName);
                    if (channel != null)
                    {
                        if (user.MoveToChannel(channel))
                        {
                            Console.WriteLine(Utils.GREEN + $"SYSTEM: Moving client to channel {channelName}");
                            Server.SendResponse(clientSocket, ".clear"); // clear the chatlog
                            Server.SendResponse(clientSocket, $"{user.currentChannel.welcomeMessage}\n Hopped to {user.currentChannel.channelName}"); // success
                        }
                        else
                        {
                            Server.SendResponse(clientSocket, "Error: Failed to join channel.");
                        }
                    }
                    else
                    {
                        Server.SendResponse(clientSocket, $"Error: Channel '{channelName}' not found.");
                    }
                }
                else
                {
                    Server.SendResponse(clientSocket, "Error: User not found.");
                }
        }
    }
}
