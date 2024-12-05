using c_tier.src.backend.server;
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
            //ServerInfo serverData = Utils.ReadFromFile<ServerInfo>("C:/Users/bocia/Documents/GitHub/c-tier/c-tier/src/secret.json");
            remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25366);
            
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

                    Frontend.Log($"Received from server: {receivedText}");

                    if (receivedText.StartsWith(".ACCOUNTOK"))
                    {
                        Frontend.Log("Account created succsefully!");
                        isSpeaking = false;
                        return true;
                   
                    }

                    //just a chat message
                    else
                    {
                        Frontend.Log("Error: " + receivedText);
                        isSpeaking = false;
                        return false;
                    }
                }

                catch (Exception ex)
                {
                    Frontend.Log($"Error receiving data: {ex.Message}");
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
            Frontend.Log("Trying socket connection...");
            clientSocket.Connect(remoteEndPoint);
            isConnected = true;
            Frontend.Log("Connection established...");
            Login(); // try logging in
            Frontend.Update();

            // Start a background task to listen for incoming messages from the server
            Task.Run(() => ReceiveMessagesFromServer());
        }

        
        private void Login()
        {
            Frontend.Log("logging in!");
            string message = ".login|" + localUser.username + "|" + localUser.password.ToString();
            Speak(message);
            Frontend.Update();
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

                    Frontend.Log($"Received from server: {receivedText}");

                    if (receivedText.StartsWith(".CHANNELLIST"))
                    {
                        string[] aux = receivedText.Split("|").Skip(1).ToArray();// Skip the ".CHANNELIST" part
                        Frontend.Log("Updating channels list");
                        Frontend.UpdateChannelList(aux);
                        isSpeaking = false;
                    }

                    if(receivedText.StartsWith(".sessiontoken"))
                    {
                        string[] aux = receivedText.Split(" ");
                        localUser.sessionToken = aux[1]; // cache the new session token
                        //Frontend.Log("SessionToken updated: " + aux[1]);
             

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
                        Frontend.CleanChat();
                        isSpeaking = false;
                    }

                    //just a chat message
                    else if(!receivedText.StartsWith('.'))
                    {
                        Frontend.PushMessage(receivedText);
                        isSpeaking = false;
                    }
                }
   
                catch (Exception ex)
                {
                    Frontend.Log($"Error receiving data: {ex.Message}");
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
            Frontend.Log($"Sending message: {message}");
            isSpeaking = false;


        }


        public string GetUsername()
        {
            return localUser.username;  
        }

    }
}
