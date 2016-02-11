using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MLLP
{
    public class MllpConnector : IDisposable
    {
        private readonly Socket _socket;

        public MllpConnector(EndPoint remoteEp)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(remoteEp);
        }

        public EndPoint RemoteEndPoint
        {
            get { return _socket.RemoteEndPoint; }
        }

        public void Dispose()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public int SendMessage(string message)
        {
            var length = Encoding.ASCII.GetByteCount(message) + 3;
            var msg = new byte[length];
            msg[0] = 0x0b;
            Encoding.ASCII.GetBytes(message).CopyTo(msg, 1);
            msg[length - 2] = 0x1c;
            msg[length - 1] = 0x0d;

            return _socket.Send(msg);
        }

        public string ReadResponse()
        {
            var bytes = new byte[1024];
            var bytesRec = _socket.Receive(bytes);
            return Encoding.ASCII.GetString(bytes, 0, bytesRec);
        }
    }
}
