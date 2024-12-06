using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using c_tier.src.backend.client;
using c_tier.src.backend.server;
using System.Security.Cryptography.X509Certificates;
using System.Data.SQLite;
using System.Drawing.Printing;
using System.Reflection;
using System.Timers;
using System.Threading;
using System.Data;

public class LoginEndpoint : Endpoint
{
    public readonly string destination = "login";
    public string response = "";


    public override string Route(Socket clientSocket, string receivedText, User users)
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


        newUser.roles.Add(Server.GetDefaultRole());

        Console.WriteLine("SYSTEM: Gave user " + username + " role " + Server.GetDefaultRole().roleName);
        //setup validation timer for user
        newUser.sessionValidationTimer.user = newUser;
        newUser.sessionValidationTimer.timer = timer;
        newUser.sessionValidationTimer.timer.Elapsed += (sender, args) => ValidateSessionForClient(userTimer);
        newUser.sessionValidationTimer.timer.AutoReset = true;
        newUser.sessionValidationTimer.timer.Enabled = true;

        Server.users.Add(clientSocket, newUser); // Cache the user



        Server.SendResponse(clientSocket, ".sessiontoken " + newUser.sessionToken); // send token
        if (newUser.MoveToChannel(Server.channels.FirstOrDefault()))
        {
            Server.SendResponse(clientSocket, Server.welcomeMessage + "\n" + "You're in " + newUser.currentChannel.channelName);
        }
        return response;
    }
}
