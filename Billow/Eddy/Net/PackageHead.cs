using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Eddy.Net
{
	/// <summary>
	/// 网络封包包头
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PackageHead
	{
		/// <summary>
		/// 有效负载的类型
		/// </summary>
		public PackageHeadFlags Flags;
		/// <summary>
		/// 随后附带的有效负载字节长度
		/// </summary>
		public ushort MessageLength;


		private readonly static int s_size;
		/// <summary>
		/// 包头的字节大小
		/// </summary>
		public static int SizeOf { get { return s_size; } }

		static PackageHead()
		{
            s_size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PackageHead));
		}

		public void WriteTo(System.IO.Stream stream)
		{
			byte[] buf = new byte[SizeOf];
			WriteTo(buf, 0);
			stream.Write(buf, 0, buf.Length);
		}

		public void WriteTo(byte[] buf, int index)
		{
            var length = BitConverter.GetBytes(this.MessageLength);
            buf[index] = (byte)this.Flags;
            length.CopyTo(buf, index + sizeof(PackageHeadFlags));
		}

		public void ReadFrom(byte[] buf, int offset)
		{
            this.Flags = (PackageHeadFlags)buf[offset];
            this.MessageLength = BitConverter.ToUInt16(buf, offset + sizeof(PackageHeadFlags));
		}

		private static void CheckBuf(byte[] buf, int index)
		{
			if (buf == null)
				throw new ArgumentNullException("buf");
			if (index < 0 || index + SizeOf > buf.Length)
				throw new ArgumentOutOfRangeException("index");
		}

		public override string ToString()
		{
			return string.Format("{{Flag=0x{0:X4},MessageLength={1}}}", Flags, MessageLength);
		}
	}
}
