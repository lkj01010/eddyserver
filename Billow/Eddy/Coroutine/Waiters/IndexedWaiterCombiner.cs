using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Coroutine.Waiters
{
    /// <summary>
    /// 类似WaiterCombiner，完成时会传递索引以指明是哪个waiter完成
    /// </summary>
    class IndexedWaiterCombiner : Waiter
    {
       private readonly Waiter[] waiters;

       public IndexedWaiterCombiner(ValueExtractor<int> index, params Waiter[] waiters)
        {
            this.waiters = waiters;
            for (int i = 0; i < waiters.Length; ++i)
            {
                int value = i;
                waiters[i].Completed += () =>
                    {
                        index.Value = value;
                        this.OnCompleted();
                    };
            }
        }

        protected override void Cancel()
        {
            foreach (var waiter in waiters)
                waiter.Dispose();
        }
    }
}
