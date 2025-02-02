using c_tier.src.backend.client;
using c_tier.src.backend.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.endpoints
{
    public class Audio : Endpoint
    {
        public override string destination
        {
            get { return ".audio"; }
            set { }
        }
        public override void Route(Socket clientSocket, string receivedText, Dictionary<Socket, User> users)
        {
            ServerFrontend.Log("SERVER: Audio Packet");
            string[] aux = receivedText.Split("|");
            short[] frames;
            try
            {
                frames = aux[1].Split(",").Select(short.Parse).ToArray();
                ServerFrontend.Log("SERVER: Audio packet parsed, sampleCount: " + frames.Length);
            }
            catch (Exception e)
            {
                ServerFrontend.Log("SERVER: Audio packet failed to parse");
                return;
            }




        }
    }
}
