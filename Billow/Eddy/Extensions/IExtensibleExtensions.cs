using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Eddy.Extensions
{
	public static class IExtensibleExtent
	{
		public static bool BinaryEquals<T>(this T my, T other)
			where T : ProtoBuf.IExtensible
		{
			if (object.ReferenceEquals(my, other))
				return true;
			if (other == null)
				return false;
			
			MemoryStream stream1 = new MemoryStream();
			MemoryStream stream2 = new MemoryStream();
			ProtoBuf.Serializer.NonGeneric.Serialize(stream1, my);
			ProtoBuf.Serializer.NonGeneric.Serialize(stream2, other);
			if (stream1.Length != stream2.Length)
				return false;

			// memcmp
			byte[] b1 = stream1.GetBuffer();
			byte[] b2 = stream2.GetBuffer();
			long len = stream1.Length;
			for (long i = 0; i < len; i++)
			{
				if (b1[i] != b2[i])
					return false;
			}
			return true;
		}
	}
}
