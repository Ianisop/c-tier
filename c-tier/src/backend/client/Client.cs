﻿using c_tier.src.backend.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace c_tier.src.backend.client
{
    public class Client
    {
        // Create a TCP socket
        private readonly Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPEndPoint remoteEndPoint;

        public Client()
        {
            //ServerInfo serverData = Utils.ReadFromFile<ServerInfo>("C:/Users/bocia/Documents/GitHub/c-tier/c-tier/src/secret.json"); // TODO: FIX THIS GARBAGE
            remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25366);

        }


        public void Connect()
        {
            Console.WriteLine($"{Utils.YELLOW}CLIENT: TRYING CONNECTION TO SERVER.");
            clientSocket.Connect(remoteEndPoint);
            Console.WriteLine($"{Utils.GREEN}CLIENT: Connected to server");

            // Start a background task to listen for incoming messages from the server
            Task.Run(() => ReceiveMessagesFromServer());

            // Receive user input and send to server
            while (true)
            {
                string action = Console.ReadLine(); // get input
                Speak(action); // send input
            }
        }

        /// <summary>
        /// Background task to handle responses from the server
        /// </summary>
        private void ReceiveMessagesFromServer()
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0) break; // Server closed the connection

                    string receivedText = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    Console.WriteLine($"Received from server: {receivedText}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving data: {ex.Message}");
                    break;
                }
            }
        }


        /// <summary>
        /// Method to send a message to the server
        /// </summary>
        /// <param name="message"></param>
        public void Speak(string message)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(message); 
            clientSocket.Send(rawData);
        }
        

    }
}