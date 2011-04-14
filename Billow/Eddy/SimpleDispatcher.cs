using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Eddy
{
    /// <summary>
    /// 简化版Dispatcher，以解决mono对Dispatcher实现不完全的问题
    /// </summary>
    public class SimpleDispatcher
    {
        [ThreadStatic]
        private static SimpleDispatcher currentDispatcher;
        private Queue<Action> actionsProcessing = new Queue<Action>();
        private Queue<Action> actionsToBeProcessed = new Queue<Action>();
        private bool shutdown = false;
        private EventWaitHandle wait = new EventWaitHandle (false, EventResetMode.AutoReset);
        private Thread baseThread;

        public static SimpleDispatcher CurrentDispatcher
        {
            get
            {
                if (currentDispatcher == null)
                    currentDispatcher = new SimpleDispatcher();
                return currentDispatcher;
            }
        }

        SimpleDispatcher()
        {
            baseThread = Thread.CurrentThread;
        }

        public void Invoke(Action action)
        {
            lock (this)
                actionsToBeProcessed.Enqueue(action);

            if (baseThread != Thread.CurrentThread)
                wait.Set();
        }

        public void Update()
        {
            if (baseThread != Thread.CurrentThread)
                throw new InvalidOperationException("Invalid thread.");

            if (shutdown)
                throw new InvalidOperationException("Dispatcher has shutdown.");

            lock (this)
            {
                Utility.Swap(ref actionsProcessing, ref actionsToBeProcessed);
            }

            while (actionsProcessing.Count != 0)
            {
                foreach (var action in actionsProcessing)
                {
                    if (shutdown)
                    {
                        actionsProcessing.Clear();
                        return;
                    }
                    action();
                }

                actionsProcessing.Clear();

                lock (this)
                {
                    Utility.Swap(ref actionsProcessing, ref actionsToBeProcessed);
                }
            }
        }

        public void Run()
        {
            shutdown = false;
            while (true)
            {
                Update();
                if (shutdown)
                    return;
                wait.WaitOne();
                wait.Reset();
            }
        }

        public void Shutdown()
        {
            this.Invoke(() => this.shutdown = true);
        }
    }
}
