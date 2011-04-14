using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Coroutine.Waiters
{
    internal class OneShotEventWaiter : Waiter
    {
        private readonly OneShotEvent oneShotEvent;
        public OneShotEventWaiter(OneShotEvent oneShotEvent)
        {
            oneShotEvent.Add(OnCompleted);
            this.oneShotEvent = oneShotEvent;
        }

        protected override void Cancel()
        {
            this.oneShotEvent.Remove(OnCompleted);
        }
    }

    internal class OneShotEventWaiter<T> : Waiter
    {
        private readonly OneShotEvent<T> oneShotEvent;
        private readonly Action<T> action;
        public OneShotEventWaiter(OneShotEvent<T> oneShotEvent, Action<T> paramGetter)
        {
            action = (param) =>
                {
                    if (paramGetter != null)
                        paramGetter(param);
                    OnCompleted();
                };
            oneShotEvent.Add(action);
            this.oneShotEvent = oneShotEvent;
        }

        protected override void Cancel()
        {
            this.oneShotEvent.Remove(action);
        }
    }
}
