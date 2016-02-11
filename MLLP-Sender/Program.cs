using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MLLP_Sender
{
    public static class SynchronousSocketClient
    {
        private static int _counter;

        private static string GenerateControlId()
        {
            Interlocked.CompareExchange(ref _counter, 0, 9999);
            Interlocked.Increment(ref _counter);
            return string.Format("{0}_{1:D4}", DateTime.Now.ToString("yyyyMMddHHmmss"), _counter);
        }

        public static void Main(String[] args)
        {
            var filename = args[0];

            try
            {
                var ipHostInfo = Dns.Resolve(Dns.GetHostName());
                var ipAddress = ipHostInfo.AddressList[0];
                var remoteEp = new IPEndPoint(ipAddress, 11000);

                var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    sender.Connect(remoteEp);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint);
                    SendFile(sender, filename);

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void SendFile(Socket sender, string filename)
        {
            var bytes = new byte[1024];

            var controlId = GenerateControlId();
            var hl7 = File.ReadAllText(filename);
            hl7 = hl7.Replace("{controlId}", controlId);

            Console.WriteLine("file: " + filename + "\n");

            var length = Encoding.ASCII.GetByteCount(hl7) + 3;
            var msg = new byte[length];
            msg[0] = 0x0b;
            Encoding.ASCII.GetBytes(hl7).CopyTo(msg, 1);
            msg[length - 2] = 0x1c;
            msg[length - 1] = 0x0d;

            var bytesSent = sender.Send(msg);
            Console.WriteLine("counter: " + _counter );
            Console.WriteLine("bytes sent: " + bytesSent);

            var bytesRec = sender.Receive(bytes);
            Console.WriteLine("response: {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
        }
    }
}