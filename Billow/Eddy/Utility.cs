using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy
{
	public static class Utility
	{
		public static void Swap<T>(ref T my, ref T other)
		{
			T temp = my;
			my = other;
			other = temp;
		}
	}
}
