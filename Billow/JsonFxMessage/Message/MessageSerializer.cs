using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Eddy.JsonFxMessage
{
	/// <summary>
	/// <see cref="ProtoBuf.IExtensible"/>对象和字节流间的编码器
	/// </summary>
	/// <remarks></remarks>
	public class MessageSerializer : Eddy.Message.IMessageSerializer
	{
        private readonly Dictionary<string, Type> messageTypeTable = new Dictionary<string, Type>();

		#region Serialize
        public byte[] Serialize<T>(T message) where T : class
		{
			var stream = new MemoryStream();
			Serialize(message, stream);
			return stream.ToArray();
		}

        public void Serialize<T>(T message, Stream dest) where T : class
		{
            MessagePackage package = new MessagePackage();
            package.name = message.GetType().FullName;
            package.serialized = JsonFx.Json.JsonWriter.Serialize(message);
            var jsonWriter = new JsonFx.Json.JsonWriter (dest);
            jsonWriter.Write(package);
            jsonWriter.TextWriter.Flush();
            //dest.Write(JsonFx.Json.JsonWriter.Serialize(package));
            dest.Flush();
		}
     
		#endregion

		#region Deserialize

		public object Deserialize(byte[] messagePackageData)
		{
            return Deserialize(messagePackageData, 0, messagePackageData.Length);
		}

		public object Deserialize(byte[] messagePackageData, int offset, int count)
		{
            var stream = new MemoryStream(messagePackageData, offset, count);
            var reader = new JsonFx.Json.JsonReader(stream);
            var package = reader.Deserialize<MessagePackage>();
            var message = JsonFx.Json.JsonReader.Deserialize(package.serialized, messageTypeTable[package.name]);
            return message;
		}

		#endregion

		#region Register
		/// <summary>注册可被解析的消息类型</summary>
		/// <typeparam name="T">可被解析的消息类型ID</typeparam>
		/// <param name="messageType"><typeparamref name="T"/>对应的<see cref="ProtoBuf.IExtensible"/>类型</param>
		public void Register<T>() where T : class
		{
			// 注册
			messageTypeTable[typeof(T).FullName] = typeof(T);
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
