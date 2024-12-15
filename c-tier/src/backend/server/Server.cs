using System.Net.Sockets;
using System.Net;
using System.Text;
using c_tier.src.backend.client;
using System.Data.SQLite;
using System.Security.Cryptography;



namespace c_tier.src.backend.server
{
    public class Server
    {
        protected static bool shouldStop = true; 
        private static readonly IPAddress ipAddress = IPAddress.Any; // Listen on all network interfaces;
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static List<Endpoint> endpoints = new List<Endpoint>(); // List to cache custom endpoints into
        public static List<ServerCommand> commands = new List<ServerCommand>(); // List to cache serverCommands into
        public static ServerConfigData config; // fetched from server_config.json
        protected static RSAParameters[] rsaKeys = new RSAParameters[2]; // private key at index 0, public key at index 1

        // Pair rsa public keys with socekts to be able to encrypt for each individual client
        protected static Dictionary<Socket,RSAParameters> socketKeyPairs = new Dictionary<Socket,RSAParameters>();

        public static readonly Role ownerRole = new Role() // Role to give to the owner of the server (TODO: Find a better way to do this)
        {
            roleName = "Owner",
            permLevel = 9,
        };

        public static List<Channel> channels = new List<Channel>(); // List to cache channels into


        public static List<Role> serverRoles = new List<Role>()
        {
            new Role("Member",1,"White", true),
            ownerRole
        };

        public static Dictionary<Socket, User> users = new Dictionary<Socket, User>();

        private static System.Timers.Timer validationTimer = new System.Timers.Timer();

        public Server()
        {


        }

        /// <summary>
        /// Starts up and initializes the server and its systems.
        /// </summary>
        public static void Start()
        {
            shouldStop = false;
            try
            {
                config = Utils.ReadFromFile<ServerConfigData>("src/server_config.json"); // load the server config
                string[] endpointFiles = Directory.GetFiles("src/backend/endpoints", "*.cs");
                string[] serverCommandFiles = Directory.GetFiles("src/backend/server-commands", "*.cs");

                //If theres no config data, quit
                if (config == null)
                {
                    ServerFrontend.LogError("SYSTEM: NO SERVER CONFIG FOUND. PLEASE CREATE A server_config.json FILE IN THE SOURCE (SRC) DIRECTORY.");
                }
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, config.port);

                ServerFrontend.Log(Utils.GREEN + "SYSTEM: Loaded server config...");
                ServerFrontend.Log(Utils.GREEN + "SYSTEM: Loading endpoints...");

                endpoints = Utils.LoadAndCreateInstances<Endpoint>(endpointFiles); // try some shit

                ServerFrontend.Log(Utils.GREEN + "SYSTEM: " + endpoints.Count + " endpoints loaded!");
                ServerFrontend.Log(Utils.GREEN + "SYSTEM: Loading server-commands...");

                commands = Utils.LoadAndCreateInstances<ServerCommand>(serverCommandFiles); // try some more shit

                ServerFrontend.Log(Utils.GREEN + "SYSTEM: " + commands.Count + " server-commands loaded!");

                SQLiteConnection tempdb = Database.InitDatabase("db.db");// try some other shit
                ServerFrontend.Log("SYSTEM: Found " + channels.Count + " channels, " + serverRoles.Count + " roles!");
                ServerFrontend.Log("SYSTEM: Generating RSA keyPair of size 2048...");
                rsaKeys = Utils.GenerateKeyPair();
                ServerFrontend.Log("SYSTEM: Keys generated!");
                serverSocket.Bind(endPoint);
            }
            catch (Exception e)
            {
                ServerFrontend.LogError($"Something went wrong! Stopping! {e.Message}");
            }
            ServerFrontend.Log("SERVER: Running on " + ipAddress.ToString());
            Task.Run(() => ServerFrontend.UpdatePerformanceMetrics()); // update performance labels in the backend
            Work();
        }


        public static void Stop()
        {
            serverSocket.Close();
        }


        /// <summary>
        /// makes the server work indefinetly 
        /// </summary>
        private static void Work()
        {
            try
            {
                ServerFrontend.Log($"SERVER: Listening on port {config.port}...");

                // Receive key from client
                byte[] buffer = new byte[2048];

                // Start listening for incoming connections
                serverSocket.Listen(10); // Backlog of 10 connections

                while (true)
                {
                    // Accept an incoming connection
                    Socket clientSocket = serverSocket.Accept();

                    ServerFrontend.Log($"SERVER: Client connected.");

                    Speak(clientSocket, ".key|" + Utils.ConvertKeyToString(rsaKeys[1]));

                    int receivedBytes = clientSocket.Receive(buffer);

                    string receivedText = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    ServerFrontend.Log("DATA: " + receivedText);
                    string[] aux = receivedText.Split("|");
                    socketKeyPairs[clientSocket] = Utils.ConvertStringToKey(aux[1]); // cache the key
                    Speak(clientSocket, ".KEYOK");
                    // Handle the client's communication asynchronously
                    Task.Run(() => HandleClientCommunication(clientSocket));
                }
            }
            catch (Exception ex)
            {
                ServerFrontend.LogError($"Error: {ex.Message}");
            }
            finally
            {
                Stop();
                shouldStop = true;
            }
        }

