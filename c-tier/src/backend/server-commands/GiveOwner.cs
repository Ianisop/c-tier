using c_tier.src.backend.client;
using c_tier.src.backend.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.commands
{
    public class GiveOwner : ServerCommand
    {
        public override string prefix
        {
            get { return "giveowner"; }
            set { }
        }

        //do stuff
        public override void Execute(string input)
        {
            string[] aux = input.Split(' ');
            string username = aux[1];

            try
            {
                User targetUser = Server.FindUserByUsername(username);
                targetUser.AddRole(Server.ownerRole);
                ServerFrontend.LogToConsole("Command " + input + " executed!");
            } catch(Exception ex) //in case anything goes wrong
            {
                ServerFrontend.LogError("Failed to execute " + input + " because " + ex.Message);
            }
            
        }
    }
}
