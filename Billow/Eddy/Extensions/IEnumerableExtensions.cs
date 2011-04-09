using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.Extensions
{
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// 计算 System.UInt32 值序列之和。
		/// </summary>
		/// <param name="source">一个要计算和的 System.UInt32 值序列。</param>
		/// <returns>序列值之和。</returns>
		/// <exception cref="System.ArgumentNullException">source 为 null。</exception>
		/// <exception cref="System.OverflowException">和大于 System.UInt32.MaxValue。</exception>
		/// <seealso cref="System.Linq.Enumerable"/>
		public static uint Sum(this IEnumerable<uint> source)
		{
			if (source == null)
				throw new ArgumentNullException();
			uint sum = 0;
			checked
			{
				foreach (var n in source)
					sum += n;
			}
			return sum;
		}
	}
}
