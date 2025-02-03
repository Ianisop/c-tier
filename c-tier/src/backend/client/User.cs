using c_tier.src.backend.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace c_tier.src.backend.client
{
    public class User
    {
        public string username { get; set; }
        public int id { get; set; }

        public string password { get; set; }
        public List<Role> roles = new List<Role>();

        public Channel oldChannel, currentChannel;
        public UserTimer sessionValidationTimer { get; set; } // Timer for session validation
        public string sessionToken { get; set; }


        public Socket socket;

        public int validationCounter;


        public User()
        {
            oldChannel = currentChannel;

        }


        /// <summary>
        /// Method to check if a user has any of the given roles
        /// </summary>
        /// <param name="requiredRoles"></param>
        /// <returns></returns>
        public bool HasRole(int minPermLevel)
        {
            foreach(var role in roles)
            {
                if(role.permLevel >= minPermLevel) return true;   
            }

            return false;
        }

        //TODO: make this method also update the ui for the client
        public void AddRole(Role role)
        {
            if(!roles.Contains(role))
                roles.Add(role);
            //ClientFrontend.Update(); // update roles list locally
        }

        /// <summary>
        /// Method to move user to channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool MoveToChannel(Channel channel)
        {
            if (channel == null) return false;
            if (currentChannel != channel && HasRole(channel.minRolePermLevel))
            { 
                oldChannel = currentChannel;
                currentChannel = channel;

                if (oldChannel !=null) oldChannel.users.Remove(socket); // remove from the old channel
                    
                channel.users.Add(socket,this); // cache the new user
                  
                channel.activeMembers++;
                if (oldChannel != null) oldChannel.activeMembers--;

                return true; 
            }
        
            return false;
        }


    }
}
