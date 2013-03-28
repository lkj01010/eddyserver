using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eddy.Message;
using Eddy.Net;

namespace Eddy.Net
{
    sealed public class MessageTcpService : TcpService
    {
        private MessageHandlers handlers;

        public event Action<Exception> MessageDeserializeFailed
        {
            add { handlers.MessageDeserializeFailed += value; }
            remove { handlers.MessageDeserializeFailed -= value; }
        }

        public MessageTcpService(IMessageSerializer serializer,
            Action<TcpSession, object> messageHandler) : 
            this(serializer, messageHandler, () => new TcpSession())
        {
        }

        public MessageTcpService(IMessageSerializer serializer,
            Action<TcpSession, object> messageHandler, Func<TcpSession> creator)
        {
            handlers = new MessageHandlers(serializer);
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
        public MessageTcpService(IMessageSerializer serializer, 
            Func<MessageHandlers, TcpSession> initializer)
        {
            handlers = new MessageHandlers(serializer);
            base.Initialize(() => initializer(handlers));
        }
    }
}
