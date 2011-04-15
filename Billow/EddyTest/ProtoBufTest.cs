using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using System.IO;

namespace EddyTest
{
    [TestClass]
    public class ProtoBufTest
    {
        public ProtoBufTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
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

        [ProtoContract]
        class Serializee
        {
            [ProtoMember(1)]
            public int ID { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public List<float> Values { get; set; }
        }
        [TestMethod]
        public void TestSerializer()
        {
            var serializee = new Serializee { ID = 3, Name = "xyz" };
            var stream = new MemoryStream();
            serializee.Values = new List<float>();
            serializee.Values.Add(1.0f);
            serializee.Values.Add(2.0f);
            serializee.Values.Add(3.0f);
            Serializer.Serialize<Serializee>(stream, serializee);

            stream.Position = 0;
            var newSerializee = Serializer.Deserialize<Serializee>(stream);
            Assert.AreEqual(serializee.ID, newSerializee.ID);
            Assert.AreEqual(serializee.Name, newSerializee.Name);

            Assert.IsTrue(serializee.Values.SequenceEqual(newSerializee.Values));
            Assert.AreEqual(serializee.Values.Count, newSerializee.Values.Count);
        }
    }
}
