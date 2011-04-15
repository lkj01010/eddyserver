using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Timers
{
    public sealed class Timer : TimerNode
    {
        private TimerScheduler scheduler;
        private bool isEnabled = false;
        private TimeSpan interval;

        internal DateTime ExpiredTime { get; private set; }
        internal int Circles { get; set; }

        public event Action Tick;

        public Timer()
        {
            scheduler = TimerScheduler.CurrentScheduler;
        }

        public Timer(TimeSpan interval, Action callback) : this()
        {
            Interval = interval;
            Tick += callback;
        }

        public void Start()
        {
            CheckThread();
            ExpiredTime = scheduler.DateTimeProvider() + interval;
            scheduler.Register(this);
            isEnabled = true;
        }

        public void Stop()
        {
            CheckThread();
            scheduler.Remove(this);
            isEnabled = false;
        }

        public TimeSpan Interval
        {
            get { return interval; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new InvalidOperationException("Interval cannot less than 0.");
                interval = value;
                if (isEnabled)
                    Start();
            }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value == isEnabled)
                    return;
                if (value)
                    Start();
                else
                    Stop();
            }
        }

        public static void InitializeScheduler(int numSlots, int slotInterval, Func<DateTime> dateTimeProvider)
        {
            TimerScheduler.Initialize(numSlots, slotInterval, dateTimeProvider);
        }

        private void CheckThread()
        {
            if (TimerScheduler.CurrentScheduler != scheduler)
                throw new InvalidOperationException("Please use SlotTimer in the thread which it was created.");
        }

        internal void OnTick()
        {
            ExpiredTime += interval;

            if (Tick != null)
                Tick();

            if (IsEnabled)
                scheduler.Register(this);
        }
    }
}
