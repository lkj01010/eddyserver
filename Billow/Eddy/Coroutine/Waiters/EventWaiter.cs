using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Coroutine.Waiters
{
    internal class EventWaiter : Waiter
    {
        private readonly Action<Action> subscriber;
        public EventWaiter(Action<Action> subscriber, Action<Action> unsubscriber)
        {
            subscriber(OnCompleted);
            this.subscriber = unsubscriber;
        }

        internal override void CleanUp(bool completed)
        {
            subscriber(OnCompleted);
        }
    }

    internal class EventWaiter<T> : Waiter
    {
        private readonly Action<Action<T>> unhooker;
        private readonly Action<T> action;
        public EventWaiter(Action<Action<T>> hooker, Action<Action<T>> unhooker, Action<T> paramGetter)
        {
            this.action = (param) =>
                {
                    if (paramGetter != null)
                        paramGetter(param);
                    this.OnCompleted();
                };
            hooker(this.action);
            this.unhooker = unhooker;
        }

        internal override void CleanUp(bool completed)
        {
            unhooker(this.action);
        }
    }

    internal class EventWaiter<T1, T2> : Waiter
    {
        private readonly Action<Action<T1, T2>> unhooker;
        private readonly Action<T1, T2> action;
        public EventWaiter(Action<Action<T1, T2>> hooker, Action<Action<T1, T2>> unhooker, Action<T1, T2> paramGetter)
        {
            this.action = (param1, param2) =>
            {
                if (paramGetter != null)
                    paramGetter(param1, param2);
                this.OnCompleted();
            };
            hooker(this.action);
            this.unhooker = unhooker;
        }

        internal override void CleanUp(bool completed)
        {
            unhooker(this.action);
        }
    }

    internal class EventWaiter<T1, T2, T3> : Waiter
    {
        private readonly Action<Action<T1, T2, T3>> unhooker;
        private readonly Action<T1, T2, T3> action;
        public EventWaiter(Action<Action<T1, T2, T3>> hooker, Action<Action<T1, T2, T3>> unhooker, Action<T1, T2, T3> paramGetter)
        {
            this.action = (param1, param2, param3) =>
            {
                if (paramGetter != null)
                    paramGetter(param1, param2, param3);
                this.OnCompleted();
            };
            hooker(this.action);
            this.unhooker = unhooker;
        }

        internal override void CleanUp(bool completed)
        {
            unhooker(this.action);
        }
    }

    internal class EventWaiter<T1, T2, T3, T4> : Waiter
    {
        private readonly Action<Action<T1, T2, T3, T4>> unhooker;
        private readonly Action<T1, T2, T3, T4> action;
        public EventWaiter(Action<Action<T1, T2, T3, T4>> hooker, Action<Action<T1, T2, T3, T4>> unhooker, Action<T1, T2, T3, T4> paramGetter)
        {
            this.action = (param1, param2, param3, param4) =>
            {
                if (paramGetter != null)
                    paramGetter(param1, param2, param3, param4);
                this.OnCompleted();
            };
            hooker(this.action);
            this.unhooker = unhooker;
        }

        internal override void CleanUp(bool completed)
        {
            unhooker(this.action);
        }
    }
}
