using c_tier.src.backend.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Terminal.Gui;
using c_tier.src.backend.server;
using Label = Terminal.Gui.Label;

namespace c_tier.src
{
    public class BackendApp
    {
        
        public Window consoleWindow = new Window("Console") { X = 200, Y = 5, Width = 40, Height = Dim.Percent(30) };
        public Window serverInfoWindow = new Window("Logs") { X = 0, Y = 0, Width = 15, Height = Dim.Percent(100) };
        public Window errorWindow = new Window("Errors") { X = 15, Y = 0, Width = 50, Height = Dim.Percent(70) };
        public Window performanceWindow = new Window("Performance") { X = 80, Y = 0, Width = 20, Height = Dim.Percent(50) };

        public Label cpuUsageLabel = new Label {};
        public Label memoryUsageLabel = new Label {};
        public Label diskUsageLabel = new Label {};
        public Label networkUsageLabel = new Label { };

        public TextView debugLogHistory= new TextView
        {
            X = 0,
            Y = 0,
            Width = 40,
            Height = 40,
            ReadOnly = true,
            Multiline = true,
            WordWrap = true,
            ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.Green, Color.Black),
            }
        };

        public TextField consoleInputField = new TextField
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = 50,
            Height = 2,
            ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Green, Color.DarkGray),
                Focus = Application.Driver.MakeAttribute(Color.Green, Color.Black),
            }
        };

       // public Button generalChannelButton = new Button {Text= "General"};
        public BackendApp()
        {
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black);
            Application.Top.Add(consoleWindow, serverInfoWindow, errorWindow, performanceWindow); // add the windows



            //Setup base widgets
            performanceWindow.Add(cpuUsageLabel, memoryUsageLabel, networkUsageLabel, diskUsageLabel);
            consoleWindow.Add(consoleInputField);
            errorWindow.Add(debugLogHistory);

          
        }


    }

    /// <summary>
    /// Static class for accesing UI in the terminal
    /// </summary>
    public static class ServerFrontend
    {
        
        public static BackendApp app = new BackendApp();
        

        public static void Init()
        {
            Application.Init();
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black);
            Colors.Base.Focus = Application.Driver.MakeAttribute(Color.Green, Color.DarkGray);

            Server server = new Server(25366, true);
            Task.Run(()=>Server.Start()); // Start the server

            // Define the KeyPress event to trigger on Enter key press
            app.consoleInputField.KeyPress += (e) =>
            {
              
                if (e.KeyEvent.Key == Key.Enter && app.consoleInputField.HasFocus && !app.consoleInputField.Text.IsEmpty) 
                {
                    Server.ProcessCommand(app.consoleInputField.Text.ToString());
                    app.consoleInputField.Text = "";
                }
            };
  

            Application.Run(); // has to be the last line

        }

        /// <summary>
        /// Log something to the consoleWindow
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            app.debugLogHistory.Text += "\n" + message;
            
        }

        public static void LogError(string message)
        {
            app.errorWindow.Text += "\n" + message;
        }

     

    }
}
