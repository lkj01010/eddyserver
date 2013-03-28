using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace Eddy.ProtoBufMessage
{
	/// <summary>
	/// 消息类型ID<see cref="MessageTypeID"/>和对应<see cref="ProtoBuf.IExtensible"/>类型的解析器
	/// </summary>
	public static class MessageTypeParser
	{
		/// <summary>
		/// 从给定汇编中自动解析消息类型ID<see cref="MessageTypeID"/>和对应<see cref="ProtoBuf.IExtensible"/>类型
		/// </summary>
		/// <param name="assembly">要解析的汇编</param>
		/// <param name="categoryIdType">Category枚举类型，该类型描述了<see cref="MessageTypeID.CategoryID"/>值</param>
		/// <remarks>
		/// 从<paramref name="categoryIdType"/>对应的枚举解析出一级ID<see cref="MessageTypeID.CategoryID"/>值，
		/// 再参考一级枚举名字得到二级ID枚举，二级枚举的值对应<see cref="MessageTypeID.TypeID"/>的值，二级枚举
		/// 的名字和所映射的<see cref="ProtoBuf.IExtensible"/>类型名对应
		/// </remarks>
		public static SortedList<MessageTypeID, Type> Parse(Assembly assembly, Type categoryIdType)
		{
			SortedList<MessageTypeID, Type> ret = new SortedList<MessageTypeID, Type>();

			foreach(var cName in Enum.GetNames(categoryIdType))
			{
				uint cValue = Convert.ToUInt32(Enum.Parse(categoryIdType, cName));
				Type typeIdType = assembly.GetType(categoryIdType.Namespace + "." + cName + "TypeID+" + cName, true);
				foreach (var tName in Enum.GetNames(typeIdType))
				{
					uint tValue = Convert.ToUInt32(Enum.Parse(typeIdType, tName));
					Type messageType = assembly.GetType(categoryIdType.Namespace + "." + tName, true);
					Debug.Assert(messageType.GetInterface(typeof(ProtoBuf.IExtensible).FullName) != null);
					ret.Add(new MessageTypeID() { CategoryID = cValue, TypeID = tValue }, messageType);
				}
			}

			return ret;
		}
	}
}
