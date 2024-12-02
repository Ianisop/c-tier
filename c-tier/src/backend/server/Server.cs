using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;

namespace c_tier.src.backend.server
{
    public class Server
    {
        static bool SHOULD_DEBUG = false;
        protected static bool shouldStop = true; // Controls if the server should stop working

        private static int port = 25366; // Port number to listen on
        private static readonly IPAddress ipAddress = IPAddress.Any; // Listen on all network interfaces;
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Create a socket
        private static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Any,port);



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port"></param>
        /// <param name="debug"></param>
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
                serverSocket.Listen(1); // Backlog of 10 connections
                
            } catch(Exception e) {
                Console.WriteLine($"{Utils.RED}Something went wrong! Stopping!"  + e.Message);
            }

            Work();
        }

        public static void Stop()
        {
            serverSocket.Close();
        }

        /// <summary>
        /// This method makes the server work endlessly
        /// </summary>
        public static void Work()
        {
            try
            {
                Console.WriteLine($"{Utils.GREEN}SERVER:Listening on port {port}...");

                // Start listening for incoming connections
                serverSocket.Listen(10); // Backlog of 10 connections

                while (true)
                {
                    // Accept an incoming connection
                    Socket clientSocket = serverSocket.Accept();
                    Console.WriteLine($"{Utils.GREEN}{Utils.BOLD}SERVER:{Utils.NOBOLD} Client connected.");

                    // Receive data from the client
                    byte[] buffer = new byte[1024];
                    int receivedBytes = clientSocket.Receive(buffer);
                    string receivedText = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    Console.WriteLine($"{Utils.GREEN}SERVER: Received from client: {Utils.NORMAL} {receivedText}");

                    // Send a response back to the client
                    string responseText = "Message received!";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
                    clientSocket.Send(responseBytes);


                    Console.WriteLine("Client disconnected.");
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

        public static Socket GetSocket() { return serverSocket; }
        /// <summary>
        /// Method to check if server is alive
        /// </summary>
        /// <returns>boolean</returns>
        public static bool IsAlive() { return shouldStop ? false : true; }
        public static int GetPort() { return port; }
        public static IPAddress GetIPAddress() { return ipAddress; }


    }
}
