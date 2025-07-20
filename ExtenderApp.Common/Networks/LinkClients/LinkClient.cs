using System;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 表示一个抽象的LinkClient类，该类继承自DisposableObject并实现ILinker接口。
    /// </summary>
    public abstract class LinkClient : DisposableObject, ILinker
    {
        /// <summary>
        /// 最大发送速率（字节/秒）
        /// </summary>
        public long MaxSendRate { get; set; } = long.MaxValue;

        /// <summary>
        /// 最大接收速率（字节/秒）
        /// </summary>
        public long MaxReceiveRate { get; set; } = long.MaxValue;

        /// <summary>
        /// 获取或设置最大接收大小。
        /// </summary>
        /// <remarks>
        /// 默认值为10MB。
        /// </remarks>
        public long MaxReceiveSize { get; set; } = Utility.MegabytesToBytes(10); // 默认10MB

        /// <summary>
        /// 获取或设置最大发送大小。
        /// </summary>
        /// <remarks>
        /// 默认值为10MB。
        /// </remarks>
        public long MaxSendSize { get; set; } = Utility.MegabytesToBytes(10); // 默认10MB

        public abstract bool Connected { get; }

        public abstract EndPoint RemoteEndPoint { get; }

        public abstract event Action<ILinker>? OnClose;
        public abstract event Action<ILinker>? OnConnect;
        public abstract event Action<Exception> OnErrored;
        public abstract event Action<byte[], int>? OnReceive;

        public abstract void Close(bool requireFullTransmission = false);

        public abstract void Connect(string host, int port);

        public abstract void Connect(IPAddress address, int port);

        public abstract void Connect(Uri uri);

        public abstract void Connect(EndPoint point);

        public abstract void ConnectAsync(Uri uri);

        public abstract void ConnectAsync(string host, int port);

        public abstract void ConnectAsync(IPAddress address, int port);

        public abstract void ConnectAsync(EndPoint point);

        public abstract void Send(byte[] data);

        public abstract void Send(byte[] data, int start, int length);

        public abstract void Send(Memory<byte> memory);

        public abstract void Send<TValue>(TValue value);

        public abstract void SendAsync<TValue>(TValue value);

        public abstract void Send(ExtenderBinaryWriter writer);

        public abstract void SendAsync(byte[] data);

        public abstract void SendAsync(byte[] data, int start, int length);

        public abstract void SendAsync(Memory<byte> memory);

        public abstract void SendAsync(ExtenderBinaryWriter writer);
    }

    /// <summary>
    /// 泛型链接客户端类，用于处理特定类型的链接器和链接解析器。
    /// </summary>
    /// <typeparam name="TLinker">实现ILinker接口的链接器类型。</typeparam>
    /// <typeparam name="TLinkParser">实现LinkParser接口的链接解析器类型。</typeparam>
    public class LinkClient<TLinker, TLinkParser> : LinkClient
        where TLinker : ILinker
        where TLinkParser : LinkParser
    {
        /// <summary>
        /// 私有只读属性，表示TLinker实例。
        /// </summary>
        public TLinker Linker { get; }

        /// <summary>
        /// 公共属性，表示TLinkParser实例。
        /// </summary>
        public TLinkParser Parser { get; }

        public override bool Connected => Linker.Connected;

        public override EndPoint RemoteEndPoint => Linker.RemoteEndPoint;

        public LinkClient(TLinker linker, TLinkParser parser)
        {
            this.Linker = linker;
            Parser = parser;
            OnReceive += Parser.Receive;
        }

        public override event Action<ILinker>? OnClose
        {
            add => Linker.OnClose += value;
            remove => Linker.OnClose -= value;
        }
        public override event Action<ILinker>? OnConnect
        {
            add => Linker.OnConnect += value;
            remove => Linker.OnConnect -= value;
        }
        public override event Action<Exception> OnErrored
        {
            add => Linker.OnErrored += value;
            remove => Linker.OnErrored -= value;
        }
        public override event Action<byte[], int>? OnReceive
        {
            add => Linker.OnReceive += value;
            remove => Linker.OnReceive -= value;
        }
        public event Action<LinkClient<TLinker, TLinkParser>>? OnCloseClient;
        public event Action<LinkClient<TLinker, TLinkParser>>? OnConnectClient;

        #region Connect

        public override void Connect(string host, int port)
        {
            Linker.OnConnect += PrivateConnected;
            Linker.Connect(host, port);

        }

        public override void Connect(IPAddress address, int port)
        {
            Linker.OnConnect += PrivateConnected;
            Linker.Connect(address, port);
        }

        public override void Connect(Uri uri)
        {
            Linker.OnConnect += PrivateConnected;
            Linker.Connect(uri);
        }

        public override void Connect(EndPoint point)
        {
            Linker.OnConnect += PrivateConnected;
            Linker.Connect(point);
        }

        public override void ConnectAsync(Uri uri)
        {
            Linker.OnConnect += PrivateConnected;
            Linker.ConnectAsync(uri);
        }

        public override void ConnectAsync(string host, int port)
        {
            Linker.OnConnect += PrivateConnected;
            Linker.ConnectAsync(host, port);
        }

        public override void ConnectAsync(IPAddress address, int port)
        {
            Linker.OnConnect += PrivateConnected;
            Linker.ConnectAsync(address, port);
        }

        public override void ConnectAsync(EndPoint point)
        {
            Linker.OnConnect += PrivateConnected;
            Linker.ConnectAsync(point);
        }

        private void PrivateConnected(ILinker linker)
        {
            this.Linker.OnConnect -= PrivateConnected;
            OnConnectClient?.Invoke(this);
        }

        #endregion

        #region Send

        public override void Send<TValue>(TValue value)
        {
            ThrowIfDisposed();
            if (Parser == null)
                throw new InvalidOperationException("没有传入链接数据解析器，不能使用此方法");
            Parser.Send(Linker, value);
        }

        public override void Send(byte[] data)
        {
            Linker.Send(data);
        }

        public override void Send(Memory<byte> memory)
        {
            Linker.Send(memory);
        }

        public override void Send(byte[] data, int start, int length)
        {
            Linker.Send(data, start, length);
        }

        public override void Send(ExtenderBinaryWriter writer)
        {
            Linker.Send(writer);
        }

        public override void SendAsync(ExtenderBinaryWriter writer)
        {
            Linker.SendAsync(writer);
        }

        public override void SendAsync<TValue>(TValue value)
        {
            ThrowIfDisposed();
            if (Parser == null)
                throw new InvalidOperationException("没有传入链接数据解析器，不能使用此方法");
            Parser.SendAsync(Linker, value);
        }

        public override void SendAsync(Memory<byte> memory)
        {
            Linker.SendAsync(memory);
        }

        public override void SendAsync(byte[] data)
        {
            Linker.SendAsync(data);
        }

        public override void SendAsync(byte[] data, int start, int length)
        {
            Linker.SendAsync(data, start, length);
        }

        #endregion

        public override void Close(bool requireFullTransmission = false)
        {
            Linker.Close(requireFullTransmission);
        }

        protected override void Dispose(bool disposing)
        {
            Linker.Dispose();

            base.Dispose(disposing);
        }
    }
}
