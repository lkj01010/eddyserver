using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Coroutine
{
    public class Controller
    {
        internal IEnumerator<Waiter> Enumerator { get; set; }
        internal Executor Executor { get; set; }

        public event Action Completed;

        public void Stop()
        {
            this.Executor.StopCoroutine(this);
            this.Executor = null;
            Enumerator = null;
        }

        internal void OnCompleted()
        {
            if (Completed != null)
                Completed();
        }
    }
}