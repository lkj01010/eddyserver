using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy.ProtoBufConnection.Message
{
	/// <summary>
	/// 用数字表示的<see cref="ProtoBuf.IExtensible"/>消息类型
	/// </summary>
	/// <remarks>
	/// 当通过网络序列化<see cref="ProtoBuf.IExtensible"/>对象时，将<see cref="MessageTypeID"/>
	/// 填充到<see cref="Eddy.Message.MessagePackage"/>中，以实现序列化对象的类型标识。该标识
	/// 对对象的反序列化非常重要。
	/// </remarks>
	public struct MessageTypeID : IComparable<MessageTypeID>
	{
		public uint CategoryID { get; set; }
		public uint TypeID { get; set; }

		#region IComparable<MessageType> 成员

		public int CompareTo(MessageTypeID other)
		{
			if (this.CategoryID > other.CategoryID)
				return 1;
			else if (this.CategoryID < other.CategoryID)
				return -1;

			if (this.TypeID > other.TypeID)
				return 1;
			else if (this.TypeID < other.TypeID)
				return -1;

			return 0;
		}

		#endregion

		public override string ToString()
		{
			return string.Format("{{CategoryID={0},TypeID={1}}}", CategoryID, TypeID);
		}
	}
}
