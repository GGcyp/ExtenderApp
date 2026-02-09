using System.Buffers;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 套字节链接器的抽象基类。
    /// </summary>
    public abstract class SocketLinker : Linker, ILinker, ILinkBind
    {
        /// <summary>
        /// 当前链接器所使用的 Socket 实例。
        /// </summary>
        internal Socket Socket { get; }

        public SocketLinker(Socket socket)
        {
            Socket = socket;
        }

        public override bool Connected => Socket.Connected;

        public override EndPoint? LocalEndPoint => Socket.LocalEndPoint;

        public override EndPoint? RemoteEndPoint => Socket.RemoteEndPoint;

        public override SocketType SocketType => Socket.SocketType;

        public override ProtocolType ProtocolType => Socket.ProtocolType;

        public override AddressFamily AddressFamily => Socket.AddressFamily;

        public void Bind(EndPoint endPoint)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                Socket.Bind(endPoint);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        protected override sealed ValueTask<Result<SocketOperationValue>> ExecuteSendAsync(Memory<byte> memory, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.SendAsync(Socket, memory, token);
        }

        protected override sealed ValueTask<Result<SocketOperationValue>> ExecuteReceiveAsync(Memory<byte> memory, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.ReceiveAsync(Socket, memory, token);
        }

        protected SocketException CreateSocketException(SocketError error)
        {
            return new SocketException((int)error);
        }

        protected override ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token)
        {
            return Socket.ConnectAsync(remoteEndPoint, token);
        }

        public override void SetOption(LinkOptionLevel optionLevel, LinkOptionName optionName, DataBuffer optionValue)
        {
            switch (optionValue)
            {
                case DataBuffer<byte> byteBuffer:
                    Socket.SetSocketOption((SocketOptionLevel)optionLevel, (SocketOptionName)optionName, byteBuffer);
                    break;

                case DataBuffer<int> intBuffer:
                    Socket.SetSocketOption((SocketOptionLevel)optionLevel, (SocketOptionName)optionName, intBuffer);
                    break;

                case DataBuffer<bool> boolBuffer:
                    Socket.SetSocketOption((SocketOptionLevel)optionLevel, (SocketOptionName)optionName, boolBuffer);
                    break;

                case DataBuffer<object> objectBuffer:
                    Socket.SetSocketOption((SocketOptionLevel)optionLevel, (SocketOptionName)optionName, objectBuffer);
                    break;

                default:
                    throw new ArgumentException("Unsupported option value type.", nameof(optionValue));
            }
        }

        public override ILinker Clone()
        {
            var socket = new Socket(AddressFamily, SocketType, ProtocolType);
            return Clone(socket);
        }

        protected abstract ILinker Clone(Socket socket);

        protected override async ValueTask DisposeAsyncManagedResources()
        {
            await base.DisposeAsyncManagedResources();
            Socket.Dispose();
        }

        protected override ValueTask ExecuteDisconnectAsync(CancellationToken token)
        {
            return Socket.DisconnectAsync(reuseSocket: false, token);
        }

        protected override Result<SocketOperationValue> ExecuteSendAsync(ReadOnlySpan<byte> span)
        {
            var length = Socket.Send(span);
            return Result.Success(new SocketOperationValue(length, RemoteEndPoint, default));
        }

        protected override ValueTask<Result<SocketOperationValue>> ExecuteSendAsync(IList<ArraySegment<byte>> buffer, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.SendAsync(Socket, buffer, token);
        }

        protected override ValueTask<Result<SocketOperationValue>> ExecuteReceiveAsync(IList<ArraySegment<byte>> buffer, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.ReceiveAsync(Socket, buffer, token);
        }
    }
}