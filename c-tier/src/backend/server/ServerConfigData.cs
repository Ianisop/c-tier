

namespace c_tier.src.backend.server
{
    public class ServerConfigData
    {
        public int port;
        public int max_connections;
        public int badValidationRequestLimit;
        public int sessionTokenValidationTimeout = 300000; // im ms (default 5 mins)

        // Empty constructor for the json serializer to use
        public ServerConfigData()
        {

        }
    }
}
