using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Terminal.Gui;
using Label = Terminal.Gui.Label;

namespace c_tier.src.backend.client
{
    public class App
    {

        public Window debugWindow = new Window("Console") { X = 200, Y = 5, Width = 40, Height = Dim.Percent(30) };
        public Window channelWindow = new Window("Channels") { X = 0, Y = 0, Width = 15, Height = Dim.Percent(100) };
        public Window chatWindow = new Window("Chat") { X = 15, Y = 0, Width = 50, Height = Dim.Percent(70) };
        public Window profileWindow = new Window("Profile") { X = 80, Y = 0, Width = 20, Height = Dim.Percent(50) };
        public Window settingsWindow = new Window("Settings") { X = 120, Y = 0, Width = 60, Height = Dim.Percent(25) };
        public Window voiceChatWindow = new Window("VoiceChat") { X = 50, Y = 0, Width = 40, Height = Dim.Percent(25),
                     ColorScheme = new ColorScheme
                     {
                         Normal = Application.Driver.MakeAttribute(Color.Red, Color.Black),
                         Focus = Application.Driver.MakeAttribute(Color.Red, Color.Black),
                     }
        };
        // public Window serverBrowserWindow = new Window("Server Browser") {X= 50, Y= 3, Width=20, Height = 40 };
        public TextField usernameTextField = new TextField { Text = "Username....", X = 0, Y = Pos.AnchorEnd(4), Width = 15, Height = 3 };
        public TextField passwordTextField = new TextField { Text = "Password....", X = 0, Y = Pos.AnchorEnd(3), Width = 15, Height = 3 };
        public Button submitButton = new Button() { Text = "Submit", X = 3, Y = 8, Width = 5, Height = 5 };
        public Button muteMicButton = new Button() { Text = "Mute", X = 1, Y = 1, Width = 2, Height = 4 };
        public Button deafenButton = new Button() { Text = "Deafen", X = 10, Y = 1, Width = 2, Height = 4 };
        public TextView chatHistory = new TextView
        {
            X = 0,
            Y = 0,
            Width = 50,
            Height = 50,
            ReadOnly = true,
            Multiline = true,
            WordWrap = true,
            ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.Green, Color.Black),
            }
        };
        public TextView debugLogHistory = new TextView
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
        public TextField chatInputField = new TextField
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
        public Label userNameLabel = new Label { Text = "username", X = 0, Y = 0, Width = 18, Height = 2 };
        public Label profileSeparator = new Label { Text = "------------------------", X = 0, Y = 1, Width = 18, Height = 1 };
        public Label roleListLabel = new Label { Text = "Roles", X = 0, Y = 2, Width = 18, Height = 1 };
        public Label activeUsersInVoiceChatLabel = new Label{ Text = "users in voicechat: 0/0" ,X=1, Y = 0, Width = 18, Height = 1 };
        public Label outputDeviceLabel = new Label{ Text = "" ,X=1, Y = 3, Width = 5, Height = 1, AutoSize = true };
        public Label inputDeviceLabel = new Label{ Text = "" ,X=1, Y = 0, Width = 5, Height = 1, AutoSize = true  };
        public Label inputLevelLabel = new Label{ Text = "|||||||||||" ,X=4, Y = 0, Width = 5, Height = 1 };


        // public Button generalChannelButton = new Button {Text= "General"};
        public App()
        {
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black);
            Application.Top.Add(voiceChatWindow, chatWindow, debugWindow, profileWindow,settingsWindow); // add the windows

            //Setup base widgets
            chatWindow.Add(chatHistory);
            chatWindow.Add(chatInputField);
            debugWindow.Add(debugLogHistory);
            voiceChatWindow.Add((muteMicButton));
            voiceChatWindow.Add((deafenButton));
            voiceChatWindow.Add(activeUsersInVoiceChatLabel,inputLevelLabel);
            settingsWindow.Add(inputDeviceLabel, outputDeviceLabel);

        }
    }

    /// <summary>
    /// Static class for accesing UI in the terminal
    /// </summary>
    public static class ClientFrontend
    {
        public static App app = new App();
        static Client client = new Client();
        private static List<string> chatTextHistory = new List<string>();
        public static void Init()
        {
            Application.Init();
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black);
            Colors.Base.Focus = Application.Driver.MakeAttribute(Color.Green, Color.DarkGray);

            SetupClient();
            // Define the KeyPress event to trigger on Enter key press
            app.chatInputField.KeyPress += (e) =>
            {
                if (e.KeyEvent.Key == Key.Enter && app.chatInputField.HasFocus && !app.chatInputField.Text.IsEmpty) // focus doesnt work
                {
                    client.SpeakEncrypted(app.chatInputField.Text.ToString()); // Send the message to the server
                    app.chatInputField.Text = "";
                }
            };

            app.submitButton.Clicked += OnAccountFormSubmit;
            Application.Run(); // has to be the last line

        }

        /// <summary>
        /// Cleans the chat history
        /// </summary>
        public static void CleanChat()
        {
            app.chatHistory.Text = "";
        }
        private static void SetupClient()
        {
            if (client.Init())
            {
                Log("Client init successful");
                Task.Run(() => client.Connect()); // Connect to the server
                app.profileWindow.Remove(app.submitButton);
                app.profileWindow.Remove(app.passwordTextField);
                app.profileWindow.Remove(app.usernameTextField);
                app.profileWindow.Add(app.userNameLabel);
                app.profileWindow.Add(app.profileSeparator);
            }
            else
            {
                Log("Client init failed");
                app.profileWindow.Add(app.submitButton);
                app.profileWindow.Add(app.passwordTextField);
                app.profileWindow.Add(app.usernameTextField);

            }
            Application.Refresh();
        }

        //Method to try the db
        public static void OnAccountFormSubmit()
        {
            string username = app.usernameTextField.Text.ToString();
            string password = app.passwordTextField.Text.ToString();

            //try creating an account
            if (client.CreateAccount(username, password))
            {
                //serialize into json, and try connecting
                User newUser = new User()
                {
                    username = username,
                    password = password
                };
                if (Utils.WriteToFile(newUser, "src/user_config.json"))
                {
                    Log("user_config.json created, logging in...");
                    client.Restart();
                    SetupClient();
                }
            };


        }

        public static int Prompt(string title, string description, params NStack.ustring[] buttons)
        {
            return MessageBox.Query(title, description, buttons);
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
                    X = 1,
                    Y = yPosition,
                };

                // Add button to the channelWindow
                app.channelWindow.Add(button);

                // Increment position for the next button
                yPosition += 2;
            }

            Log("Frontend: Updated channels list.");
            // Explicitly refresh the UI
            //Application.Refresh();
        }



        /// <summary>
        /// Push a message to the chatWindow
        /// </summary>
        /// <param name="message"></param>
        public static void PushMessage(string message)
        {
            //chatTextHistory.Add(message);
            app.chatHistory.Text += message + '\n';
            UpdateTextHistory();


        }

        /// <summary>
        /// Log something to the consoleWindow
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            app.debugLogHistory.Text += "\n" + message;

        }

        public static void UpdateTextHistory()
        {
            // Scroll to the last line
            //app.chatHistory.ScrollTo(chatTextHistory.Count - 1, true);
            //Log("SCROLLING TO: " + (chatTextHistory.Count - 1).ToString());
        }

        /// <summary>
        /// Updates profile ui elements
        /// </summary>
        public static void Update()
        {
            app.userNameLabel.Text = client.GetUsername();
        }

        public static void ChangeColorOfVoiceChatWindow(Color color)
        {
            app.voiceChatWindow.ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(color, Color.Black),
                Focus = Application.Driver.MakeAttribute(color, Color.Black),
            };
        }
        public static void UpdateInputLevel(short[] audioFrames)
        {
            int maxAmplitude = audioFrames.Max(Math.Abs);
            int level = (int)(maxAmplitude / 32767.0 * 5); // Scale from 0-5
            ClientFrontend.app.inputLevelLabel.Text = new string('|', level);
        }

    }
}
