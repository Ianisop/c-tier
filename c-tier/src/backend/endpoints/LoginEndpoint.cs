using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using c_tier.src.backend.client;
using c_tier.src.backend.server;
using c_tier.src;


public class LoginEndpoint : Endpoint
{
    public readonly string destination = "login";
    public string response = "";


    public override string Route(Socket clientSocket, string receivedText, User users)
    {

        // LOGIN ENDPOINT
        if (receivedText.StartsWith(".login"))
        {
            Console.WriteLine("SYSTEM: Attempting log in request validation. | " + receivedText);

            string[] aux = receivedText.Split("|");
            string username = aux[1];
            string password = aux[2];
            Console.WriteLine("SYSTEM: Log in request for account: " + username);

            //Init validation timer
            var timer = new System.Timers.Timer(Server.sessionTokenValidationTimeout);
            var userTimer = new UserTimer();

            //Create a local user
            User newUser = new User()
            {
                username = username,
                password = password,
                socket = clientSocket,
                sessionToken = Auth.CreateSession(username, password),
                sessionValidationTimer = userTimer,


            };

            //setup default role for new user

            if (newUser.username == ServerConfigData.ownerUsername) newUser.roles.Add(Server.ownerRole);
            newUser.roles.Add(Server.GetDefaultRole());

            Console.WriteLine("SYSTEM: Gave user " + username + " role " + Server.GetDefaultRole().roleName);
            //setup validation timer for user
            newUser.sessionValidationTimer.user = newUser;
            newUser.sessionValidationTimer.timer = timer;
            newUser.sessionValidationTimer.timer.Elapsed += (sender, args) => ValidateSessionForClient(userTimer);
            newUser.sessionValidationTimer.timer.AutoReset = true;
            newUser.sessionValidationTimer.timer.Enabled = true;

            Server.users.Add(clientSocket, newUser); // Cache the user

            c_tier.src.backend.server.Channel channel = Server.channels.FirstOrDefault(); // get the default channel to send the client to

            Server.SendResponse(clientSocket, ".sessiontoken " + newUser.sessionToken); // send token
            if (newUser.MoveToChannel(channel))
            {
                Server.SendResponse(clientSocket, channel.welcomeMessage + "\n" + "You're in " + newUser.currentChannel.channelName);
            }
        }
        return response;
    }

    /// <summary>
    /// Callback method to validate user sessions
    /// </summary>
    /// <param name="userTimer"></param>
    private static void ValidateSessionForClient(UserTimer userTimer)
    {
        if (userTimer.user.validationCounter >= Server.badValidationRequestLimit)
        {
            Console.WriteLine(Utils.RED + "SYSTEM: Disconnecting client(failed to validate session)" + Utils.GREEN);
            Server.SendResponse(userTimer.user.socket, ".DISCONNECT");
            userTimer.timer.Stop();
            return;
        }
        else
        {
            Server.SendResponse(userTimer.user.socket, ".SENDTOKEN");
            Console.WriteLine("SYSTEM: asked for validation for client " + userTimer.user.username);
            userTimer.user.validationCounter++;
        }

    }
}
