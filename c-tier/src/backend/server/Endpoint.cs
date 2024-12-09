
using System.Net.Sockets;
using c_tier.src.backend.client;


public abstract class Endpoint
{
    abstract public string destination { get; set; }

    public Endpoint()
    {

    }

    //Each endpoint should get the same data ig
    public abstract void Route(Socket clientSocket, string receivedText, Dictionary<Socket, User> users);


}
