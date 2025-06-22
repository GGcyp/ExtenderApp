using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// LinkClient 类表示一个与 Linker 服务通信的客户端。
    /// 实现了 IDisposable 接口和 ILinker 接口。
    /// </summary>
    public class LinkClient : DisposableObject, ILinker
    {
        private readonly ILinker _linker;
        private LinkParser? linkParser;
        public TrafficRecorder Recorder;

        public bool Connected => _linker.Connected;

        public LinkClient(ILinker linker)
        {
            _linker = linker ?? throw new ArgumentNullException(nameof(linker));
            Recorder = new TrafficRecorder();
            OnSendedTraffic += Recorder.RecordSend;
            OnReceivedTraffic += Recorder.RecordReceive;
            OnReceive += ProtectedReceive;
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
            _linker.Connect(host, port);
        }

        public void Connect(IPAddress address, int port)
        {
            _linker.Connect(address, port);
        }

        #endregion

        #region Send

        public void Send<T>(T value)
        {
            ThrowIfDisposed();
            if (linkParser == null)
                throw new InvalidOperationException("LinkParser is not set.");
            linkParser.Send(_linker, value);
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

        #region Receive

        public T Deserialize<T>(byte[] data, int length)
        {
            ThrowIfDisposed();
            if (linkParser == null)
                throw new InvalidOperationException("LinkParser is not set.");
            return linkParser.Deserialize<T>(data);
        }

        protected virtual void ProtectedReceive(byte[] data, int length)
        {

        }

        #endregion

        public void Set(Socket socket)
        {
            _linker.Set(socket);
        }

        public void SetLinkParser(LinkParser linkParser)
        {
            this.linkParser = linkParser ?? throw new ArgumentNullException(nameof(linkParser));
        }

        public void Close(bool requireFullTransmission = false, bool requireFullDataProcessing = false)
        {
            _linker.Close(requireFullTransmission, requireFullDataProcessing);
        }

        public bool TryReset()
        {
            throw new NotImplementedException();
        }
    }
}
