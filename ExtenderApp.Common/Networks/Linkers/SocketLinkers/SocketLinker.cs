using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.Networks;
using ExtenderApp.Abstract.Options;
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

        /// <summary>
        /// 初始化 <see cref="SocketLinker"/> 的新实例。
        /// </summary>
        /// <param name="socket">要使用的 <see cref="Socket"/> 实例。</param>
        public SocketLinker(Socket socket)
        {
            Socket = socket;
        }

        /// <inheritdoc/>
        protected override sealed void ExecuteBind(EndPoint endPoint)
        {
            Socket.Bind(endPoint);
        }

        /// <inheritdoc/>
        protected override sealed ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token)
        {
            return Socket.ConnectAsync(remoteEndPoint, token);
        }

        /// <inheritdoc/>
        protected override sealed ValueTask ExecuteDisconnectAsync(CancellationToken token)
        {
            return Socket.DisconnectAsync(reuseSocket: false, token);
        }

        protected override void OnRegisterOption(OptionIdentifier identifier, OptionValue optionValue)
        {
            if (LinkOptions.ReceiveBufferSizeIdentifier.TryBindChangedHandler(optionValue, static (o, item) => ((SocketLinker)o!).Socket.ReceiveBufferSize = item.Item2))
                return;
            if (LinkOptions.SendBufferSizeIdentifier.TryBindChangedHandler(optionValue, static (o, item) => ((SocketLinker)o!).Socket.SendBufferSize = item.Item2))
                return;
            if (LinkOptions.ReceiveTimeoutIdentifier.TryBindChangedHandler(optionValue, static (o, item) => ((SocketLinker)o!).Socket.ReceiveTimeout = item.Item2))
                return;
            if (LinkOptions.SendTimeoutIdentifier.TryBindChangedHandler(optionValue, static (o, item) => ((SocketLinker)o!).Socket.SendTimeout = item.Item2))
                return;
        }

        protected override void OnOptionValueChanged(OptionIdentifier identifier, OptionValue optionValue)
        {
            switch (identifier)
            {
                case SocketOptionIdentifier<int> socketOptionIdentifierInt:
                    Socket.SetSocketOption(socketOptionIdentifierInt.SocketLevel, socketOptionIdentifierInt.SocketName, (socketOptionIdentifierInt.ConvertOptionValue(optionValue)).Value);
                    return;

                case SocketOptionIdentifier<bool> socketOptionIdentifierBool:
                    Socket.SetSocketOption(socketOptionIdentifierBool.SocketLevel, socketOptionIdentifierBool.SocketName, (socketOptionIdentifierBool.ConvertOptionValue(optionValue)).Value);
                    return;

                case SocketOptionIdentifier<object> socketOptionIdentifierObject:
                    Socket.SetSocketOption(socketOptionIdentifierObject.SocketLevel, socketOptionIdentifierObject.SocketName, (socketOptionIdentifierObject.ConvertOptionValue(optionValue)).Value);
                    return;

                case SocketOptionIdentifier<byte[]> socketOptionIdentifierByteArray:
                    Socket.SetSocketOption(socketOptionIdentifierByteArray.SocketLevel, socketOptionIdentifierByteArray.SocketName, (socketOptionIdentifierByteArray.ConvertOptionValue(optionValue)).Value);
                    return;
            }
        }

        #region Send

        /// <inheritdoc/>
        protected override sealed Result<LinkOperationValue> ExecuteSend(ReadOnlySpan<byte> span, LinkFlags flags)
        {
            var length = Socket.Send(span, (SocketFlags)flags, out var errorCode);

            if (TryGetSocketError(errorCode, out var ex))
                return Result.FromException<LinkOperationValue>(ex);
            return CreateOperationValue(length);
        }

        /// <inheritdoc/>
        protected override sealed Result<LinkOperationValue> ExecuteSend(IList<ArraySegment<byte>> buffer, LinkFlags flags)
        {
            var length = Socket.Send(buffer, (SocketFlags)flags, out var errorCode);

            if (TryGetSocketError(errorCode, out var ex))
                return Result.FromException<LinkOperationValue>(ex);
            return CreateOperationValue(length);
        }

        /// <inheritdoc/>
        protected override sealed ValueTask<Result<LinkOperationValue>> ExecuteSendAsync(Memory<byte> memory, LinkFlags flags, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.SendAsync(Socket, memory, flags, token);
        }

        /// <inheritdoc/>
        protected override sealed ValueTask<Result<LinkOperationValue>> ExecuteSendAsync(IList<ArraySegment<byte>> buffer, LinkFlags flags, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.SendAsync(Socket, buffer, flags, token);
        }

        #endregion Send

        #region Receive

        /// <inheritdoc/>
        protected override sealed Result<LinkOperationValue> ExecuteReceive(Span<byte> span, LinkFlags flags)
        {
            var length = Socket.Receive(span, (SocketFlags)flags, out var errorCode);

            if (TryGetSocketError(errorCode, out var ex))
                return Result.FromException<LinkOperationValue>(ex);
            return CreateOperationValue(length);
        }

        /// <inheritdoc/>
        protected override sealed Result<LinkOperationValue> ExecuteReceive(IList<ArraySegment<byte>> buffer, LinkFlags flags)
        {
            var length = Socket.Receive(buffer, (SocketFlags)flags, out var errorCode);

            if (TryGetSocketError(errorCode, out var ex))
                return Result.FromException<LinkOperationValue>(ex);
            return CreateOperationValue(length);
        }

        /// <inheritdoc/>
        protected override sealed ValueTask<Result<LinkOperationValue>> ExecuteReceiveAsync(Memory<byte> memory, LinkFlags flags, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.ReceiveAsync(Socket, memory, token);
        }

        /// <inheritdoc/>
        protected override sealed ValueTask<Result<LinkOperationValue>> ExecuteReceiveAsync(IList<ArraySegment<byte>> buffer, LinkFlags flags, CancellationToken token)
        {
            var args = AwaitableSocketEventArgs.Get();
            return args.ReceiveAsync(Socket, buffer, token);
        }

        #endregion Receive

        /// <summary>
        /// 尝试将套接字错误转换为异常。
        /// </summary>
        /// <param name="error">套接字错误码。</param>
        /// <param name="exception">转换后的异常。</param>
        /// <returns>是否发生错误。</returns>
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

        /// <summary>
        /// 创建套接字异常。
        /// </summary>
        /// <param name="error">套接字错误码。</param>
        /// <returns>对应的异常实例。</returns>
        protected SocketException CreateSocketException(SocketError error)
        {
            return new SocketException((int)error);
        }

        /// <summary>
        /// 创建操作结果。
        /// </summary>
        /// <param name="length">传输长度。</param>
        /// <param name="receiveMessageFromPacketInfo">接收数据包信息。</param>
        /// <returns>操作结果。</returns>
        private Result<LinkOperationValue> CreateOperationValue(int length, IPPacketInformation receiveMessageFromPacketInfo = default)
        {
            return Result.Success(new LinkOperationValue(length, RemoteEndPoint, receiveMessageFromPacketInfo));
        }

        /// <inheritdoc/>
        public override ILinker Clone()
        {
            var socket = new Socket(AddressFamily, SocketType, ProtocolType);
            return Clone(socket);
        }

        /// <summary>
        /// 使用指定 <see cref="Socket"/> 克隆链接器。
        /// </summary>
        /// <param name="socket">要用于克隆的套接字。</param>
        /// <returns>克隆后的链接器实例。</returns>
        protected abstract ILinker Clone(Socket socket);

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncManagedResources()
        {
            await base.DisposeAsyncManagedResources();
            Socket.Dispose();
        }
    }
}