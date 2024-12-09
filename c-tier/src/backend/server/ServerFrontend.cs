
using Terminal.Gui;
using Label = Terminal.Gui.Label;

namespace c_tier.src.backend.server
{
    public class BackendApp
    {

        public Window consoleWindow = new Window("Console") { X = 40, Y = 40, Width = 40, Height = 4 };
        public Window serverInfoWindow = new Window("Logs") { X = 0, Y = 0, Width = 40, Height = Dim.Percent(100) };
        public Window errorWindow = new Window("Errors")
        {
            X = 40,
            Y = 0,
            Width = 50,
            Height = Dim.Percent(70),
            ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Red, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.Red, Color.Black),
            }
        };
        public Window performanceWindow = new Window("Performance") { X = 100, Y = 0, Width = 30, Height = Dim.Percent(100) };
        public Label cpuUsageLabel = new Label { X = 0, Y = 1, Height = 1, Width = 15 };
        public Label memoryUsageLabel = new Label { X = 0, Y = 2 };
        public Label diskUsageLabel = new Label { X = 0, Y = 3 };
        public Label networkUsageLabel = new Label { X = 0, Y = 4 };

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

        public TextField consoleInputField = new TextField
        {
            X = 0,
            Y = 1,
            Width = 40,
            Height = 20,
            ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Green, Color.DarkGray),
                Focus = Application.Driver.MakeAttribute(Color.Green, Color.Black),
            }
        };



        public BackendApp()
        {
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black);
            Application.Top.Add(consoleWindow, serverInfoWindow, errorWindow, performanceWindow); // add the windows



            //Setup base widgets
            performanceWindow.Add(cpuUsageLabel, memoryUsageLabel, networkUsageLabel, diskUsageLabel);
            consoleWindow.Add(consoleInputField);
            errorWindow.Add(debugLogHistory);
            serverInfoWindow.Add(debugLogHistory);

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

            Server server = new Server();
            Task.Run(() => Server.Start()); // Start the server

            // Define the KeyPress event to trigger on Enter key press
            app.consoleInputField.KeyPress += (e) =>
            {
                if (e.KeyEvent.Key == Key.Enter && app.consoleInputField.HasFocus && !app.consoleInputField.Text.IsEmpty)
                {
                    Server.ProcessCommand(app.consoleInputField.Text.ToString()); // process a backend command
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

        /// <summary>
        /// Updates performance metrics
        /// </summary>
        public static void UpdatePerformanceMetrics()
        {
            while (true) // TODO: find a solution for this pls theres no reason to use a whole thread only for this, right?
            {
                app.cpuUsageLabel.Text = "CPU USAGE: " + Utils.GetCpuUsage();
                app.memoryUsageLabel.Text = "MEMORY USAGE: " + Utils.GetMemoryUsage();
                app.diskUsageLabel.Text = "DISK USAGE: NaN";
                app.networkUsageLabel.Text = "NETWORK USAGE:\n" + Utils.GetNetworkUsage();
            }

        }


    }
}
