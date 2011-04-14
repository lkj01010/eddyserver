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
        private readonly List<Waiter> waiters;

        public WaiterCombiner(params Waiter[] waiters)
        {
            this.waiters = new List<Waiter>(waiters);

            foreach (var waiter in waiters)
            {
                var temp = waiter;
                waiter.Completed += () =>
                    {
                        // 防止重复CleanUp
                        this.waiters.Remove(temp);
                        this.OnCompleted();
                    };
            }
        }

        internal override void CleanUp(bool completed)
        {
            foreach (var waiter in waiters)
                waiter.CleanUp(false);
        }
    }
}
