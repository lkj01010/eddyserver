using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Coroutine.Waiters
{
    internal class EventWaiter : Waiter
    {
        private readonly Action<Action> unhooker;
        public EventWaiter(Action<Action> hooker, Action<Action> unhooker)
        {
            hooker(OnCompleted);
            this.unhooker = unhooker;
        }

        internal override void CleanUp(bool completed)
        {
            unhooker(OnCompleted);
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
}
