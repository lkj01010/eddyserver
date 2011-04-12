using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eddy.Timers;

namespace Eddy.Coroutine.Waiters
{
    /// <summary>
    /// 等待指定时间间隔
    /// </summary>
    internal class TimeSpanWaiter : Waiter
    {
        private readonly SlotTimer timer;

        public TimeSpanWaiter(TimeSpan span)
        {
            timer = new SlotTimer(span, this.OnCompleted);
            timer.Start();
        }

        protected override void Cancel()
        {
            timer.Stop();
        }
    }
}
