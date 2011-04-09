using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eddy.Message;

namespace Eddy.Net
{
    sealed public class MessageTcpService : TcpService
    {
        private MessageTcpHandlers handlers;

        public event Action<Exception> MessageDeserializeFailed
        {
            add { handlers.MessageDeserializeFailed += value; }
            remove { handlers.MessageDeserializeFailed -= value; }
        }

        public MessageTcpService(MessageSerializer serializer,
            Action<ProtoBuf.IExtensible> messageHandler) : 
            this(serializer, messageHandler, () => new TcpSession())
        {
        }

        public MessageTcpService(MessageSerializer serializer,
            Action<ProtoBuf.IExtensible> messageHandler, Func<TcpSession> creator)
        {
            handlers = new MessageTcpHandlers(serializer);
            base.Initialize(() => 
            {
                var session = creator();
                session.Initialize(handlers.CreateReceivedHandler(messageHandler),
                    handlers.CreateSendingHandler());
                return session;
            });
            
        }

        /// <summary>
        /// 适用于MessageHandler不能在Service创建时确定的场合
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="initializer"></param>
        public MessageTcpService(MessageSerializer serializer, 
            Func<MessageTcpHandlers, TcpSession> initializer)
        {
            handlers = new MessageTcpHandlers(serializer);
            base.Initialize(() => initializer(handlers));
        }
    }
}
