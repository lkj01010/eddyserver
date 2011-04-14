using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Coroutine.Waiters
{
    internal class MessageWaiter<T> : Waiter where T : class
    {
        private readonly Action<T> messageGetter;
        private MessageDispatcher dispatcher;
        private readonly Action<object> handler;

        public MessageWaiter(MessageDispatcher dispatcher, Action<T> messageGetter)
        {
            this.messageGetter = messageGetter;
            this.dispatcher = dispatcher;
            this.handler = (message) =>
                {
                    messageGetter(message as T);
                    this.OnCompleted();
                };
            this.dispatcher.AddHandler(typeof(T), this.handler);
        }

        internal override void CleanUp(bool completed)
        {
            if (!completed)
                this.dispatcher.RemoveHandler(typeof(T), handler);
        }
    }
}
