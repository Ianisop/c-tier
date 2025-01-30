using c_tier.src.backend.client;

using System.Net.Sockets;


namespace c_tier.src.backend.server
{
    public abstract class Channel 
    {
        abstract public  string channelName { get; set; }
        abstract public  int channelID { get; set; }
        abstract public  string channelDescription { get; set; }
        abstract public  int minRolePermLevel { get; set; }
        abstract public int activeMembers { get; set; }
        abstract public  Dictionary<Socket, User> users { get; set; }
        abstract public string welcomeMessage { get; set; }

        public Channel()
        {

        }

        /// <summary>
        /// Initializes the channel with its values
        /// </summary>
        public abstract void Init();

    }
}
