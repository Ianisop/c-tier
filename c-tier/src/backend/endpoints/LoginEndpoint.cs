using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using c_tier.src.backend.client;
using c_tier.src.backend.server;


public class LoginEndpoint : Endpoint
{
    public override string destination
    {
        get { return ".login"; }
        set { }
    }



    public override void Route(Socket clientSocket, string receivedText, Dictionary<Socket, User> users)
    {

        ServerFrontend.Log("SYSTEM: Attempting log in request validation. | " + receivedText);

        //early check if theres a fuck up and the socket is already cached
        if (Server.users.ContainsKey(clientSocket))
        {
            ServerFrontend.Log("SYSTEM: User already logged in for this socket.");
            return;
        }

        string[] aux = receivedText.Split(" ");
        string username = aux[1];
        string password = aux[2];
        ServerFrontend.Log("SYSTEM: Log in request for account: " + username);

        //Init validation timer
        var timer = new System.Timers.Timer(Server.config.sessionTokenValidationTimeout);
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
        ServerFrontend.Log("SERVER: user created for " + username);
        //setup default role for new user
        newUser.roles.Add(Server.GetDefaultRole());
        // Console.WriteLine("SYSTEM: Gave user " + username + " role " + Server.GetDefaultRole().roleName);
        //setup validation timer for user
        newUser.sessionValidationTimer.user = newUser;
        newUser.sessionValidationTimer.timer = timer;
        newUser.sessionValidationTimer.timer.Elapsed += (sender, args) => ValidateSessionForClient(userTimer);
        newUser.sessionValidationTimer.timer.AutoReset = true;
        newUser.sessionValidationTimer.timer.Enabled = true;

        Server.users.Add(clientSocket, newUser); // Cache the user

        Channel channel = Server.channels[0]; // get the default channel to send the client to
        ServerFrontend.Log("SERVER: sending " + username + " to " + channel.channelName);
        Server.SpeakEncrypted(clientSocket, ".sessiontoken " + newUser.sessionToken); // send token
        if (channel != null && newUser.MoveToChannel(channel))
        {
            Server.SpeakEncrypted(clientSocket, channel.welcomeMessage + "\n" + "You're in " + newUser.currentChannel.channelName);
        }


    }

    /// <summary>
    /// Callback method to validate user sessions
    /// </summary>
    /// <param name="userTimer"></param>
    private static void ValidateSessionForClient(UserTimer userTimer)
    {
        if (userTimer.user.validationCounter >= Server.config.badValidationRequestLimit)
        {
            ServerFrontend.Log("SYSTEM: Disconnecting client(failed to validate session)");
            Server.SpeakEncrypted(userTimer.user.socket, ".DISCONNECT");
            userTimer.timer.Stop();
            return;
        }
        else
        {
            Server.SpeakEncrypted(userTimer.user.socket, ".SENDTOKEN");
            ServerFrontend.Log("SYSTEM: asked for validation for client " + userTimer.user.username);
            userTimer.user.validationCounter++;
        }

    }
}
