using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Eddy.Extensions
{
	public static class XElementExtensions
	{
		/// <summary>
		/// 从<see cref="XElement"/>中得到给定属性的值。
		/// 当<see cref="XElement"/>为null，或不存在给定的属性，或给定的属性值为null时，
		/// 都会返回<paramref name="defaultValue"/>
		/// </summary>
		public static string AttributeValue(this XElement my, XName name, string defaultValue)
		{
			if (my != null)
			{
				var attr = my.Attribute(name);
				if (attr != null && attr.Value != null)
					return attr.Value;
			}
			return defaultValue;
		}
	}
}
