using c_tier.src.backend.client;

using System.Net.Sockets;


namespace c_tier.src.backend.server
{
    public abstract class Channel 
    {
        abstract public required string channelName { get; set; }
        abstract public required int channelID { get; set; }
        abstract public required string channelDescription { get; set; }
        abstract public required int minRolePermLevel { get; set; }
        abstract public required int activeMembers { get; set; }
        abstract public required Dictionary<Socket, User> users { get; set; }
        abstract public required string welcomeMessage { get; set; }

        public Channel(string channelName, string channelDescription, int minRolePermLevel, string welcomeMessage)
        {
            this.channelName = channelName;
            this.channelDescription = channelDescription;
            this.minRolePermLevel = minRolePermLevel;
            this.welcomeMessage = welcomeMessage;
            users = new Dictionary<Socket, User>();
        }

        /// <summary>
        /// Initializes the channel with its values
        /// </summary>
        public abstract void Init();

    }
}
