using c_tier.src.backend.client;
using c_tier.src.backend.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.server_commands
{
    public class SendMessage : ServerCommand
    {
        public override string prefix
        {
            get { return "sendmsg"; }
            set { }
        }

        //do stuff
        public override void Execute(string input)
        {
            string[] aux = input.Split(' ');
            string username = aux[1];
            string message = aux[2];

            try
            {
                Server.SpeakEncrypted(Server.FindUserByUsername(username).socket, message);
                ServerFrontend.Log("Command " + input + " executed!");
            }
            catch (Exception ex) //in case anything goes wrong
            {
                ServerFrontend.LogError("Failed to execute " + input + " because " + ex.Message);
            }

        }

    }
}
