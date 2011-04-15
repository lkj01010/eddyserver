using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eddy.Message;
using Eddy.Net;
using System.IO;
using System.Diagnostics;

namespace Eddy.Net
{
	/// <summary>
	/// 基于<see cref="ProtoBuf.IExtensible"/>消息的网络客户端
	/// </summary>
	sealed public class MessageTcpClient : TcpClient
	{
        private MessageTcpHandlers handlers;

        public event Action Connected;
        public event Action<Exception> Disconnected;
        public event Action<Exception> MessageDeserializeFailed
        {
            add { handlers.MessageDeserializeFailed += value; }
            remove { handlers.MessageDeserializeFailed -= value; }
        }

		public MessageTcpClient(MessageSerializer serializer, 
            Action<TcpSession, object> messageHandler) :
            this (serializer, messageHandler, () => new TcpSession())
        {
        }

        public MessageTcpClient(MessageSerializer serializer,
            Action<TcpSession, object> messageHandler, Func<TcpSession> creator)
        {
            handlers = new MessageTcpHandlers(serializer);
            base.Initialize(() =>
            {
                var session = creator();
                session.Initialize(handlers.CreateReceivedHandler(session, messageHandler),
                    handlers.CreateSendingHandler());
                session.Connected += OnConnected;
                session.Disconnected += OnDisconnected;
                return session;
            });
        }

        public void Send(object message)
        {
			if (Session == null)
				return;
            Session.Send(message);
        }

        private void OnConnected()
        {
            if (Connected != null)
                Connected();
        }

        private void OnDisconnected(Exception e)
        {
            if (Disconnected != null)
                Disconnected(e);
        }
	}
}
