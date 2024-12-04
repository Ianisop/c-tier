using c_tier.src.backend.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Terminal.Gui;
using Label = Terminal.Gui.Label;

namespace c_tier.src
{
    public class App
    {
        
        public Window debugWindow = new Window("Console") { X = 200, Y = 5, Width = 40, Height = Dim.Percent(30) };
        public Window channelWindow = new Window("Channels") { X = 0, Y = 0, Width = 15, Height = Dim.Percent(100) };
        public Window chatWindow = new Window("Chat") { X = 15, Y = 0, Width = 50, Height = Dim.Percent(50) };
        public Window profileWindow = new Window("Profile") { X = 80, Y = 0, Width = 20, Height = Dim.Percent(50) };
        // public Window serverBrowserWindow = new Window("Server Browser") {X= 50, Y= 3, Width=20, Height = 40 };
        public Window profileEditor = new Window("Profile Editor") { X = 50, Y = 15, Width = 50, Height = Dim.Percent(50) };
        public TextField usernameTextField = new TextField {Text="Username....", X=Pos.AnchorEnd(1), Y=Pos.AnchorEnd(2),Width=35,Height=3 };
        public TextField passwordTextField = new TextField {Text="Password....", X=Pos.AnchorEnd(1), Y=Pos.AnchorEnd(4),Width=35,Height=3 };
        public TextView chatHistory = new TextView { X = 0, Y = 0, Width = 50, Height = 50,ReadOnly = true, Multiline = true, WordWrap = true,
            ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black), 
                Focus = Application.Driver.MakeAttribute(Color.Green, Color.Black), 
            }
        };
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
        public TextField chatInputField = new TextField {X = 0, Y = Pos.AnchorEnd(1), Width = 50, Height = 2,
            ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Green, Color.DarkGray),
                Focus = Application.Driver.MakeAttribute(Color.Green, Color.Black), 
            }
        };
        public Label userNameLabel = new Label{Text = "username", X = 0, Y =0, Width = 18, Height = 2};
        public Label profileSeparator = new Label{Text = "------------------------", X = 0, Y = 1, Width = 18, Height = 1};
        public Label roleListLabel = new Label{Text = "Roles", X = 0, Y =2, Width = 18, Height = 1};
       // public Button generalChannelButton = new Button {Text= "General"};
        public App()
        {
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black);
            Application.Top.Add(channelWindow, chatWindow, debugWindow, profileWindow); // add the windows

            // Add the chat history to the chat window
            chatWindow.Add(chatHistory);

            profileEditor.Add(usernameTextField, passwordTextField);
            chatWindow.Add(chatInputField);
            debugWindow.Add(debugLogHistory);
            //channelWindow.Add(generalChannelButton);
            profileWindow.Add(userNameLabel);
            profileWindow.Add(profileSeparator);
            profileWindow.Add(roleListLabel);
        }


    }

    /// <summary>
    /// Static class for accesing UI in the terminal
    /// </summary>
    public static class Frontend
    {
        
        public static App app = new App();
        static Client client = new Client();
        public static void Init()
        {
            Application.Init();
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black);
            Colors.Base.Focus = Application.Driver.MakeAttribute(Color.Green, Color.DarkGray);

            Task.Run(() => client.Connect()); // Connect to the server
                                         

            // Define the KeyPress event to trigger on Enter key press
            app.chatInputField.KeyPress += (e) =>
            {
              
                if (e.KeyEvent.Key == Key.Enter && app.chatInputField.HasFocus && !app.chatInputField.Text.IsEmpty) // focus doesnt work
                {
                    client.Speak(app.chatInputField.Text.ToString()); // Send the message to the server
                    app.chatInputField.Text = "";
                }
            };




            Application.Run(); // has to be the last line
        }

        public static void UpdateChannelList(string[] channelNames)
        {
            // Clear existing buttons
            app.channelWindow.Clear();

            // Starting Y position for the first button
            int yPosition = 0;

            // Add each channel name as a button
            foreach (string channelName in channelNames)
            {
                var button = new Button
                {
                    Text = channelName,
                    X = 1,          // Indent for visual appeal
                    Y = yPosition,  // Dynamic vertical position
                };

                // Add button to the channelWindow
                app.channelWindow.Add(button);

                // Increment position for the next button
                yPosition += 2; // Adjust for spacing (e.g., 2 rows per button)
            }

            Frontend.Log("Frontend: Updated channels list.");
            // Explicitly refresh the UI
            //Application.Refresh();
        }



        /// <summary>
        /// Push a message to the chatWindow
        /// </summary>
        /// <param name="message"></param>
        public static void PushMessage(string message)
        {
            app.chatHistory.Text += message + '\n';
        }

        /// <summary>
        /// Log something to the consoleWindow
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            app.debugLogHistory.Text += "\n" + message;
        }

        public static void SwitchScene()
        {
            Application.Top.Add(app.profileEditor);
        }

        public static void Update()
        {
            app.userNameLabel.Text = client.GetUsername();
        }
     

    }
}
