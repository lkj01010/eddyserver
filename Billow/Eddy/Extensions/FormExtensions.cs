using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Eddy.Extensions
{
	public static class FormExtensions
	{
		/// <summary>
		/// 保证指定的方法在UI主线程中运行，方便实现非主线跨访问UI的跨线程操作
		/// <example>用法为：<c>form.MainThreadExecute(() => SomeFunction(x, y));</c></example>
		/// </summary>
		public static void MainThreadExecute(this Form invoker, MethodInvoker action)
		{
			if (invoker.InvokeRequired)
			{
				invoker.BeginInvoke(action);
			}
			else
			{
				action();
			}
		}
	}
}
