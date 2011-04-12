using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Coroutine.Waiters
{
    /// <summary>
    /// 用于组合多个Waiter
    /// </summary>
    internal class WaiterCombiner : Waiter
    {
        private readonly Waiter[] waiters;

        public WaiterCombiner(params Waiter[] waiters)
        {
            this.waiters = waiters;
            foreach (var waiter in waiters)
                waiter.Completed += this.OnCompleted;
        }

        protected override void Cancel()
        {
            foreach (var waiter in waiters)
                waiter.Dispose();
        }
    }
}
