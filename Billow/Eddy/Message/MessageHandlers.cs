using System;
using System.Diagnostics;
using System.IO;
using Eddy.Message;
using Eddy.Net;

namespace Eddy.Message
{
    /// <summary>
    /// 网络连接数据处理类，创建序列化和反序列化消息的delegate
    /// </summary>
    public class MessageHandlers
    {
        public MessageHandlers(IMessageSerializer serializer)
        {
            this.serializer = serializer;
        }

        /// <summary>
        /// 消息反序列化失败事件，需要保证线程安全
        /// </summary>
        public event Action<Exception> MessageDeserializeFailed;

        /// <summary>
        /// 反序列化接收到的数据，输出message 
        /// messageHandler需要保证线程安全
        /// </summary>
        public TcpSession.ReceivedHandler CreateReceivedHandler(TcpSession session, Action<TcpSession, object> messageHandler)
        {
            bool isInitialized = false;
            bool isReadingHeader = true;
            bool isPartial = false;
            var partialStream = new MemoryStream();
            TcpSession.ReceivedHandler handler = stream =>
            {
                if (!isInitialized)
                {
                    Debug.Assert(stream.Length == 0 && stream.Position == 0);
                    isInitialized = true;
                    return PackageHead.SizeOf;
                }
                if (isReadingHeader)
                {
                    PackageHead header = new PackageHead();
                    header.ReadFrom(stream.GetBuffer(), (int)stream.Position);
                    isReadingHeader = false;
                    isPartial = ((header.Flags & PackageHeadFlags.Partial) == PackageHeadFlags.Partial);
                    return header.MessageLength;
                }
                else
                {
                    if (isPartial) // 超大包未接收完全
                    {
                        // 在临时缓冲中拼包
                        stream.WriteTo(partialStream);
                    }
                    else if (partialStream.Length != 0) // 超大包最后一个分包
                    {
                        // 拼包然后反序列化
                        stream.WriteTo(partialStream);
                        object message = null;
                        try
                        {
                           message = this.serializer.Deserialize(partialStream.GetBuffer(), 0, (int)partialStream.Length);
                        }
                        catch (Exception e)
                        {
                            OnMessageDeserializeFailed(e);
                        }

                        if (message != null)
                            messageHandler(session, message);

                        partialStream.Position = 0;
                        partialStream.SetLength(0);
                    }
                    else // 普通大小的包
                    {
                        // 直接反序列化
                        object message = null;
                        try
                        {
                            message = this.serializer.Deserialize(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
                        }
                        catch (Exception e)
                        {
                            OnMessageDeserializeFailed(e);
                        }

                        if (message != null)
                            messageHandler(session, message);
                    }

                    isReadingHeader = true;
                    return PackageHead.SizeOf;
                }
            };

            return handler;
        }

        private void OnMessageDeserializeFailed(Exception e)
        {
            if (MessageDeserializeFailed != null)
                MessageDeserializeFailed(e);
        }

        /// <summary>
        /// 序列化要发送的message 
        /// </summary>
        public TcpSession.SendingHandler CreateSendingHandler()
        {
            TcpSession.SendingHandler handler = (messages, stream) =>
            {
                foreach (var message in messages)
                {
                    // 保留包头空间
                    var headerPosition = stream.Position;
                    stream.SetLength(stream.Length + PackageHead.SizeOf);
                    stream.Position += PackageHead.SizeOf;
                    // 序列化消息
                    serializer.Serialize(message, stream);

                    PackageHead header = new PackageHead();
                    var length = stream.Length - (headerPosition + PackageHead.SizeOf);
                    // 超大包
                    if (length > ushort.MaxValue)
                    {
                        // 写入第一个包的包头
                        header.Flags |= PackageHeadFlags.Partial;
                        header.MessageLength = ushort.MaxValue;
                        header.WriteTo(stream.GetBuffer(), (int)headerPosition);
                        length -= ushort.MaxValue;

                        // 从第一个包末尾截断，把剩余数据拷到临时缓冲中
                        var keptSize = headerPosition + ushort.MaxValue + PackageHead.SizeOf;
                        var dataLeft = new byte[stream.Length - keptSize];
                        var offset = 0;
                        stream.Position = keptSize;
                        stream.Read(dataLeft, 0, dataLeft.Length);
                        stream.Position = keptSize;
                        stream.SetLength(keptSize);

                        // 从临时缓冲中把剩余数据加上包头写入
                        while (length > 0)
                        {
                            ushort truncateLength = ushort.MaxValue;
                            // 最后的部分
                            if (length <= ushort.MaxValue)
                            {
                                truncateLength = (ushort)length;
                                header.Flags &= ~PackageHeadFlags.Partial;
                            }
                            header.MessageLength = truncateLength;
                            header.WriteTo(stream);
                            stream.Write(dataLeft, offset, truncateLength);
                            offset += truncateLength;
                            length -= truncateLength;
                        }
                        Debug.Assert(offset == dataLeft.Length);
                    }
                    else // 普通大小的包
                    {
                        // 写入包头
                        header.MessageLength = (ushort)length;
                        header.WriteTo(stream.GetBuffer(), (int)headerPosition);
                    }
                }
            };
            return handler;
        }

        private readonly IMessageSerializer serializer;
    }
}

