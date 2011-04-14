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

        public static ValueExtractor<T> ExtractValue<T>()
        {
            return new ValueExtractor<T>();
        }

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
            controller.Enumerator.Current.Dispose();
            this.controllers.Remove(controller);
        }

        public Waiter WaitForSeconds(float seconds)
        {
            var span = new TimeSpan(0, 0, 0, 0, (int)(seconds * 1000));
            return new TimeSpanWaiter(span);
        }

        public Waiter WaitForOneShotEvent(OneShotEvent oneShotEvent)
        {
            return new OneShotEventWaiter(oneShotEvent);
        }

        public Waiter WaitForOneShotEvent<T>(OneShotEvent<T> oneShotEvent, Action<T> paramGetter)
        {
            return new OneShotEventWaiter<T>(oneShotEvent, paramGetter);
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
                    controller.Enumerator.Current.Dispose();
                }
            }
            controllers.Clear();
            disposed = true;
        }
        #endregion

        private void OnWaiterCompleted(Controller controller)
        {
            controller.Enumerator.Current.Dispose();
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
