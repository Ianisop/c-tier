﻿using c_tier.src.backend.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Terminal.Gui;


namespace c_tier.src.backend.client
{
    public class Client
    {
        // Create a TCP socket
        private  Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPEndPoint remoteEndPoint;
        private bool isSpeaking = false;
        protected User localUser;
        public bool isConnected = false;
        

        public Client()
        {

            ServerInfo serverData = Utils.ReadFromFile<ServerInfo>("src/secret.json");
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverData.ip), serverData.port);
            
        }

       

        public void Stop()
        {
            clientSocket.Disconnect(true);
            isConnected = false;
        }

        public bool Init()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true,
            };

            User user = Utils.ReadFromFile<User>("src/user_config.json", options);

            if (user == null)
            {
                
                return false;
            }
            else
            {
                localUser = user;
                return true;
            }
       
        }

        public bool CreateAccount(string username, string password)
        {

            clientSocket.Connect(remoteEndPoint);
            isConnected = true;
            Speak(".createaccount " + username + " " + password);

            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0) break; // Server closed the connection

                    string receivedText = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                    ClientFrontend.Log($"Received from server: {receivedText}");

                    if (receivedText.StartsWith(".ACCOUNTOK"))
                    {
                        ClientFrontend.Log("Account created succsefully!");
                        isSpeaking = false;
                        return true;
                   
                    }

                    //just a chat message
                    else
                    {
                        ClientFrontend.Log("Error: " + receivedText);
                        isSpeaking = false;
                        return false;
                    }
                }

                catch (Exception ex)
                {
                    ClientFrontend.Log($"Error receiving data: {ex.Message}");
                    isSpeaking = false;
                    return false;
             
                }

            }
            return false;

        }


        public void Restart()
        {
           clientSocket.Dispose();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        }
        public void Connect()
        {
            JsonSerializerOptions options = new()
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = true
            };

            localUser.socket = clientSocket;
            ClientFrontend.Log("Trying socket connection...");
            clientSocket.Connect(remoteEndPoint);
            isConnected = true;
            ClientFrontend.Log("Connection established...");

            Login(); // try logging in
            ClientFrontend.Update();

            // Start a background task to listen for incoming messages from the server
            Task.Run(() => ReceiveMessagesFromServer());
        }

        
        private void Login()
        {
            ClientFrontend.Log("logging in!");
            string message = ".login " + localUser.username + " " + localUser.password.ToString();
            Speak(message);
            ClientFrontend.Update();
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

                    ClientFrontend.Log($"Received from server: {receivedText}");

                    if (receivedText.StartsWith(".CHANNELLIST"))
                    {
                        string[] aux = receivedText.Split("|").Skip(1).ToArray();// Skip the ".CHANNELIST" part
                        ClientFrontend.Log("Updating channels list");
                        ClientFrontend.UpdateChannelList(aux);
                        isSpeaking = false;
                    }

                    if(receivedText.StartsWith(".sessiontoken"))
                    {
                        string[] aux = receivedText.Split(" ");
                        localUser.sessionToken = aux[1]; // cache the new session token
                        ClientFrontend.Log("SessionToken updated: " + aux[1]);
             

                    }
                    if(receivedText.StartsWith(".DISCONNECT"))
                    {
                        clientSocket.Disconnect(true);
                    }
                    if(receivedText.StartsWith(".SENDTOKEN"))
                    {
                        Speak(".validate " + localUser.sessionToken);
                        //Frontend.Log("Validating session");
                        isSpeaking = false;
                    }
                    if(receivedText.StartsWith(".clear"))
                    {
                        ClientFrontend.CleanChat();
                        isSpeaking = false;
                    }

                    //just a chat message
                    else if(!receivedText.StartsWith('.'))
                    {
                        ClientFrontend.PushMessage(receivedText);
                        isSpeaking = false;
                    }
                }
   
                catch (Exception ex)
                {
                    ClientFrontend.Log($"Error receiving data: {ex.Message}");
                    isSpeaking = false;
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
            if (isSpeaking) return; // this ensures it only sends one message (not really)

            isSpeaking = true;
            byte[] rawData = Encoding.UTF8.GetBytes(message);
            clientSocket.Send(rawData);
            ClientFrontend.Log($"Sending message: {message}");
            isSpeaking = false;


        }


        public string GetUsername()
        {
            if(localUser != null) return localUser.username;
            return null;
        }

    }
}
