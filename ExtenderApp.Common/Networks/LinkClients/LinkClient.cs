using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// LinkClient 类表示一个与 Linker 服务通信的客户端。
    /// 实现了 IDisposable 接口和 ILinker 接口。
    /// </summary>
    public class LinkClient<TLinker, TLinkParser> : DisposableObject, ILinker
        where TLinker : ILinker
        where TLinkParser : LinkParser
    {
        private readonly TLinker _linker;
        private TLinkParser Parser { get; }
        public TrafficRecorder Recorder { get; }

        public bool Connected => _linker.Connected;

        public LinkClient(TLinker linker, TLinkParser parser)
        {
            _linker = linker ?? throw new ArgumentNullException(nameof(linker));
            Parser = parser;
            Recorder = new TrafficRecorder();
            OnSendedTraffic += Recorder.RecordSend;
            OnReceivedTraffic += Recorder.RecordReceive;
            OnReceive += Parser.Receive;
        }

        public event Action<ILinker>? OnClose
        {
            add => _linker.OnClose += value;
            remove => _linker.OnClose -= value;
        }
        public event Action<ILinker>? OnConnect
        {
            add => _linker.OnConnect += value;
            remove => _linker.OnConnect -= value;
        }
        public event Action<LinkClient<TLinker, TLinkParser>>? OnCloseClient;
        public event Action<LinkClient<TLinker, TLinkParser>>? OnConnectClient;
        public event Action<string> OnErrored
        {
            add => _linker.OnErrored += value;
            remove => _linker.OnErrored -= value;
        }
        public event Action<byte[], int>? OnReceive
        {
            add => _linker.OnReceive += value;
            remove => _linker.OnReceive -= value;
        }
        public event Action<int>? OnSendingTraffic
        {
            add => _linker.OnSendingTraffic += value;
            remove => _linker.OnSendingTraffic -= value;
        }
        public event Action<int>? OnSendedTraffic
        {
            add => _linker.OnSendedTraffic += value;
            remove => _linker.OnSendedTraffic -= value;
        }
        public event Action<int>? OnReceiveingTraffic
        {
            add => _linker.OnReceiveingTraffic += value;
            remove => _linker.OnReceiveingTraffic -= value;
        }
        public event Action<int>? OnReceivedTraffic
        {
            add => _linker.OnReceivedTraffic += value;
            remove => _linker.OnReceivedTraffic -= value;
        }

        #region Connect

        public void Connect(string host, int port)
        {
            _linker.OnConnect += PrivateConnected;
            _linker.Connect(host, port);

        }

        public void Connect(IPAddress address, int port)
        {
            _linker.OnConnect += PrivateConnected;
            _linker.Connect(address, port);
        }
        public void ConnectAsync(string host, int port)
        {
            _linker.OnConnect += PrivateConnected;
            _linker.ConnectAsync(host, port);
        }

        public void ConnectAsync(IPAddress address, int port)
        {
            _linker.OnConnect += PrivateConnected;
            _linker.ConnectAsync(address, port);
        }

        public void ConnectAsync(EndPoint point)
        {
            _linker.OnConnect += PrivateConnected;
            _linker.ConnectAsync(point);
        }

        private void PrivateConnected(ILinker linker)
        {
            _linker.OnConnect -= PrivateConnected;
            OnConnectClient?.Invoke(this);
        }



        #endregion

        #region Send
        public void Send<TValue>(TValue value)
        {
            ThrowIfDisposed();
            if (Parser == null)
                throw new InvalidOperationException("没有传入链接数据解析器，不能使用此方法");
            Parser.Send(_linker, value);
        }

        public void Send(byte[] data)
        {
            _linker.Send(data);
        }

        public void Send(Memory<byte> memory)
        {
            _linker.Send(memory);
        }

        public void Send(byte[] data, int start, int length)
        {
            _linker.Send(data, start, length);
        }

        public void SendAsync<TValue>(TValue value)
        {
            ThrowIfDisposed();
            if (Parser == null)
                throw new InvalidOperationException("没有传入链接数据解析器，不能使用此方法");
            Parser.SendAsync(_linker, value);
        }

        public void SendAsync(Memory<byte> memory)
        {
            _linker.SendAsync(memory);
        }

        public void SendAsync(byte[] data)
        {
            _linker.SendAsync(data);
        }

        public void SendAsync(byte[] data, int start, int length)
        {
            _linker.SendAsync(data, start, length);
        }

        #endregion

        public void Close(bool requireFullTransmission = false, bool requireFullDataProcessing = false)
        {
            _linker.Close(requireFullTransmission, requireFullDataProcessing);
        }

        protected override void Dispose(bool disposing)
        {
            _linker.Dispose();

            base.Dispose(disposing);
        }
    }
}
