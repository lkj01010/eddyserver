using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Eddy.Net
{
    /// <summary>
    /// 高并发Tcp网络连接监听器
    /// </summary>
    public class TcpService
    {
		private Socket listener;
		private Func<TcpSession> sessionCreator;
        private readonly SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
        private const int BacklogSize = 1024;
        public event Action<Exception> ExceptionHandler;

        public void Initialize(Func<TcpSession> sessionCreator)
        {
            this.sessionCreator = sessionCreator;
        }

        public void Listen(IPEndPoint localEndPoint)
        {
            try
            {
                Debug.Assert(this.listener == null);
                this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.listener.Bind(localEndPoint);
                this.listener.Listen(BacklogSize);
                this.acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCallback);
                if (!this.listener.AcceptAsync(this.acceptArgs))
                    AcceptCallback(null, this.acceptArgs);
            }
            catch (Exception e)
            {
                if (this.ExceptionHandler != null)
                    this.ExceptionHandler(e);
            }
        }

        public void Close()
        {
            try
            {
                this.listener.Close();
                this.listener = null;
            }
            catch (Exception e)
            {
                if (this.ExceptionHandler != null)
                    this.ExceptionHandler(e);
            }
        }

        private void AcceptCallback(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                var session = sessionCreator();
                var socket = args.AcceptSocket;
                session.SetSocket(socket);
                args.AcceptSocket = null;
                if (!listener.AcceptAsync(args))
                    AcceptCallback(null, args);
            }
            catch (Exception e)
            {
                if (this.ExceptionHandler != null)
                    this.ExceptionHandler(e);
            }
        }
    }
}
