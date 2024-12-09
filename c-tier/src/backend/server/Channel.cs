using c_tier.src.backend.client;

using System.Net.Sockets;


namespace c_tier.src.backend.server
{
    public class Channel
    {
        public string channelName;
        public int channelID;
        public string channelDescription;
        public int minRolePermLevel = 1;
        public int activeMembers;
        public Dictionary<Socket, User> users = new Dictionary<Socket, User>();
        public string welcomeMessage;


        public Channel(string channelName, string channelDescription, int minRolePermLevel, string welcomeMessage)
        {
            this.channelName = channelName;
            this.channelDescription = channelDescription;
            this.minRolePermLevel = minRolePermLevel;
            this.welcomeMessage = welcomeMessage;


        }



    }
}
