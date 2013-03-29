using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace Eddy.Net
{
    /// <summary>
    /// 网络连接会话类 
    /// </summary>
    public class TcpSession
    {
        private ReceivedHandler handleReceived;
        private SendingHandler handleSending;
        private Socket socket;
        private MemoryStream sendStream = new MemoryStream(128);
        private MemoryStream receiveStream = new MemoryStream(128);
        private Queue<object> messagesToBeSent = new Queue<object>();
        private Queue<object> messagesSending = new Queue<object>();
        private volatile bool isSending = false;
        private volatile bool isReceiving = false;
        private volatile bool isConnected = false;
        private volatile Exception asyncException = null;
        private readonly SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
        private readonly SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();

        public const int DefaultBufferSize = 128;

        public bool IsConnected
        {
            get { return socket.Connected; }
        }

        /// <summary>
        /// 接收数据处理，必须保证线程安全
        /// 返回下次想要receive的字节数
        /// </summary>
        public delegate int ReceivedHandler(MemoryStream stream);

        /// <summary>
        /// 发送数据处理，必须保证线程安全
        /// 把messages序列化到buffer中
        /// 返回序列化后的字节数
        /// </summary>
        public delegate void SendingHandler(Queue<object> messages, MemoryStream stream);

        public void Initialize(ReceivedHandler receivedEventHandler, SendingHandler sendingEventHandler)
        {
            sendArgs.UserToken = this;
            receiveArgs.UserToken = this;
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SendCallback);
            receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCallback);
            handleReceived = receivedEventHandler;
            handleSending = sendingEventHandler;
        }

        public event Action Connected;
        public event Action<Exception> Disconnected;

        public void Send(object message)
        {
            lock (this)
            {
                if (asyncException != null)
                    return;

                if (!isConnected)
                    return;

                if (!isSending)
                {
                    isSending = true;
                    messagesToBeSent.Enqueue(message);
                    sendArgs.SocketError = SocketError.Success;
                    SendCallback(null, sendArgs);
                }
                else
                {
                    messagesToBeSent.Enqueue(message);
                }
            }
        }

        /// <summary>
        /// 阻塞式断开连接，确保socket缓冲区的数据发送
        /// </summary>
        /// <param name="timeout">超时参数</param>
        public void BlockingDisconnect(int timeout)
        {
            if (!isConnected)
                return;

            while (true)
            {
                if (!isSending)
                {
                    lock (this)
                    {
                        if (!isConnected)
                            return;

                        if (isSending)
                            continue;

                        isConnected = false;
                        socket.LingerState = new LingerOption(true, timeout);
                        socket.Close(timeout);
                        if (Disconnected != null)
                            Disconnected(null);
                        return;
                    }
                }
                Thread.Sleep(0);
            }
        }

        public void Disconnect()
        {
            Disconnect(null);
        }

        private void Disconnect(Exception e)
        {
            if (!isConnected)
                return;

            lock (this)
            {
                if (!isConnected)
                    return;
                isConnected = false;
                if (!isSending)
                    DoClose(e);
            }
        }

        private void DoClose(Exception e)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                if (Disconnected != null)
                    Disconnected(e);
            }
            catch
            {
            }
        }

        internal void SetSocket(Socket socket)
        {
            try
            {
                Debug.Assert(this.socket == null);
                // Initialize socket
                this.socket = socket;
                this.isConnected = true;
                socket.NoDelay = true;
                socket.SendBufferSize = 128 * 1024;
                socket.ReceiveBufferSize = 128 * 1024;

                // start receiving
                int size = handleReceived(receiveStream);
                receiveStream.Position = 0;
                receiveStream.SetLength(size);
                receiveArgs.SetBuffer(receiveStream.GetBuffer(), 0, (int)receiveStream.Length);
                isReceiving = true;
                if (!socket.ReceiveAsync(receiveArgs))
                    ReceiveCallback(null, receiveArgs);

                if (Connected != null)
                    Connected();
            }
            catch (Exception e)
            {
                Disconnect(e);
            }
        }

        private void SendCallback(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                try
                {
                    var bytesSent = args.BytesTransferred;
                    var stream = sendStream;
                    var bytesShouldBeSent = stream.Length - stream.Position;
                    var bytesLeft = bytesShouldBeSent - bytesSent;
                    if (bytesLeft > 0)
                    {
                        stream.Position += bytesSent;
                        args.SetBuffer(stream.GetBuffer(), (int)stream.Position, (int)bytesLeft);
                        if (!socket.SendAsync(args))
                            SendCallback(null, args);
                        return;
                    }
                    Debug.Assert(messagesSending.Count == 0);
                    Debug.Assert(isSending);
                    lock (this)
                    {
                        Utility.Swap(ref messagesSending, ref messagesToBeSent);
                        if (messagesSending.Count == 0)
                        {
                            isSending = false;
                            if (!isConnected)
                            {
                                DoClose(null);
                            }
                            else if (asyncException != null)
                            {
                                Debug.Assert(!isReceiving);
                                Disconnect(asyncException);
                                asyncException = null;
                            }
                            return;
                        }
                    }
                    stream.Position = 0;
                    stream.SetLength(0);
                    handleSending(messagesSending, stream);
                    messagesSending.Clear();
                    args.SetBuffer(stream.GetBuffer(), 0, (int)stream.Length);
                    bool needCallback = false;
                    try
                    {
                        needCallback = !socket.SendAsync(args);
                    }
                    catch (ObjectDisposedException)
                    {
                        isSending = false;
                        needCallback = false;
                    }
                    if (needCallback)
                        SendCallback(null, args);

                }
                catch (Exception e)
                {
                    ProcessSendingFailed(e);
                }
            }
            else
            {
                ProcessSendingFailed(new SocketException((int)args.SocketError));
            }
        }

        private void ProcessSendingFailed(Exception e)
        {
            isSending = false;

            if (!isConnected)
                return;

            lock (this)
            {
                if (!isConnected)
                    return;

                if (asyncException != null)
                {
                    Debug.Assert(!isReceiving);
                    Disconnect(asyncException);
                    asyncException = null;
                }
                else
                {
                    if (isReceiving)
                        asyncException = e;
                    else
                        Disconnect(e);
                }
            }
        }
        private void ReceiveCallback(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    var bytesRead = args.BytesTransferred;
                    var stream = receiveStream;
                    var bytesShouldBeRead = stream.Length - stream.Position;
                    var bytesLeft = bytesShouldBeRead - bytesRead;
                    // incomplete read
                    if (bytesLeft > 0)
                    {
                        stream.Position += bytesRead;
                        args.SetBuffer(stream.GetBuffer(), (int)stream.Position, (int)bytesLeft);
                        if (!socket.ReceiveAsync(args))
                            ReceiveCallback(null, args);
                        return;
                    }
                    stream.Position = 0;
                    var size = handleReceived(stream);
                    stream.Position = 0;
                    stream.SetLength(size);
                    args.SetBuffer(stream.GetBuffer(), 0, (int)stream.Length);
                    bool needCallback = false;
                    try
                    {
                        needCallback = !socket.ReceiveAsync(args);
                    }
                    catch (ObjectDisposedException)
                    {
                        isReceiving = false;
                        needCallback = false;
                    }
                    if (needCallback)
                        ReceiveCallback(null, args);
                }
                catch (Exception e)
                {
                    ProcessReceivingFailed(e);
                }
            }
            else
            {
                ProcessReceivingFailed(new SocketException((int)args.SocketError));
            }
        }

        private void ProcessReceivingFailed(Exception e)
        {
            isReceiving = false;
            if (!isConnected)
                return;

            lock (this)
            {
                if (!isConnected)
                    return;

                if (asyncException != null)
                {
                    Debug.Assert(!isSending);
                    Disconnect(asyncException);
                    asyncException = null;
                }
                else
                {
                    if (isSending)
                        asyncException = e;
                    else
                        Disconnect(e);
                }
            }
        }
    }
}
