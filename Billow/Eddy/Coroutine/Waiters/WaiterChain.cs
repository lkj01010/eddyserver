using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Coroutine.Waiters
{
    /// <summary>
    /// 用于连接多个Waiter
    /// </summary>
    internal class WaiterChain : Waiter
    {
        private Controller controller;
        public WaiterChain(Controller controller)
        {
            this.controller = controller;
            this.controller.Completed += this.OnCompleted;
        }

        protected override void Cancel()
        {
            this.controller.Completed -= this.OnCompleted;
            this.controller.Stop();
        }
    }
}
