using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using c_tier.src.backend.client;
using System.Security.Cryptography.X509Certificates;
using System.Data.SQLite;
using System.Drawing.Printing;
using System.Reflection;
using System.Timers;
using System.Threading;
using System.Data;

public abstract class Endpoint
{
    abstract public string destination { get; set; } 

    public Endpoint()
    {

    }

    //Each endpoint should get the same data ig
    public abstract void Route(Socket clientSocket, string receivedText, Dictionary<Socket, User> users);


}
