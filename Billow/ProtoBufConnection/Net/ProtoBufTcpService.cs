using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eddy.ProtoBufConnection.Message;
using Eddy.Net;

namespace Eddy.ProtoBufConnection.Net
{
    sealed public class ProtoBufTcpService : TcpService
    {
        private ProtoBufTcpHandlers handlers;

        public event Action<Exception> MessageDeserializeFailed
        {
            add { handlers.MessageDeserializeFailed += value; }
            remove { handlers.MessageDeserializeFailed -= value; }
        }

        public ProtoBufTcpService(MessageSerializer serializer,
            Action<TcpSession, object> messageHandler) : 
            this(serializer, messageHandler, () => new TcpSession())
        {
        }

        public ProtoBufTcpService(MessageSerializer serializer,
            Action<TcpSession, object> messageHandler, Func<TcpSession> creator)
        {
            handlers = new ProtoBufTcpHandlers(serializer);
            base.Initialize(() => 
            {
                var session = creator();
                session.Initialize(handlers.CreateReceivedHandler(session, messageHandler),
                    handlers.CreateSendingHandler());
                return session;
            });
            
        }

        /// <summary>
        /// 适用于MessageHandler不能在Service创建时确定的场合
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="initializer"></param>
        public ProtoBufTcpService(MessageSerializer serializer, 
            Func<ProtoBufTcpHandlers, TcpSession> initializer)
        {
            handlers = new ProtoBufTcpHandlers(serializer);
            base.Initialize(() => initializer(handlers));
        }
    }
}
