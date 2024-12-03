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
    internal class App
    {
        
        public Window debugWindow = new Window("Console") { X = 200, Y = 5, Width = 40, Height = Dim.Percent(30) };
        public Window channelWindow = new Window("Channels") { X = 0, Y = 0, Width = 15, Height = Dim.Percent(100) };
        public Window chatWindow = new Window("Chat") { X = 15, Y = 0, Width = 50, Height = Dim.Percent(50) };
        public Window profileWindow = new Window("Profile") { X = 80, Y = 0, Width = 20, Height = Dim.Percent(50) };
        public Window serverBrowserWindow = new Window("Server Browser") {X= 50, Y= 3, Width=20, Height = 40 };
        public TextView chatHistory = new TextView { X = 0, Y = 0, Width = 50, Height = 50,ReadOnly = true, Multiline = true, WordWrap = true,
            ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black), 
                Focus = Application.Driver.MakeAttribute(Color.Green, Color.Black), 
            }
        };
        public TextField chatInputField = new TextField {Text = "", X = 0, Y = Pos.AnchorEnd(1), Width = 50, Height = 2,
            ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Green, Color.DarkGray),
                Focus = Application.Driver.MakeAttribute(Color.Green, Color.Black), 
            }
        };
        public Label userNameLabel = new Label{Text = "username", X = 0, Y =0, Width = 18, Height = 2};
        public Label profileSeparator = new Label{Text = "------------------------", X = 0, Y = 1, Width = 18, Height = 1};
        public Label roleListLabel = new Label{Text = "Roles", X = 0, Y =2, Width = 18, Height = 1};
        public App()
        {
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black);
            Application.Top.Add(channelWindow, chatWindow, debugWindow, profileWindow, serverBrowserWindow); // add the windows

            // Add the chat history to the chat window
            chatWindow.Add(chatHistory);

            // Position the chat input field at the bottom of the chat window
           // chatInputField.Y = Pos.AnchorEnd(2); // Position it 2 rows up from the bottom of the chatWindow
            chatWindow.Add(chatInputField);

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
        
        static App app = new App();

        public static void Init()
        {
            Application.Init();
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black);
            Colors.Base.Focus = Application.Driver.MakeAttribute(Color.Green, Color.DarkGray);

            // Setup client first
            Client client = new Client();
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = true // Add this
            };
            
            client.localUser = Utils.ReadFromFile<User>("src/user_config.json",options);

            if (client.localUser == null || client == null)
            {
                Frontend.Log(Utils.RED + "Client init failed...."); 

            }
         
            app.userNameLabel.Text = client.localUser.username;
            foreach(var role in client.localUser.roles)
            {
                app.roleListLabel.Text += "\n" + role.roleName;
            }
           
            Task.Run(() => client.Connect()); // Connect to the server


            // Define the KeyPress event to trigger on Enter key press
            app.chatInputField.KeyPress += (e) =>
            {
              
                if (e.KeyEvent.Key == Key.Enter && app.chatInputField.HasFocus) // focus doesnt work
                {
                    client.Speak(app.chatInputField.Text.ToString()); // Send the message to the server
                    app.chatInputField.Text = " ";
                }
            };


            Application.Run(); // has to be the last line

        }

        

        /// <summary>
        /// Push a message to the chatWindow
        /// </summary>
        /// <param name="message"></param>
        public static void PushMessage(string message)
        {
            app.chatWindow.Text += "\n" + message;
        }

        /// <summary>
        /// Log something to the consoleWindow
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            app.debugWindow.Text = "";
            app.debugWindow.Text = message;
        }

        public static void SwitchScene()
        {

        }

    }
}
