using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Timers
{
    public class SlotTimerNode
    {
        internal SlotTimerNode Prev { get; private set; }
        internal SlotTimerNode Next { get; private set; }

        internal void LinkSelf()
        {
            Unlink();
            Prev = this;
            Next = this;
        }

        internal void LinkBefore(SlotTimerNode node)
        {
            Unlink();

            if (node.Prev != null)
            {
                node.Prev.Next = this;
                this.Prev = node.Prev;
            }

            node.Prev = this;
            this.Next = node;
        }

        internal void LinkAfter(SlotTimerNode node)
        {
            Unlink();

            if (node.Next != null)
            {
                node.Next.Prev = this;
                this.Next = node.Next;
            }

            node.Next = this;
            this.Prev = node;
        }

        internal void Unlink()
        {
            if (Prev != null)
                Prev.Next = Next;

            if (Next != null)
                Next.Prev = Prev;

            Prev = null;
            Next = null;
        }
    }
}
