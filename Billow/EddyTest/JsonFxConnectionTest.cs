using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Eddy.Net;
using Eddy.JsonFxMessage;
using Eddy.Message;
using Eddy;
using System.Net;

namespace EddyTest
{
    [TestClass]
    public class JsonFxTest
    {
        public JsonFxTest()
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

        MessageTcpClient client;
        MessageTcpService service;

        class Message
        {
            public int ID;
            public string Name;
        }

        [TestMethod]
        public void TestNetwork()
        {
            var dispatcher = SimpleDispatcher.CurrentDispatcher;
            var message = new Message { ID = 7, Name = "abc" };
            var serializer = new MessageSerializer();
            serializer.Register<Message>();

            bool done = false;

            // 初始化client
            client = new MessageTcpClient(serializer, (session, o) =>
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
            service = new MessageTcpService(serializer, (session, o) =>
            {
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(message.ID, (o as Message).ID);
                    Assert.AreEqual(message.Name, (o as Message).Name);
                    Console.WriteLine(message);
                    session.Send(message);
                });
            });

            var ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
            service.Listen(new IPEndPoint(ip, 9528));
            client.Connect(new IPEndPoint(ip, 9528));
            dispatcher.Run();
            Assert.IsTrue(done);
        }
    }
}
