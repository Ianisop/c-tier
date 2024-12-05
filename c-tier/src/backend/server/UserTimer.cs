using c_tier.src.backend.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.server
{
    public class UserTimer
    {
        public User user { get; set; }
        public System.Timers.Timer timer { get; set; }

        public UserTimer()
        {
        
        }
    }

}
