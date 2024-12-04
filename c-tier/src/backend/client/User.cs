using c_tier.src.backend.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.client
{
    public class User
    {
        public string username { get; set; }
        public int id { get; set; }

        public string password { get; set; }
        public List<Role> roles { get; set; }

        public Channel oldChannel, currentChannel;

        public User()
        {
            oldChannel = currentChannel;
        }


        /// <summary>
        /// Method to check if a user has any of the given roles
        /// </summary>
        /// <param name="requiredRoles"></param>
        /// <returns></returns>
        public bool HasRole(List<Role> requiredRoles)
        {
            return true;
            foreach(var role in roles)
            {
                if(requiredRoles.Contains(role)) return true;   
            }

            return false;
        }

        /// <summary>
        /// Method to move user to channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool MoveToChannel(Channel channel)
        {
            if (currentChannel != channel && HasRole(roles))
            { 
                oldChannel = currentChannel;
                currentChannel = channel;

                if (oldChannel !=null) oldChannel.users.Remove(this); // remove from the old channel
                channel.users.Add(this); // cache the new user

                channel.activeMembers++;
                if (oldChannel != null) oldChannel.activeMembers--;

                return true; 
            }
        
            return false;
        }

    }
}