        /// <summary>
        /// Processes all backend commands that match a cached ServerCommand from the src/backend/server-commands/ directory.
        /// </summary>
        /// <param name="command"></param>
        public static void ProcessCommand(string command)
        {
            ServerFrontend.Log(command);
            string prefix = command.Split(' ')[0];
            foreach (var cmd in commands)
            {
                if (cmd.prefix == prefix)
                {
                    try
                    {
                        cmd.Execute(command);
                        break; // command found
                    }
                    catch (Exception ex)
                    {
                        ServerFrontend.LogError("Failed to execute command " + command + " because " + ex.Message);
                        break; 
                    }
                }

            }

        }


        /// <summary>
        /// Method to handle client communication asynchroniously
        /// </summary>
        /// <param name="clientSocket"></param>
        private static void HandleClientCommunication(Socket clientSocket)
        {
            try
            {
                // Receive data from the client
                byte[] buffer = new byte[2048];
                while (true)
                {
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0) break; // Client disconnected
                    string message = Convert.ToBase64String(buffer, 0, receivedBytes);
                    ServerFrontend.Log("Decrypting data");
                    string receivedText = Utils.Decrypt(message, rsaKeys[0]);
                    
                    ServerFrontend.Log("DATA: " + receivedText);
                    string[] aux = receivedText.Split(" ");

                    //route to the right endpoint
                    foreach (var endpoint in endpoints)
                    {
                        if (endpoint.destination.Equals(aux[0]))
                        {
                            endpoint.Route(clientSocket, receivedText, users);
                            ServerFrontend.Log("SERVER: Routing to endpoint " + endpoint.destination + "! (" + receivedText + ")");
                        }
                    }

                    // If it's just a message (program shouldnt reach this if a valid command has been entered and processed)
                    if (!receivedText.StartsWith('.'))
                    {
                        ServerFrontend.Log($"SERVER: Received from client: {receivedText}");
                        if (users.TryGetValue(clientSocket, out var user)) // Find the user
                            UpdateClientsAndAuthor($"{user.username}: {receivedText}", clientSocket); // Send the message
                    }
                }
            }
            catch (Exception ex)
            {
                users.TryGetValue(clientSocket, out var user);
                if (user != null) ServerFrontend.LogError($"Error handling: Client {user.username}: {ex.Message}");
                else ServerFrontend.LogError($"Error handling: Client (unknown): {ex.Message}");

            }



        }

        //TODO: REPLACE THIS BY SORTING THE ENTIRE ROLES LIST ON INIT AND PUT THE DEFAULT ROLE FIRST
        public static Role GetDefaultRole()
        {
            foreach (var i in serverRoles)
            {
                if (i.isDefault == true)
                {
                    ServerFrontend.Log("Default Role Found! : " + i.isDefault); ;
                    return i;
                }
            }
            return null;
        }

        /// <summary>
        /// Updates all clients besides the message provider
        /// </summary>
        /// <param name="message"></param>
        /// <param name="clientToIgnore"></param>
        private static void UpdateClientsNoAuthor(string message, Socket author)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(message);

            foreach (Socket socket in users.Keys)
            {
                if (socket == author) continue; // ignore the og client when spreading the word
                socket.Send(msgBytes); // bye bye
            }
        }


        /// <summary>
        /// Sends a message in the same channel as the author
        /// </summary>
        /// <param name="message"></param>
        /// <param name="host"></param>
        private static void UpdateClientsAndAuthor(string message, Socket author)
        {

            users.TryGetValue(author, out var user);

            foreach (var socket in user.currentChannel.users.Keys)
            {
                SpeakEncrypted(socket,message);
            }
        }

        /// <summary>
        /// Creates a new channel and caches it, returns true if succsesfull
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="rolesWithAccess"></param>
        public static bool CreateChannel(string newChannelName, string newChannelDesc, int minRolePermLevel, string welcomeMessage)
        {
            Channel aux = channels.Find(x => x.channelName == newChannelName); // Check if theres a channel with the same name already
            if (aux != null) return false; // channel already exists
            Channel newChannel = new Channel(newChannelName, newChannelDesc, minRolePermLevel, welcomeMessage);
            channels.Add(newChannel);
            ServerFrontend.Log("SYSTEM: CREATED NEW CHANNEL " + newChannelName);
            return true;
        }

        /// <summary>
        /// Sends a message through the server socket
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="responseText"></param>
        public static void Speak(Socket clientSocket, string responseText)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
            clientSocket.Send(responseBytes);
        }


        /// <summary>
        /// Sends an encrypted message through the server socket
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="responseText"></param>
        public static void SpeakEncrypted(Socket clientSocket, string responseText)
        {
            string encrypted = Utils.Encrypt(responseText, socketKeyPairs[clientSocket]);
            byte[] plainBytes = Encoding.UTF8.GetBytes(encrypted);
            byte[] responseBytes = Convert.FromBase64String(Convert.ToBase64String(plainBytes));
            clientSocket.Send(responseBytes);
        }

        /// <summary>
        /// Returns the first occurance of a cached user by their username
        /// </summary>
        /// <param name="username"></param>
        /// <returns>an Instance of <href>User</href></returns>
        public static User FindUserByUsername(string username)
        {
            foreach (var user in users.Values)
            {
                if (user.username == username) return user;
            }
            return null;
        }

    }
}
