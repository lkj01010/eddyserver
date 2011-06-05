using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Tables
{
	[ProtoContract]
	[Tables.Table("Test.xls", TableLocations.Client | TableLocations.Server)]
	public class Test
	{
		[ProtoMember(1, Name = "A")]
		public int A { get; set; }

		[ProtoMember(2, Name = "B")]
		public string B { get; set; }

		[ProtoMember(3)]
		public float C { get; set; }

		[ProtoMember(4, Name = @"测试")]
		public bool D { get; set; }
	}
}
