using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eddy.Coroutine.Waiters;

namespace Eddy.Coroutine
{
    public class Executor : IDisposable
    {
        private bool disposed = false;
        private List<Controller> controllers = new List<Controller>();
        private MessageDispatcher messageDispatcher;

        public Controller StartCoroutine(IEnumerable<Waiter> coroutine)
        {
            var enumerator = coroutine.GetEnumerator();
            var controller = new Controller { Enumerator = enumerator, Executor = this };
            if (!enumerator.MoveNext())
            {
                controller.OnCompleted();
                return controller;
            }
            enumerator.Current.Completed += () => this.OnWaiterCompleted(controller);
            this.controllers.Add(controller);
            return controller;
        }

        public void StopCoroutine(Controller controller)
        {
            if (controller.Enumerator.Current == null)
                return;
            controller.Enumerator.Current.CleanUp(false);
            this.controllers.Remove(controller);
        }

        /// <summary>
        /// 接收消息，如果内部消耗了该消息，返回null，否则把返回原消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public T OnMessage<T>(T message) where T : class
        {
            if (message == null)
                throw new NullReferenceException();
            if (messageDispatcher == null)
                return message;
            return messageDispatcher.OnMessage(message);
        }

        public Waiter WaitForSeconds(float seconds)
        {
            var span = new TimeSpan(0, 0, 0, 0, (int)(seconds * 1000));
            return new TimeSpanWaiter(span);
        }

        public Waiter WaitForEvent(Action<Action> hooker, Action<Action> unhooker)
        {
            return new EventWaiter(hooker, unhooker);
        }

        public Waiter WaitForEvent<T>(Action<Action<T>> hooker, Action<Action<T>> unhooker, Action<T> paramGetter)
        {
            return new EventWaiter<T>(hooker, unhooker, paramGetter);
        }

        public Waiter WaitForEvent<T1, T2>(Action<Action<T1, T2>> hooker, Action<Action<T1, T2>> unhooker, Action<T1, T2> paramGetter)
        {
            return new EventWaiter<T1, T2>(hooker, unhooker, paramGetter);
        }

        public Waiter WaitForEvent<T1, T2, T3>(Action<Action<T1, T2, T3>> hooker, Action<Action<T1, T2, T3>> unhooker, Action<T1, T2, T3> paramGetter)
        {
            return new EventWaiter<T1, T2, T3>(hooker, unhooker, paramGetter);
        }

        public Waiter WaitForEvent<T1, T2, T3, T4>(Action<Action<T1, T2, T3, T4>> hooker, Action<Action<T1, T2, T3, T4>> unhooker, Action<T1, T2, T3, T4> paramGetter)
        {
            return new EventWaiter<T1, T2, T3, T4>(hooker, unhooker, paramGetter);
        }

        public Waiter WaitForOneShotEvent(OneShotEvent oneShotEvent)
        {
            return new OneShotEventWaiter(oneShotEvent);
        }

        public Waiter WaitForOneShotEvent<T>(OneShotEvent<T> oneShotEvent, Action<T> paramGetter)
        {
            return new OneShotEventWaiter<T>(oneShotEvent, paramGetter);
        }

        public Waiter WaitForOneShotEvent<T1, T2>(OneShotEvent<T1, T2> oneShotEvent, Action<T1, T2> paramGetter)
        {
            return new OneShotEventWaiter<T1, T2>(oneShotEvent, paramGetter);
        }

        public Waiter WaitForOneShotEvent<T1, T2, T3>(OneShotEvent<T1, T2, T3> oneShotEvent, Action<T1, T2, T3> paramGetter)
        {
            return new OneShotEventWaiter<T1, T2, T3>(oneShotEvent, paramGetter);
        }

        public Waiter WaitForOneShotEvent<T1, T2, T3, T4>(OneShotEvent<T1, T2, T3, T4> oneShotEvent, Action<T1, T2, T3, T4> paramGetter)
        {
            return new OneShotEventWaiter<T1, T2, T3, T4>(oneShotEvent, paramGetter);
        }

        public Waiter WaitForAny(params Waiter[] waiters)
        {
            return new WaiterCombiner(waiters);
        }

        public Waiter WaitForAny(Action<int> indexGetter, params Waiter[] waiters)
        {
            return new IndexedWaiterCombiner(indexGetter, waiters);
        }

        public Waiter WaitForCoroutine(IEnumerable<Waiter> coroutine)
        {
            var controller = StartCoroutine(coroutine);
            return new WaiterChain(controller);
        }

        public Waiter WaitForMessage<T>(Action<T> messageGetter) where T : class
        {
            if (this.messageDispatcher == null)
                this.messageDispatcher = new MessageDispatcher();
            return new MessageWaiter<T>(this.messageDispatcher, messageGetter);
        }

        #region IDisposable Members

        ~Executor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected int GetControllerCount()
        { 
            return controllers.Count; 
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;
            foreach (var controller in this.controllers)
            {
                if (controller.Enumerator.Current != null)
                {
                    controller.Enumerator.Current.CleanUp(false);
                }
            }
            controllers.Clear();
            disposed = true;
        }
        #endregion

        private void OnWaiterCompleted(Controller controller)
        {
            controller.Enumerator.Current.CleanUp(true);
            if (controller.Enumerator.MoveNext())
            {
                controller.Enumerator.Current.Completed += () => this.OnWaiterCompleted(controller);
            }
            else
            {
                controller.OnCompleted();
                this.controllers.Remove(controller);
            }
        }
    }
}
