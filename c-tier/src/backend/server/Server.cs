using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using c_tier.src.backend.client;

namespace c_tier.src.backend.server
{
    public class Server
    {
        static bool SHOULD_DEBUG = false;
        protected static bool shouldStop = true; // Controls if the server should stop working
        private static int port = 25366; // Port number to listen on
        private static readonly IPAddress ipAddress = IPAddress.Any; // Listen on all network interfaces;
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Create a socket
        private static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        public static readonly string welcomeMessage = "System: Welcome to the server!";
        public static List<Channel> channels = new List<Channel>()
        { new Channel("General", "The Place to be!", new List<Role>(){ new Role(1, "Creator")})};

        public static readonly Dictionary<string, Action> commands = new Dictionary<string, Action>()    // Dict to hold all commands
        {
            
        };

        private static Dictionary<Socket, User> users = new Dictionary<Socket, User>();
        private static int nextClientId = 1;  // Client ID counter
        private static char commandPrefix = '/'; // Slash by default

        public Server(int targetPort, bool debug)
        {
            port = targetPort;
            SHOULD_DEBUG = debug;
        }

        public static void Start()
        {
            shouldStop = false;
            try
            {
                serverSocket.Bind(endPoint);
                serverSocket.Listen(1); // Backlog 1 connection
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Utils.RED}Something went wrong! Stopping! {e.Message}");
            }
            Console.WriteLine(Utils.GREEN + "SERVER: Running on " + ipAddress.ToString());
            Work();
        }

        public static void Stop()
        {
            serverSocket.Close();
        }

        private static void Work()
        {
            try
            {
                Console.WriteLine($"{Utils.GREEN}SERVER: Listening on port {port}...");

                // Start listening for incoming connections
                serverSocket.Listen(10); // Backlog of 10 connections

                while (true)
                {
                    // Accept an incoming connection
                    Socket clientSocket = serverSocket.Accept();

                    Console.WriteLine($"{Utils.GREEN}{Utils.BOLD}SERVER:{Utils.NOBOLD} Client connected.");

                    // Handle the client's communication asynchronously
                    Task.Run(() => HandleClientCommunication(clientSocket));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Stop();
                shouldStop = true;
            }
        }

        private static void HandleClientCommunication(Socket clientSocket)
        {
            try
            {
                // Receive data from the client
                byte[] buffer = new byte[1024];
                while (true)
                {

                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0) break; // Client disconnected

                    string receivedText = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                    //LOGIN ENDPOINT
                    if(receivedText.StartsWith(".LOGIN"))
                    {
                        Console.WriteLine("SYSTEM: Attempting log in request validation. | " + receivedText);

                        string[] aux = receivedText.Split("|");
                        string username = aux[1]; 
                        string password = aux[2]; 
                        Console.WriteLine("SYSTEM: Log in request: " + username + " | " + password);

                        User newUser = new User()
                        {
                            username = username,
                            password = password
                        };
                        users.Add(clientSocket,newUser);
                        newUser.MoveToChannel(channels.FirstOrDefault());
                        SendResponse(clientSocket, welcomeMessage);
                        SendResponse(clientSocket, "You're in " + newUser.currentChannel.channelName);

                        // Send the channel list
                        string channelNameList = "";
                        foreach (Channel channel in channels) channelNameList += "|" + channel.channelName;
                        SendResponse(clientSocket, ".CHANNELIST"+ channelNameList);
                        
                    }

                    else // if its just a message
                    {
                        CheckAndParseCommand(receivedText, clientSocket); // process a possible command

                        Console.WriteLine($"{Utils.GREEN}SERVER: Received from client : {Utils.NORMAL} {receivedText}");

                        users.TryGetValue(clientSocket, out var user); // find the user
                        UpdateClientsAndHost($"{user.username}: {receivedText}", clientSocket); // send the message
                    }

                }
            }
            catch (Exception ex)
            {   users.TryGetValue(clientSocket, out var user);
                Console.WriteLine($"Error handling client {user.username}: {ex.Message}");
            }
            finally
            {
                users.TryGetValue(clientSocket, out var user);

                // Clean up after client disconnects
                Console.WriteLine($"Client {user.username} disconnected.");
                users.Remove(clientSocket); // Remove from dictionary
                clientSocket.Close(); // Close the socket
            }
        }


        /// <summary>
        /// Updates all clients besides the message provider
        /// </summary>
        /// <param name="message"></param>
        /// <param name="clientToIgnore"></param>
        private static void UpdateClientsNoHost(string message, Socket clientToIgnore)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(message);

            foreach (Socket socket in users.Keys)
            {
                if (socket == clientToIgnore) continue; // ignore the og client when spreading the word
                socket.Send(msgBytes); // bye bye
            }
        }


        /// <summary>
        /// Sends a message in the same channel as the host
        /// </summary>
        /// <param name="message"></param>
        /// <param name="host"></param>
        private static void UpdateClientsAndHost(string message, Socket host)
        {

            byte[] msgBytes = Encoding.UTF8.GetBytes(message);

            foreach (Socket socket in users.Keys)
            {
                if (users[socket].currentChannel == users[host].currentChannel) socket.Send(msgBytes); // bye bye
            }
        }
        /// <summary>
        /// Method to create a new channel
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="rolesWithAccess"></param>
        public static bool CreateChannel(string newChannelName, string newChannelDesc, List<Role> rolesWithBaseAccess)
        {
            Channel aux = channels.Find(x => x.channelName == newChannelName); // Check if theres a channel with the same name already
            if (aux != null) return false; // channel already exists
            Channel newChannel = new Channel(newChannelName, newChannelDesc, rolesWithBaseAccess);
            channels.Add(newChannel);
            Console.WriteLine("SYSTEM: CREATED NEW CHANNEL " + newChannelName);
            return true;
        }

        /// <summary>
        /// Checks for a potential command 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="clientSocket"></param>
        /// <param name="clientId"></param>
        private static void CheckAndParseCommand(string command, Socket clientSocket)
        {
            char prefix = command[0]; // fetch the prefix
            if (prefix != commandPrefix) return;

            command = command.Substring(1); // remove the prefix
            try
            {
                if (commands.ContainsKey(command))
                {
                    commands[command](); // Execute command
                }
                else
                {
                    SendResponse(clientSocket, Utils.RED + "Invalid command."); // Misspellings, invalid perms
                }
            }
            catch (Exception ex)
            {
                SendResponse(clientSocket, Utils.RED + "Error executing command."); // Anything else lol
            }
        }

        /// <summary>
        /// method to talk to a client at a time
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="responseText"></param>
        private static void SendResponse(Socket clientSocket, string responseText)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
            clientSocket.Send(responseBytes);
        }

        public static Socket GetSocket() { return serverSocket; }

        public static bool IsAlive() { return !shouldStop; }

        public static int GetPort() { return port; }

        public static IPAddress GetIPAddress() { return ipAddress; }
    }
}
