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
        public List<Role> roles { get; set; }

        public Channel oldChannel, currentChannel;

        public User()
        {

        }


        /// <summary>
        /// Method to check if a user has any of the given roles
        /// </summary>
        /// <param name="requiredRoles"></param>
        /// <returns></returns>
        public bool HasRole(List<Role> requiredRoles)
        {
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
            if(currentChannel != channel && HasRole(roles)) return true;
            return false;
        }

    }
}
