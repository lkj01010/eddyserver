using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy
{
    /// <summary>
    /// 每次调用后都会清空delegate列表的事件
    /// </summary>
    public class OneShotEvent
    {
        private Action handlers;
        private Action temp;

        public void Add(Action handler)
        {
            handlers += handler;
        }

        public void Remove(Action handler)
        {
            handlers -= handler;
            temp -= handler;
        }

        public void Raise()
        {
            if (handlers == null) 
                return; 
            temp = handlers;
            handlers = null;
            temp();
            temp = null;
        }
    }

    public class OneShotEvent<T>
    {
        private Action<T> handlers;
        private Action<T> temp;

        public void Add(Action<T> handler)
        {
            handlers += handler;
        }

        public void Remove(Action<T> handler)
        {
            handlers -= handler;
            temp -= handler;
        }

        public void Raise(T arg)
        {
            if (handlers == null)
                return;
            temp = handlers;
            handlers = null;
            temp(arg);
            temp = null;
        }
    }

    public class OneShotEvent<T1, T2>
    {
        private Action<T1, T2> handlers;
        private Action<T1, T2> temp;

        public void Add(Action<T1, T2> handler)
        {
            handlers += handler;
        }

        public void Remove(Action<T1, T2> handler)
        {
            handlers -= handler;
            temp -= handler;
        }

        public void Raise(T1 arg1, T2 arg2)
        {
            if (handlers == null) 
                return; 
            temp = handlers;
            handlers = null;
            temp(arg1, arg2);
            temp = null;
        }
    }

    public class OneShotEvent<T1, T2, T3>
    {
        private Action<T1, T2, T3> handlers;
        private Action<T1, T2, T3> temp;

        public void Add(Action<T1, T2, T3> handler)
        {
            handlers += handler;
        }

        public void Remove(Action<T1, T2, T3> handler)
        {
            handlers -= handler;
            temp -= handler;
        }

        public void Raise(T1 arg1, T2 arg2, T3 arg3)
        {
            if (handlers == null) 
                return; 
            temp = handlers;
            handlers = null;
            temp(arg1, arg2, arg3);
            temp = null;
        }
    }

    public class OneShotEvent<T1, T2, T3, T4>
    {
        private Action<T1, T2, T3, T4> handlers;
        private Action<T1, T2, T3, T4> temp;

        public void Add(Action<T1, T2, T3, T4> handler)
        {
            handlers += handler;
        }

        public void Remove(Action<T1, T2, T3, T4> handler)
        {
            handlers -= handler;
            temp -= handler;
        }

        public void Raise(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (handlers == null) 
                return; 
            temp = handlers;
            handlers = null;
            temp(arg1, arg2, arg3, arg4);
            temp = null;
        }
    }
}
