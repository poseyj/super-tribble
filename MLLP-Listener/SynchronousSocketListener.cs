using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MLLP_Listener
{
    public static class SynchronousSocketListener
    {
        private static string data = null;

        private static void StartListening()
        {
            var ipHostInfo = Dns.Resolve(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, 11000);

            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Socket handler = null;
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    // Program is suspended while waiting for an incoming connection.
                    Console.WriteLine("Waiting for a connection...");
                    handler = listener.Accept();
                    data = null;

                    var done = false;
                    while (!done)
                    {
                        var bytes = new byte[1024];
                        var bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        for (var i = 0; i < bytesRec; i++)
                        {
                            if (bytes[i] == 0x1c)
                            {
                                done = true;
                            }
                        }
                    }

                    Console.WriteLine("Text received : {0}", data);
                    var msg = Encoding.ASCII.GetBytes(data);

                    handler.Send(msg);
                }
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static int Main(String[] args)
        {
            StartListening();
            return 0;
        }
    }
}