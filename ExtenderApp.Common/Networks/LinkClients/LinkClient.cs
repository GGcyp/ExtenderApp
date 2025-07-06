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
        public abstract bool Connected { get; }

        public abstract event Action<ILinker>? OnClose;
        public abstract event Action<ILinker>? OnConnect;
        public abstract event Action<Exception> OnErrored;
        public abstract event Action<byte[], int>? OnReceive;
        public abstract event Action<int>? OnSendedTraffic;
        public abstract event Action<int>? OnReceiveingTraffic;
        public abstract event Action<int>? OnReceivedTraffic;

        public abstract void Close(bool requireFullTransmission = false, bool requireFullDataProcessing = false);

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

        public abstract void SendWriter(ExtenderBinaryWriter writer);

        public abstract void SendAsync(byte[] data);

        public abstract void SendAsync(byte[] data, int start, int length);

        public abstract void SendAsync(Memory<byte> memory);

        public abstract void SendAsyncWriter(ExtenderBinaryWriter writer);
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

        /// <summary>
        /// 公共属性，表示TrafficRecorder实例。
        /// </summary>
        public TrafficRecorder Recorder { get; }

        public override bool Connected => Linker.Connected;

        public LinkClient(TLinker linker, TLinkParser parser)
        {
            this.Linker = linker;
            Parser = parser;
            Recorder = new TrafficRecorder();
            OnSendedTraffic += Recorder.RecordSend;
            OnReceivedTraffic += Recorder.RecordReceive;
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
        public override event Action<int>? OnSendedTraffic
        {
            add => Linker.OnSendedTraffic += value;
            remove => Linker.OnSendedTraffic -= value;
        }
        public override event Action<int>? OnReceiveingTraffic
        {
            add => Linker.OnReceiveingTraffic += value;
            remove => Linker.OnReceiveingTraffic -= value;
        }
        public override event Action<int>? OnReceivedTraffic
        {
            add => Linker.OnReceivedTraffic += value;
            remove => Linker.OnReceivedTraffic -= value;
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

        public override void SendWriter(ExtenderBinaryWriter writer)
        {
            Linker.SendWriter(writer);
        }

        public override void SendAsyncWriter(ExtenderBinaryWriter writer)
        {
            Linker.SendAsyncWriter(writer);
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

        public override void Close(bool requireFullTransmission = false, bool requireFullDataProcessing = false)
        {
            Linker.Close(requireFullTransmission, requireFullDataProcessing);
        }

        protected override void Dispose(bool disposing)
        {
            Linker.Dispose();

            base.Dispose(disposing);
        }
    }
}
