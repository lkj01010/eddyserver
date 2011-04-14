using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Coroutine
{
    public abstract class Waiter : IDisposable
    {
        public event Action Completed;

        public void Dispose()
        {
            this.CleanUp();
        }

        protected void OnCompleted()
        {
            if (Completed != null)
                Completed();
        }

        protected abstract void CleanUp();
    }
}
