using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using ClientCore;

public class Resource
{
    internal WWW www;
    internal List<Resource> dependencies;
    internal AssetBundle mainAsset;

    public string RelativePath { get; internal set; }

    public bool IsLoading { get; internal set; }

    public bool IsDone
    {
        get
        {
            if (dependencies == null)
            {
                if (www == null)
                    return false;
                return www.isDone;
            }
            else
            {
                if (!dependencies.All(x => x.IsDone))
                    return false;
                if (www == null)
                    return false;
                return www.isDone;
            }
        }
    }

    public float Progress
    {
        get
        {
            float total = 1.0f;
            float current = www.progress;
            if (dependencies != null)
            {
                total += dependencies.Count;
                current += dependencies.Aggregate(0.0f, (sum, res) => sum + res.Progress);
            }
            return current / total;
        }
    }

    public WWW MainWWW { get { return www; } }

    public IEnumerator Wait()
    {
		var enumerator = ResourceManager.LoadAsync(this);
        Stack<IEnumerator> stack = new Stack<IEnumerator>();
        stack.Push(enumerator);

        while (stack.Count > 0)
		{
            var current = stack.Peek();
            if (current.MoveNext())
            {
                while (current.Current is IEnumerator)
                {
                    stack.Push(current.Current as IEnumerator);
                    current = stack.Peek();
                }
                yield return current.Current;
            }
            else
            {
                stack.Pop();
            }
		}
    }
}
