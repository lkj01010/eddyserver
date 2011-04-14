using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Coroutine
{
    public abstract class Waiter
    {
        public event Action Completed;

        protected void OnCompleted()
        {
            if (Completed != null)
                Completed();
        }

        internal abstract void CleanUp(bool completed);
    }
}
