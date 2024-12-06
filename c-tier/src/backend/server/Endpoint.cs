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
    public readonly string destination = ""; // readonly bcs this shouldnt be changeable at runtime
    public string response = "";
    public Endpoint()
    {

    }

    //Each endpoint should get the same data ig
    public virtual string Route(Socket clientSocket, string receivedText, User users)
    {
        return response;
    }

}
