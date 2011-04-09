using Eddy.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace Eddy.Test
{
    
    
    /// <summary>
    ///这是 PackageHeadTest 的测试类，旨在
    ///包含所有 PackageHeadTest 单元测试
    ///</summary>
	[TestClass()]
	public class PackageHeadTest
	{


		private TestContext testContextInstance;

		/// <summary>
		///获取或设置测试上下文，上下文提供
		///有关当前测试运行及其功能的信息。
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region 附加测试属性
		// 
		//编写测试时，还可使用以下属性:
		//
		//使用 ClassInitialize 在运行类中的第一个测试前先运行代码
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//使用 ClassCleanup 在运行完类中的所有测试后再运行代码
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//使用 TestInitialize 在运行每个测试前先运行代码
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//使用 TestCleanup 在运行完每个测试后运行代码
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///WriteTo 的测试
		///</summary>
		[TestMethod()]
		public void WriteReadTest()
		{
			PackageHead target = new PackageHead();
			PackageHead other = new PackageHead();
			byte[] temp = new byte[PackageHead.SizeOf];

			target.Flags = ushort.MinValue;
			target.MessageLength = ushort.MinValue;
			target.WriteTo(temp, 0);
			other.ReadFrom(temp, 0);
			Assert.AreEqual(target, other);

			target.Flags = (PackageHeadFlags)ushort.MaxValue;
			target.MessageLength = ushort.MaxValue;
			target.WriteTo(temp, 0);
			other.ReadFrom(temp, 0);
			Assert.AreEqual(target, other);

			Random random = new Random();
			int index = random.Next(100);
			temp = new byte[PackageHead.SizeOf + 100];
			target.Flags = (PackageHeadFlags)12;
			target.MessageLength = 16;
			target.WriteTo(temp, index);
			other.ReadFrom(temp, index);
			Assert.AreEqual(target, other);
		}
	}
}
