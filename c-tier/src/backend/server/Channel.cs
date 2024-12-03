using c_tier.src.backend.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.server
{
    public class Channel
    {
        public string channelName;
        public int channelID;
        public string channelDescription;
        public List<Role> rolesWithAccess;
        public int activeMembers;
        public Socket socket;


        public Channel(string channelName, string channelDescription, List<Role> rolesWithAcess)
        {
            this.channelName = channelName;
            this.channelDescription = channelDescription;
            this.rolesWithAccess = rolesWithAcess;
          
        }


    }
}
