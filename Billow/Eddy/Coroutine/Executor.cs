using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eddy.Coroutine.Waiters;

namespace Eddy.Coroutine
{
    using Controller = IEnumerator<Waiter>;

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
            var controller = coroutine.GetEnumerator();
            if (!controller.MoveNext())
                return controller;
            controller.Current.Completed += () => this.OnWaiterCompleted(controller);
            this.controllers.Add(controller);
            return controller;
        }

        public void StopCoroutine(Controller controller)
        {
            if (controller.Current == null)
                return;
            controller.Current.Dispose();
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

        public Waiter WaitForOneShotEvent<T>(OneShotEvent<T> oneShotEvent, ValueExtractor<T> extractor)
        {
            return new OneShotEventWaiter<T>(oneShotEvent, extractor);
        }

        public Waiter WaitForAny(params Waiter[] waiters)
        {
            return new WaiterCombiner(waiters);
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

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;
            foreach (var controller in this.controllers)
            {
                if (controller.Current != null)
                    controller.Current.Dispose();
            }
            controllers.Clear();
            disposed = true;
        }
        #endregion

        private void OnWaiterCompleted(Controller controller)
        {
            controller.Current.Dispose();
            if (controller.MoveNext())
                controller.Current.Completed += () => this.OnWaiterCompleted(controller);
            else
                this.controllers.Remove(controller);
        }
    }
}
