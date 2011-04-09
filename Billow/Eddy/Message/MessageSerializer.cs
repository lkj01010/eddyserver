using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Eddy.Message
{
	/// <summary>
	/// <see cref="ProtoBuf.IExtensible"/>对象和字节流间的编码器
	/// </summary>
	/// <remarks>性能分析参见：http://www.servicestack.net/benchmarks/NorthwindDatabaseRowsSerialization.100000-times.2010-08-17.html </remarks>
	public class MessageSerializer
	{
		private readonly Dictionary<MessageTypeID, Func<Stream, ProtoBuf.IExtensible>> deserializeTable = new Dictionary<MessageTypeID, Func<Stream, ProtoBuf.IExtensible>>();
		private readonly Dictionary<Type, MessageTypeID> messageTypeTable = new Dictionary<Type, MessageTypeID>();

		#region Serialize
		public byte[] Serialize<T>(T message)
			where T : ProtoBuf.IExtensible
		{
			var stream = new MemoryStream();
			Serialize(message, stream);
			return stream.ToArray();
		}

		public void Serialize<T>(T message, Stream dest)
			where T : ProtoBuf.IExtensible
		{
			Debug.Assert(ProtoBuf.Serializer.NonGeneric.CanSerialize(message.GetType()));

			MemoryStream messageBuf = new MemoryStream();
			SerializeImpl(messageBuf, message);

			var messageType = messageTypeTable[message.GetType()];
			MessagePackage package = new MessagePackage();
			package.category_id = messageType.CategoryID;
			package.type_id = messageType.TypeID;
			package.message = messageBuf.ToArray();

			SerializeImpl(dest, package);
		}

		private static void SerializeImpl<T>(Stream stream, T message)
			where T : ProtoBuf.IExtensible
		{
			// 以下方法实现中未能正确处理泛型类型探测，此处的T全部以ProtoBuf.IExtensible对待了。
			// 若有性能不足，以后可以仿deserializeTable的方式，缓存加速
			//ProtoBuf.Serializer.Serialize(stream, message);
			ProtoBuf.Serializer.NonGeneric.Serialize(stream, message);
		}
		#endregion

		#region Deserialize

		public ProtoBuf.IExtensible Deserialize(byte[] messagePackageData)
		{
			return DeserializeImpl(new MemoryStream(messagePackageData, 0, messagePackageData.Length));
		}

		public ProtoBuf.IExtensible Deserialize(byte[] messagePackageData, int offset, int count)
		{
			return DeserializeImpl(new MemoryStream(messagePackageData, offset, count));
		}

		private ProtoBuf.IExtensible DeserializeImpl(Stream stream)
		{
			MessagePackage package = ProtoBuf.Serializer.Deserialize<MessagePackage>(stream);

			MemoryStream messageBuf = new MemoryStream(package.message);
			return deserializeTable[new MessageTypeID() { CategoryID = package.category_id, TypeID = package.type_id }](messageBuf);
		}
		
		#endregion

		#region Register
		/// <summary>注册可被解析的消息类型</summary>
		/// <typeparam name="T">可被解析的消息类型ID</typeparam>
		/// <param name="messageType"><typeparamref name="T"/>对应的<see cref="ProtoBuf.IExtensible"/>类型</param>
		public void Register<T>(MessageTypeID messageType)
			where T : class, ProtoBuf.IExtensible
		{
			// 反序列化预编译
			ProtoBuf.Serializer.PrepareSerializer<T>();

			// 注册
			messageTypeTable[typeof(T)] = messageType;
			deserializeTable[messageType] = (stream) => ProtoBuf.Serializer.Deserialize<T>(stream);
		}

		/// <summary>注册可被解析的消息类型</summary>
		/// <param name="messageTypeID">可被解析的消息类型ID</param>
		/// <param name="messageType"><paramref name="messageTypeID"/>对应的<see cref="ProtoBuf.IExtensible"/>类型</param>
		/// <remarks>对泛型重载Register&lt;T&gt;的非泛型包装</remarks>
		public void Register(MessageTypeID messageTypeID, Type messageType)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod;
			MethodInfo method = this.GetType().GetMethod("Register", flags, null, new Type[] { typeof(MessageTypeID) }, null);
			method = method.MakeGenericMethod(messageType);
			method.Invoke(this, new object[] { messageTypeID });
		}
		#endregion

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var pair in messageTypeTable)
			{
				sb.AppendLine(pair.Value + ":" + pair.Key);
			}
			return sb.ToString();
		}
	}
}
