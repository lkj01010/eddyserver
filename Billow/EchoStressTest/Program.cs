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
            string url = "127.0.0.1";// "tyrenus.gamextasy.com";
            int num = 2048;

            if (args.Length >= 1)
            {
                url = args[0];
            }

            if (args.Length >= 2)
            {
                num = int.Parse(args[1]);
            }

            var clients = new List<MessageTcpClient>();
            var dispatcher = SimpleDispatcher.CurrentDispatcher;
            var serializer = new MessageSerializer();
            serializer.Register<Message>();

            for (int i = 0; i < num; ++i)
            {
                var message = new Message { ID = i, Name = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" };

                // 初始化client
                var client = new MessageTcpClient(serializer, (session, o) =>
                {
                    dispatcher.Invoke(() =>
                    {
                        //Console.WriteLine((o as Message).ID);
                        session.Send(o);
                    });
                });
                var id = i;
                client.Connected += () =>
                {
                    Console.WriteLine("connected" + id);
                    client.Send(message);
                };
                client.Disconnected += (e) =>
                {
                    Console.WriteLine("disconnected:" + id + " " + e.Message);
                };
                client.MessageDeserializeFailed += (e) =>
                {
                    Console.WriteLine("MessageDeserializeFailed:" + id + " " + e.Message);
                };

                var ip = System.Net.Dns.GetHostAddresses(url);
                //Console.WriteLine(ip.Length);
                //Console.WriteLine(ip[0]);
                client.Connect(new IPEndPoint(ip[0], 9528));
                clients.Add(client);
                System.Threading.Thread.Sleep(30);
            }
            var timer = new Eddy.Timers.Timer();
            timer.Tick += () =>
            {
                Console.WriteLine("{0} clients connected.", clients.Where (x => x.IsConnected).Count ());
            };
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();
            dispatcher.Run();
        }
    }
}
