using c_tier.src.backend.server;
using Pv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Terminal.Gui;


namespace c_tier.src.backend.client
{
    public class Client
    {
        // Create a TCP socket
        private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPEndPoint remoteEndPoint;
        private bool isSpeaking = false;
        protected User localUser;
        public bool isConnected = false;
        public RSAParameters[] rsaKeys;
        public string serverPubKey;

        public Client()
        {
            ServerInfo serverData = Utils.ReadFromFile<ServerInfo>("src/secret.json");
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverData.ip), serverData.port);
            rsaKeys = Utils.GenerateKeyPair();
        }



        public void Stop()
        {
            clientSocket.Disconnect(true);
            isConnected = false;
        }

        public bool Init()
        {
            try
            {
                User user = Utils.ReadFromFile<User>("src/user_config.json");

                if (user == null)
                {
                    ClientFrontend.Log("user_config.json not found");
                    return false;
                }
                else
                {
                    localUser = user;
                    AudioManager.Init(); // initialize the audio shit
                    ClientFrontend.Log("AudioManager: good!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                ClientFrontend.Log("Something went wrong! " + ex.Message);
                return false;
            }


        }

        public bool CreateAccount(string username, string password)
        {
            clientSocket.Connect(remoteEndPoint);
            isConnected = true;

            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    int receivedBytes = clientSocket.Receive(buffer);
                    //if (receivedBytes == 0) break; // Server closed the connection

                    string receivedText = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                    ClientFrontend.Log($"Received from server: {receivedText}");
                

                    if (receivedText.StartsWith(".KEYOK"))
                    {
                        SpeakEncrypted(".createaccount " + username + " " + password);
                        return true;
                    }
                    if (receivedText.StartsWith(".key"))
                    {
                        string[] tokens = receivedText.Split("|");
                        serverPubKey = tokens[1];
                        var data = ".key|" + Utils.ConvertKeyToString(rsaKeys[1]);
                        byte[] rawData = Encoding.UTF8.GetBytes(data);
                        clientSocket.Send(rawData);
                        ClientFrontend.Log("Sent key back!");

                    }

                    if (receivedText.StartsWith(".ACCOUNTOK"))
                    {
                        ClientFrontend.Log("Account created succsefully!");
                        return true;

                    }
                }

                catch (Exception ex)
                {
                    ClientFrontend.Log($"Error receiving data: {ex.Message}");
                    isSpeaking = false;
                    return false;

                }

            }

        }

        public void StreamAudio()
        {
            AudioManager.recorder.Start();
            ClientFrontend.ChangeColorOfVoiceChatWindow(Color.Green);

            byte[] audioData;
            int chunkSize = 2024; // Define the chunk size (e.g., 256 bytes)

            while (AudioManager.recorder.IsRecording)
            {
                short[] frames = AudioManager.recorder.Read();
                AudioManager.Play(frames);  // Play the chunk using the speaker immediately
                //ClientFrontend.Log("Recording into frames array!");

                // Convert the short[] to a byte[] (for transmission)
                audioData = Utils.ShortArrayToByteArray(frames);
                //ClientFrontend.Log("Starting to separate into chunks!");

                // Send each chunk separately and immediately play it
                for (int i = 0; i < audioData.Length; i += chunkSize)
                {
                    int currentChunkSize = Math.Min(chunkSize, audioData.Length - i);
                    byte[] chunk = new byte[currentChunkSize];
                    Array.Copy(audioData, i, chunk, 0, currentChunkSize);
                    //ClientFrontend.Log("Rebuilding arrays!");

                    // Convert chunk to Base64 string
                    string base64Chunk = Convert.ToBase64String(chunk);

                    // Send the chunk as a string (with .audio| prefix)
                    Speak(".audio|" + base64Chunk);
                    //ClientFrontend.Log("Sending chunk: " + base64Chunk);

                  
                    
                }
                
                // Update input level (optional UI update)
                ClientFrontend.UpdateInputLevel(frames);
            }

            AudioManager.recorder.Stop();
            ClientFrontend.ChangeColorOfVoiceChatWindow(Color.Red);
        }





        public void Restart()
        {
            clientSocket.Dispose();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Connect()
        {
            localUser.socket = clientSocket;
            ClientFrontend.Log("Trying socket connection...");
            try { clientSocket.Connect(remoteEndPoint); }
            catch (Exception ex) { };

            isConnected = true;
            ClientFrontend.Log("Connection established...");

            byte[] buffer = new byte[1024];

            while (true)
            {
                int receivedBytes = clientSocket.Receive(buffer);
                if (receivedBytes == 0) break; // Server closed the connection
                string receivedText = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                ClientFrontend.Log($"Received from server: {receivedText}");
                if(receivedText.StartsWith(".KEYOK"))
                {
                    Login();
                    break;
                }
                if (receivedText.StartsWith(".key|"))
                {
                    string[] tokens = receivedText.Split("|");
                    serverPubKey = tokens[1];
                    var data = ".key|" + Utils.ConvertKeyToString(rsaKeys[1]);
                    byte[] rawData = Encoding.UTF8.GetBytes(data);
                    clientSocket.Send(rawData);
                    ClientFrontend.Log($"Sending message: {data}");
                    ClientFrontend.Log("Sent key back!");
                    
                }

            }

            ClientFrontend.Update();

            // Start a background task to listen for incoming messages from the server
            Task.Run(() => ReceiveMessagesFromServer());
        }


        private void Login()
        {
            ClientFrontend.Log("logging in!");
            string message = ".login " + localUser.username + " " + localUser.password.ToString();
            SpeakEncrypted(message);
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
                    ClientFrontend.Log("Received data from server.");
                    string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    ClientFrontend.Log("Received message from server: " + message);
                    string receivedText = Utils.Decrypt(message, rsaKeys[0]); // decrypt

                    ClientFrontend.Log($"Received from server: {receivedText}");

                    if (receivedText.StartsWith(".CHANNELLIST"))
                    {
                        string[] aux = receivedText.Split("|").Skip(1).ToArray();// Skip the ".CHANNELIST" part
                        ClientFrontend.Log("Updating channels list");
                        ClientFrontend.UpdateChannelList(aux);
                        isSpeaking = false;
                    }

                    if (receivedText.StartsWith(".sessiontoken"))
                    {
                        string[] aux = receivedText.Split(" ");
                        localUser.sessionToken = aux[1]; // cache the new session token
                        ClientFrontend.Log("SessionToken updated: " + aux[1]);


                    }
                    if (receivedText.StartsWith(".DISCONNECT"))
                    {
                        clientSocket.Disconnect(true);
                    }
                    if (receivedText.StartsWith(".SENDTOKEN"))
                    {
                        SpeakEncrypted(".validate " + localUser.sessionToken);
                        //Frontend.Log("Validating session");
                        isSpeaking = false;
                    }
                    if (receivedText.StartsWith(".startaudio"))
                    {
                        ClientFrontend.Log("Starting audio streaing");
                        StreamAudio();
                    }
                    if (receivedText.StartsWith(".stopaudio"))
                    {
                        AudioManager.recorder.Stop();
                        AudioManager.speaker.Stop();
                    }

                    if (receivedText.StartsWith(".clear"))
                    {
                        ClientFrontend.CleanChat();
                        isSpeaking = false;
                    }
                    if (receivedText.StartsWith(".audio"))
                    {
                        string[] aux = receivedText.Split("|");
                        byte[] byteSamples;
                        try
                        {
                            byteSamples = Convert.FromBase64String(aux[1]); // Decode from Base64
                        }
                        catch (FormatException ex)
                        {
                            ClientFrontend.Log("Error decoding Base64 audio: " + ex.Message);
                            return;
                        }

                        short[] shortSamples = Utils.ByteArrayToShortArray(byteSamples); // Convert bytes to short[]
                        ClientFrontend.Log("PlayingAudio" + shortSamples);
                        AudioManager.Play(shortSamples); // Play audio
                    }




                    //just a chat message
                    else if (!receivedText.StartsWith('.'))
                    {
                        ClientFrontend.PushMessage(receivedText);
                        isSpeaking = false;
                    }
                }

                catch (Exception ex)
                {
                    ClientFrontend.Log($"Error receiving data: {ex.Message} + {ex.StackTrace}");
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


        /// <summary>
        /// Method to send an encrypted message to the server
        /// </summary>
        /// <param name="message"></param>
        public void SpeakEncrypted(string message)
        {
            if (isSpeaking) return; // this ensures it only sends one message (not really)

            isSpeaking = true;
            var encrypted = Utils.Encrypt(message, Utils.ConvertStringToKey(serverPubKey));
            byte[] rawData = Convert.FromBase64String(encrypted);
            clientSocket.Send(rawData);
            ClientFrontend.Log($"Sending encrypted message: {message}");
            isSpeaking = false;


        }
        public string GetUsername()
        {
            if (localUser != null) return localUser.username;
            return null;
        }

    }
}
