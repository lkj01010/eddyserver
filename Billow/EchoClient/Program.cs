using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eddy;
using Eddy.JsonFxMessage;
using System.Net;
using Eddy.Net;

class Message
{
    public int ID;
    public string Name;
}

namespace EchoClient
{

    class Program
    {
        static void Main(string[] args)
        {
            SimpleDispatcher dispatcher;
            MessageTcpClient client;
            dispatcher = SimpleDispatcher.CurrentDispatcher;
            var message = new Message { ID = 7, Name = "abc" };
            var serializer = new MessageSerializer();
            serializer.Register<Message>();

            // 初始化client
            client = new MessageTcpClient(serializer, (session, o) =>
            {
                dispatcher.Invoke(() =>
                {
                    Console.WriteLine((o as Message).ID);
                    Console.WriteLine((o as Message).Name);
                });
            });
            client.Connected += () =>
            {
                Console.WriteLine("connected");
                client.Send(message);
            };
            client.Disconnected += (e) =>
            {
                Console.WriteLine("disconnected:" + e.Message);
            };

            var ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
            client.Connect(new IPEndPoint(ip, 9528));
            dispatcher.Run();
        }
    }

}
