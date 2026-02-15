using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 套字节链接器的抽象基类。
    /// </summary>
    public abstract class SocketLinker : Linker
    {
        /// <summary>
        /// 当前链接器所使用的 Socket 实例。
        /// </summary>
        internal Socket Socket { get; }

        public SocketLinker(Socket socket)
        {
            Socket = socket;
        }

        #region Info

        public override sealed bool Connected => Socket.Connected;

        public override sealed EndPoint? LocalEndPoint => Socket.LocalEndPoint;

        public override sealed EndPoint? RemoteEndPoint => Socket.RemoteEndPoint;

        public override sealed SocketType SocketType => Socket.SocketType;

        public override sealed ProtocolType ProtocolType => Socket.ProtocolType;

        public override sealed AddressFamily AddressFamily => Socket.AddressFamily;

        public override sealed int ReceiveBufferSize { get => Socket.ReceiveBufferSize; set => Socket.ReceiveBufferSize = value; }
        public override sealed int SendBufferSize { get => Socket.SendBufferSize; set => Socket.SendBufferSize = value; }
        public override sealed int ReceiveTimeout { get => Socket.ReceiveTimeout; set => Socket.ReceiveTimeout = value; }
        public override sealed int SendTimeout { get => Socket.SendTimeout; set => Socket.SendTimeout = value; }

        #endregion Info

        protected override void ExecuteBind(EndPoint endPoint)
        {
            Socket.Bind(endPoint);
        }

        protected override ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token)
        {
            return Socket.ConnectAsync(remoteEndPoint, token);
        }

        protected override ValueTask ExecuteDisconnectAsync(CancellationToken token)
        {
            return Socket.DisconnectAsync(reuseSocket: false, token);
        }

        #region Send

        protected override sealed Result<SocketOperationValue> ExecuteSend(ReadOnlySpan<byte> span, LinkFlags flags)
        {
            var length = Socket.Send(span, (SocketFlags)flags, out var errorCode);

            if (TryGetSocketError(errorCode, out var ex))
                return Result.FromException<SocketOperationValue>(ex);
            return CreateOperationValue(length);
        }

        protected override sealed ValueTask<Result<SocketOperationValue>> ExecuteSendAsync(Memory<byte> memory, LinkFlags flags, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.SendAsync(Socket, memory, flags, token);
        }

        protected override sealed ValueTask<Result<SocketOperationValue>> ExecuteSendAsync(IList<ArraySegment<byte>> buffer, LinkFlags flags, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.SendAsync(Socket, buffer, flags, token);
        }

        #endregion Send

        #region Receive

        protected override Result<SocketOperationValue> ExecuteReceive(Span<byte> span, LinkFlags flags)
        {
            var length = Socket.Receive(span, (SocketFlags)flags, out var errorCode);

            if (TryGetSocketError(errorCode, out var ex))
                return Result.FromException<SocketOperationValue>(ex);
            return CreateOperationValue(length);
        }

        protected override Result<SocketOperationValue> ExecuteReceive(IList<ArraySegment<byte>> buffer, LinkFlags flags)
        {
            var length = Socket.Receive(buffer, (SocketFlags)flags, out var errorCode);

            if (TryGetSocketError(errorCode, out var ex))
                return Result.FromException<SocketOperationValue>(ex);
            return CreateOperationValue(length);
        }

        protected override sealed ValueTask<Result<SocketOperationValue>> ExecuteReceiveAsync(Memory<byte> memory, LinkFlags flags, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.ReceiveAsync(Socket, memory, token);
        }

        protected override sealed ValueTask<Result<SocketOperationValue>> ExecuteReceiveAsync(IList<ArraySegment<byte>> buffer, LinkFlags flags, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.ReceiveAsync(Socket, buffer, token);
        }

        #endregion Receive

        protected bool TryGetSocketError(SocketError error, out SocketException exception)
        {
            if (error == SocketError.Success)
            {
                exception = null!;
                return false;
            }
            exception = CreateSocketException(error);
            return true;
        }

        protected SocketException CreateSocketException(SocketError error)
        {
            return new SocketException((int)error);
        }

        private Result<SocketOperationValue> CreateOperationValue(int length, IPPacketInformation receiveMessageFromPacketInfo = default)
        {
            return Result.Success(new SocketOperationValue(length, RemoteEndPoint, receiveMessageFromPacketInfo));
        }

        public override ILinkInfo SetOption(LinkOptionLevel optionLevel, LinkOptionName optionName, ValueCache optionValue)
        {
            if (optionValue.TryGetValue(out byte[] byteArray))
            {
                Socket.SetSocketOption((SocketOptionLevel)optionLevel, (SocketOptionName)optionName, byteArray);
                return this;
            }
            else if (optionValue.TryGetValue(out int intValue))
            {
                Socket.SetSocketOption((SocketOptionLevel)optionLevel, (SocketOptionName)optionName, intValue);
                return this;
            }
            else if (optionValue.TryGetValue(out bool boolValue))
            {
                Socket.SetSocketOption((SocketOptionLevel)optionLevel, (SocketOptionName)optionName, boolValue);
            }
            else if (optionValue.TryGetValue(out object objectValue))
            {
                Socket.SetSocketOption((SocketOptionLevel)optionLevel, (SocketOptionName)optionName, objectValue);
            }

            optionValue.Release();
            return this;
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
    }
}