using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
        public static readonly Dictionary<string, Action> commands = new Dictionary<string, Action>()    // Dict to hold all commands
        {

        };

        private static Dictionary<int, Socket> connectedClients = new Dictionary<int, Socket>();
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
                    int clientId = nextClientId++; // Generate a unique ID for the client
                    connectedClients[clientId] = clientSocket; // Store client in dictionary

                    Console.WriteLine($"{Utils.GREEN}{Utils.BOLD}SERVER:{Utils.NOBOLD} Client {clientId} connected.");

                    // Handle the client's communication asynchronously
                    Task.Run(() => HandleClientCommunication(clientSocket, clientId));
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

        private static void HandleClientCommunication(Socket clientSocket, int clientId)
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
                    CheckAndParseCommand(receivedText, clientSocket, clientId); // process a possible command

                    Console.WriteLine($"{Utils.GREEN}SERVER: Received from client {clientId}: {Utils.NORMAL} {receivedText}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client {clientId}: {ex.Message}");
            }
            finally
            {
                // Clean up after client disconnects
                Console.WriteLine($"Client {clientId} disconnected.");
                connectedClients.Remove(clientId); // Remove from dictionary
                clientSocket.Close(); // Close the socket
            }
        }

        private static void DisconnectClient(int clientId)
        {
            if (connectedClients.ContainsKey(clientId))
            {
                Socket clientSocket = connectedClients[clientId];
                byte[] responseBytes = Encoding.UTF8.GetBytes("You have been disconnected.");
                clientSocket.Send(responseBytes); // Notify client
                clientSocket.Close(); // Close the connection
                connectedClients.Remove(clientId); // Remove from connected clients
                Console.WriteLine($"Client {clientId} has been disconnected.");
            }
        }

        private static void UpdateClients(string message)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(message);
            foreach (Socket socket in connectedClients.Values)
            {
                socket.Send(msgBytes);
            }
        }

        private static void CheckAndParseCommand(string command, Socket clientSocket, int clientId)
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
                    SendResponse(clientSocket, Utils.RED + "Invalid command.");
                }
            }
            catch (Exception ex)
            {
                SendResponse(clientSocket, Utils.RED + "Error executing command.");
            }
        }

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
