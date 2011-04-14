using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eddy.Extensions;

namespace Eddy.Coroutine
{
    class MessageDispatcher
    {
        public T OnMessage<T>(T message) where T : class
        {
            var queue = handlers.GetValueOrDefault(typeof(T));
            if (queue == null || queue.Count == 0)
                return message;
            var handler = queue.Dequeue();
            handler(message);
            return null;
        }

        public void AddHandler(Type type, Action<object> handler)
        {
            var queue = handlers.GetValueOrDefault(type);
            if (queue == null)
            {
                queue = new Queue<Action<object>>();
                handlers.Add(type, queue);
            }
            queue.Enqueue(handler);
        }

        public void RemoveHandler(Type type, Action<object> handler)
        {
            var queue  = handlers.GetValueOrDefault(type);
            if (queue == null || queue.Count == 0)
                return;
            var newQueue = new Queue<Action<object>>();
            while (queue.Count != 0)
            {
                var tempHandler = queue.Dequeue();
                if (tempHandler == handler)
                    continue;
                newQueue.Enqueue(tempHandler);
            }
            handlers[type] = newQueue;
        }

        private Dictionary<Type, Queue<Action<object>>> handlers 
            = new Dictionary<Type,Queue<Action<object>>>();
    }
}
