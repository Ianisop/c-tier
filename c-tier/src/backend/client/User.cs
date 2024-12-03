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
        public User()
        {

        }
    }
}
