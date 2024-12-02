using c_tier.src.backend.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace c_tier.src
{
    public static class Frontend
    {
        
        public static void Init()
        {
            //      Client client = new Client();
            // client.Connect(); // Connect to the server
            Application.Init();
            var top = Application.Top;


            var win1 = new Window("Channels") { X = 0, Y = 0, Width = 15, Height = Dim.Percent(100) };
            var win2 = new Window("Chat") { X = 50, Y = 0, Width = 50, Height = Dim.Percent(50) };

            top.Add(win1, win2);

            var label1 = new Label("Channel Name") { X = 1, Y = 1 };
            var label2 = new Label("USER: Yo!") { X = 1, Y = 1 };

            win1.Add(label1);
            win2.Add(label2);

            Application.Run();

        }
    }
}
