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

namespace EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var dispatcher = SimpleDispatcher.CurrentDispatcher;
            var serializer = new MessageSerializer();
            serializer.Register<Message>();

            // 初始化service
            var service = new MessageTcpService(serializer, (session, o) =>
            {
                dispatcher.Invoke(() =>
                {
                    session.Send(o);
                });
            });
            service.MessageDeserializeFailed += (e) => { Console.WriteLine(e.Message); };
            var ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
            service.Listen(new IPEndPoint(ip, 9528));
            dispatcher.Run();
        }
    }
}
