using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace Eddy.Timers
{
    class SlotTimerScheduler
    {
        [ThreadStatic]
        private static SlotTimerScheduler currentScheduler;
        private List<SlotTimerNode> slots;
        private SlotTimerNode tempSlot;
        private int lastFiredSlot;
        private DateTime lastFiredTime;
        private readonly DateTime startTime;
        private Timer updateTimer;
        private TimeSpan slotInterval;
        internal Func<DateTime> DateTimeProvider { get; private set; }

        internal static SlotTimerScheduler CurrentScheduler
        {
            get
            {
                if (currentScheduler == null)
                    currentScheduler = new SlotTimerScheduler();
                return currentScheduler;
            }
        }

        private SlotTimerScheduler()
            : this(8192, 20, () => Eddy.DateTimeProvider.Now)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="numSlots">总槽数，越大则槽冲突越小</param>
        /// <param name="slotInterval">槽间隔，越小精度越高，单位毫秒</param>
        /// <param name="dateTimeProvider">获取当前DateTime的函数</param>
        private SlotTimerScheduler(int numSlots, int slotInterval, Func<DateTime> dateTimeProvider)
        {
            this.slots = new List<SlotTimerNode>(numSlots);
            for (int i = 0; i < numSlots; ++i)
            {
                var node = new SlotTimerNode();
                node.LinkSelf();
                slots.Add(node);
            }
            this.tempSlot = new SlotTimerNode();
            this.tempSlot.LinkSelf();
            this.slotInterval = new TimeSpan(0, 0, 0, 0, slotInterval);
            this.DateTimeProvider = dateTimeProvider;
            this.startTime = dateTimeProvider();
            this.lastFiredSlot = -1;
            this.lastFiredTime = this.startTime - this.slotInterval;

            var dispatcher = Dispatcher.CurrentDispatcher;
            TimerCallback callback = (state) =>
                {
                    Action action = () => this.Update();
                    dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
                };
            this.updateTimer = new Timer(callback, null,
            new TimeSpan(0, 0, 0, 0, slotInterval / 2), new TimeSpan(0, 0, 0, 0, slotInterval));
        }

        /// <summary>
        /// 初始化本线程的Scheduler
        /// </summary>
        /// <param name="numSlots">总槽数，越大则槽冲突越小</param>
        /// <param name="slotInterval">槽间隔，越小精度越高，单位毫秒</param>
        /// /// <param name="dateTimeProvider">获取当前DateTime的函数</param>
        internal static void Initialize(int numSlots, int slotInterval, Func<DateTime> dateTimeProvider)
        {
            if (currentScheduler != null)
                throw new InvalidOperationException("SlotTimerScheduler has been initialized in this thread.");

            currentScheduler = new SlotTimerScheduler(numSlots, slotInterval, dateTimeProvider);
        }

        internal void Remove(SlotTimer timer)
        {
            timer.Unlink();
        }

        internal void Register(SlotTimer timer)
        {
            int slot = GetSlot(timer.ExpiredTime);
            var head = slots[slot];
            timer.LinkBefore(head);
            timer.Circles = (int)(timer.Interval.Ticks / slotInterval.Ticks / slots.Count);
        }

        private int GetSlot(DateTime time)
        {
            return (int)((time - startTime).Ticks / slotInterval.Ticks) % slots.Count;
        }

        private void Update()
        {
            var now = DateTimeProvider();
            while (lastFiredTime + slotInterval < now)
            {
                lastFiredTime += slotInterval;
                lastFiredSlot = (lastFiredSlot + 1) % slots.Count;
                var slot = slots[lastFiredSlot];
                while (slot.Next != slot)
                {
                    var timer = slot.Next as SlotTimer;
                    timer.Unlink();
                    if (timer.Circles > 0)
                    {
                        --timer.Circles;
                        timer.LinkAfter(tempSlot);
                        continue;
                    }
                    timer.OnTick();
                }
                if (tempSlot.Next != tempSlot)
                {
                    slots[lastFiredSlot] = tempSlot;
                    tempSlot = slot;
                }
            }
        }
    }
}
