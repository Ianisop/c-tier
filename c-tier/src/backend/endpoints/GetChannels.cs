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
    public class GetChannels : Endpoint
    {
        public override string destination
        {
            get { return ".gc"; }
            set { }
        }

        public override void Route(Socket clientSocket, string receivedText, Dictionary<Socket, User> users)
        {
             ServerFrontend.Log("SERVER: Client asked for channel list!");

                // Send the channel list
                string channelNameList = "";
                foreach (Channel channel in Server.channels) channelNameList += "|" + channel.channelName;
                Server.SendResponse(clientSocket, ".CHANNELLIST" + channelNameList);
                 ServerFrontend.Log("SYSTEM: Channel list sent!");
            
        }
    }
}
