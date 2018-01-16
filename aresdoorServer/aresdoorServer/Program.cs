using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace aresdoorServer
{
    class NetworkConnection
    {
        public static void RecieveResponse(TcpClient tcpClient)
        {
            try
            {
                Byte[] bytes = new Byte[256];
                string data = null; int i;

                // Loop to receive all the data sent by the client.
                while ((i = tcpClient.GetStream().Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = Encoding.ASCII.GetString(bytes, 0, i);

                    Console.Write(data); // Return data to requesting code
                }
            } catch (Exception exc) {
                if (exc is IOException)
                { tcpClient.Close(); Thread.CurrentThread.Abort(); }
                else
                { Console.WriteLine("RecieveResponse() Exception: {0}", exc); tcpClient.Close(); Thread.CurrentThread.Abort(); }
            }
        }

        public static void SendResponse(TcpClient tcpClient)
        {
            try
            {
                while (true)
                {
                    byte[] msg = Encoding.ASCII.GetBytes(Console.ReadLine() + "\n");
                    // Send back a response.
                    tcpClient.GetStream().Write(msg, 0, msg.Length);
                }
            } catch (Exception exc) {
                if (exc is IOException)
                { tcpClient.Close(); Thread.CurrentThread.Abort(); }
                else
                { Console.WriteLine("SendResponse() Exception: {0}", exc); tcpClient.Close(); Thread.CurrentThread.Abort(); }
            }
        }

        public static void CloseTCPClient(TcpClient tcpClient)
        {
            try
            {
                tcpClient.GetStream().Close();
                tcpClient.Close();
            } catch (Exception) { tcpClient.Close(); }
        }
    }
    class Program
    {
        public static IPAddress listenHostname = IPAddress.Any;
        public static int listenPort = 9000;

        static void Main(string[] args)
        {
            TcpListener tcpServer = null;
            try
            {
                // TcpListener server = new TcpListener(port);
                tcpServer = new TcpListener(listenHostname, listenPort);

                // Start listening for client requests.
                tcpServer.Start();

                Console.CancelKeyPress += delegate
                {
                    Console.WriteLine("Caught: ^C");
                    Environment.Exit(0);
                };

                Console.WriteLine("Listening on {0}:{1}", listenHostname, listenPort);

                // Enter the listening loop.
                while (true)
                {
                    TcpClient tcpClient = tcpServer.AcceptTcpClient();
                    Console.WriteLine("Recieved connection from {0}.", ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString());

                    Thread rrThread1 = new Thread(() => NetworkConnection.RecieveResponse(tcpClient));
                    Thread srThread2 = new Thread(() => NetworkConnection.SendResponse(tcpClient));

                    rrThread1.Start();
                    srThread2.Start();
                }
            }
            catch (Exception exc)
            { Console.WriteLine("Main() Exception: {0}", exc.Message); }
            finally
            { tcpServer.Stop(); } // Stop listening for new clients.

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}
