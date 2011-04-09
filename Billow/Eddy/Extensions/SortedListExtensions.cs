using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Extensions
{
	public static class SortedListExtent
	{
		public static void ForEach<TKey, TValue>(this SortedList<TKey, TValue> my, Action<KeyValuePair<TKey, TValue>> action)
		{
			foreach (var pair in my)
			{
				action(pair);
			}
		}
	}
}
