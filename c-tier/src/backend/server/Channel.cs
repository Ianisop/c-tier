﻿using c_tier.src.backend.client;
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
        public int minRolePermLevel;
        public int activeMembers;
        public Dictionary<Socket,User> users = new Dictionary<Socket,User>();


        public Channel(string channelName, string channelDescription, int minRolePermLevel)
        {
            this.channelName = channelName;
            this.channelDescription = channelDescription;
            this.minRolePermLevel = minRolePermLevel;
           
          
        }

        

    }
}
