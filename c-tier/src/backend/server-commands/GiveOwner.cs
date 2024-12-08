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


        public override void Execute(string input)
        {
            string[] aux = input.Split(' ');
            string username = aux[1];
            User targetUser = Server.FindUserByUsername(username);
            if(targetUser != null)
            {
                targetUser.AddRole(Server.ownerRole);
                ServerFrontend.LogToConsole("Command " + input + " executed!");
            }
            else
            {
                ServerFrontend.LogError("Error: Command " + input + " failed to execute");
            }
            
        }
    }
}
