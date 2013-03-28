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
	public interface IMessageSerializer
	{

		#region Serialize
        byte[] Serialize<T>(T message) where T : class;

        void Serialize<T>(T message, Stream dest) where T : class;

		#endregion

		#region Deserialize

		object Deserialize(byte[] messagePackageData);

		object Deserialize(byte[] messagePackageData, int offset, int count);
		
		#endregion
	}
}
