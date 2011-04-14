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
       private readonly List<Waiter> waiters;

       public IndexedWaiterCombiner(Action<int> indexGetter, params Waiter[] waiters)
        {
            this.waiters = new List<Waiter>(waiters);
            for (int i = 0; i < waiters.Length; ++i)
            {
                int value = i;
                waiters[value].Completed += () =>
                    {
                        // 防止重复CleanUp
                        this.waiters.Remove(waiters[value]);
                        if (indexGetter != null)
                            indexGetter(value);
                        this.OnCompleted();
                    };
            }
        }

       internal override void CleanUp(bool completed)
        {
            foreach (var waiter in waiters)
            {
                waiter.CleanUp(false);
            }
        }
    }
}
