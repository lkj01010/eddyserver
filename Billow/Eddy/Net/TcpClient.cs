using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Eddy.Net
{
    /// <summary>
    /// Tcp客户端连接
    /// </summary>
    public class TcpClient
    {
        private Func<TcpSession> sessionCreator;
        public TcpSession Session { get; private set; }
        public event Action<Exception> ExceptionHandler;

        public void Initialize(Func<TcpSession> sessionCreator)
        {
            this.sessionCreator = sessionCreator;
        }

        public void Connect(IPEndPoint remoteEndPoint)
        {
            try
            {
                Debug.Assert(this.Session == null);
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.RemoteEndPoint = remoteEndPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectCallback);
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                args.AcceptSocket = socket;
                if (socket.ConnectAsync(args) == false)
                    ConnectCallback(null, args);
            }
            catch (Exception e)
            {
                if (ExceptionHandler != null)
                    ExceptionHandler(e);
            }
        }

        public void Disconnect()
        {
            try
            {
                this.Session.Disconnect();
                this.Session = null;
            }
            catch (Exception e)
            {
                if (ExceptionHandler != null)
                    ExceptionHandler(e);
            }
        }

        public void BlockingDisconnect(int timeout)
        {
            try
            {
                this.Session.BlockingDisconnect(timeout);
                this.Session = null;
            }
            catch (Exception e)
            {
                if (ExceptionHandler != null)
                    ExceptionHandler(e);
            }
        }

        private void ConnectCallback(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                Session = sessionCreator();
                Session.SetSocket(args.AcceptSocket);
            }
            catch (Exception e)
            {
                if (ExceptionHandler != null)
                    ExceptionHandler(e);
            }
        }
    }

}
