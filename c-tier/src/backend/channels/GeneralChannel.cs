using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using c_tier.src.backend.client;
using c_tier.src.backend.server;

namespace c_tier.src.backend.channels
{
    public class GeneralChannel : Channel
    {
        public override string channelName { get; set; }
         public override int channelID { get; set; }
         public override string channelDescription { get; set; }
         public override int minRolePermLevel { get; set; }
         public override int activeMembers { get; set; }
         public override Dictionary<Socket, User> users { get; set; }
  
        public override string welcomeMessage { get; set; }


        public override void Init()
        {
            this.channelName = "General";
            this.channelDescription = "Chann";
        }


    }
}
