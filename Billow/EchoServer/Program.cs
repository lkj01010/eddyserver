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
            HashSet<TcpSession> sessions = new HashSet<TcpSession>();

            // 初始化service
            var service = new MessageTcpService(serializer, (session, o) =>
            {
                dispatcher.Invoke(() =>
                {
                    sessions.Add(session);
                    session.Disconnected += (e) =>
                    {
                        Console.WriteLine("Disconnected:" + e.Message);
                        sessions.Remove(session);
                    };
                    session.Send(o);
                });
            });
            service.MessageDeserializeFailed += (e) => { Console.WriteLine(e.Message); };
            var ip = IPAddress.Any;
            service.Listen(new IPEndPoint(ip, 9528));
            var timer = new Eddy.Timers.Timer();
            timer.Tick += () =>
            {
                Console.WriteLine("{0} clients connected.", sessions.Count);
            };
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();
            dispatcher.Run();
        }
    }
}
