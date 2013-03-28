using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eddy.ProtoBufConnection.Net;
using Eddy.ProtoBufConnection.Message;
using ProtoBuf;
using Eddy;
using System.Net;

namespace EddyTest
{
    [TestClass]
    public class NetworkTest
    {
        public NetworkTest()
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

        ProtoBufTcpClient client;
        ProtoBufTcpService service;

        [ProtoContract]
        class Message
        {
            [ProtoMember(1)]
            public int ID { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestNetwork()
        {
            var dispatcher = SimpleDispatcher.CurrentDispatcher;
            var message = new Message { ID = 7, Name = "abc" };
            var serializer = new MessageSerializer();
            serializer.Register<Message>(new MessageTypeID { CategoryID = 1, TypeID = 1});

            bool done = false;

            // 初始化client
            client = new ProtoBufTcpClient(serializer, (session, o) =>
            {
                dispatcher.Invoke(() =>
                    {
                        Assert.AreEqual(message.ID, (o as Message).ID);
                        Assert.AreEqual(message.Name, (o as Message).Name);
                        done = true;
                        dispatcher.Shutdown();
                    });
            });
            client.Connected += () =>
                {
                    client.Send(message);
                };

            // 初始化service
            service = new ProtoBufTcpService(serializer, (session, o) =>
            {
                dispatcher.Invoke(() =>
                    {
                        Assert.AreEqual(message.ID, (o as Message).ID);
                        Assert.AreEqual(message.Name, (o as Message).Name);
                        session.Send(message);
                    });
            });

            var ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
            service.Listen(new IPEndPoint(ip, 9527));
            client.Connect(new IPEndPoint(ip, 9527));
            dispatcher.Run();
            Assert.IsTrue(done);
        }
    }
}
