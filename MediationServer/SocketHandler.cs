using System;
using System.Net;
using System.Net.Sockets;

namespace MediationServer
{
    class SocketHandler
    {
        Socket _socket;
        public delegate void SocketAcceptedFileRequestHandler(Socket e);
        public delegate void SocketAcceptedConnectionHandler(Socket e);
        public event SocketAcceptedFileRequestHandler SocketAcceptedFileRequestEvent;
        public event SocketAcceptedConnectionHandler SocketAcceptedConnectionEvent;

        public bool Listening { get; private set; }
        public int Port { get; private set; }

        public SocketHandler(int port)
        {
            Port = port;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        //start a new socket
        public void Start()
        {
            if (Listening) return;
            _socket.Bind(new IPEndPoint(0, Port));
            _socket.Listen(0);
            _socket.BeginAccept(ConnectCallback, null);
            Listening = true;
        }

        //stop a socket and dispose it
        public void Stop()
        {
            if (!Listening) return;
            _socket.Close();
            _socket.Dispose();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket s = _socket.EndAccept(ar);
                if (SocketAcceptedFileRequestEvent != null)
                    SocketAcceptedFileRequestEvent(s);

                if (SocketAcceptedConnectionEvent != null)
                    SocketAcceptedConnectionEvent(s);

                _socket.BeginAccept(ConnectCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

