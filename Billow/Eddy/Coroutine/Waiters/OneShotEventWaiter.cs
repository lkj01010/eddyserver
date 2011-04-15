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

        internal override void CleanUp(bool completed)
        {
            if (!completed)
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

        internal override void CleanUp(bool completed)
        {
            if (!completed)
                this.oneShotEvent.Remove(action);
        }
    }

    internal class OneShotEventWaiter<T1, T2> : Waiter
    {
        private readonly OneShotEvent<T1, T2> oneShotEvent;
        private readonly Action<T1, T2> action;
        public OneShotEventWaiter(OneShotEvent<T1, T2> oneShotEvent, Action<T1, T2> paramGetter)
        {
            action = (param1, param2) =>
            {
                if (paramGetter != null)
                    paramGetter(param1, param2);
                OnCompleted();
            };
            oneShotEvent.Add(action);
            this.oneShotEvent = oneShotEvent;
        }

        internal override void CleanUp(bool completed)
        {
            if (!completed)
                this.oneShotEvent.Remove(action);
        }
    }

    internal class OneShotEventWaiter<T1, T2, T3> : Waiter
    {
        private readonly OneShotEvent<T1, T2, T3> oneShotEvent;
        private readonly Action<T1, T2, T3> action;
        public OneShotEventWaiter(OneShotEvent<T1, T2, T3> oneShotEvent, Action<T1, T2, T3> paramGetter)
        {
            action = (param1, param2, param3) =>
            {
                if (paramGetter != null)
                    paramGetter(param1, param2, param3);
                OnCompleted();
            };
            oneShotEvent.Add(action);
            this.oneShotEvent = oneShotEvent;
        }

        internal override void CleanUp(bool completed)
        {
            if (!completed)
                this.oneShotEvent.Remove(action);
        }
    }

    internal class OneShotEventWaiter<T1, T2, T3, T4> : Waiter
    {
        private readonly OneShotEvent<T1, T2, T3, T4> oneShotEvent;
        private readonly Action<T1, T2, T3, T4> action;
        public OneShotEventWaiter(OneShotEvent<T1, T2, T3, T4> oneShotEvent, Action<T1, T2, T3, T4> paramGetter)
        {
            action = (param1, param2, param3, param4) =>
            {
                if (paramGetter != null)
                    paramGetter(param1, param2, param3, param4);
                OnCompleted();
            };
            oneShotEvent.Add(action);
            this.oneShotEvent = oneShotEvent;
        }

        internal override void CleanUp(bool completed)
        {
            if (!completed)
                this.oneShotEvent.Remove(action);
        }
    }
}
