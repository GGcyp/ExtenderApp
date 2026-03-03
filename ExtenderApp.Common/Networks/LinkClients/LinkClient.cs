using System.Collections;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.Networks;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 链路客户端的抽象基类，封装对 <see cref="ILinker"/> 的委托并提供基础能力。
    /// </summary>
    /// <typeparam name="TLinker">具体的链接器类型。</typeparam>
    public class LinkClient<TLinker> : OptionsObject, ILinkClient, ILinkClientPipeline
        where TLinker : ILinker
    {
        /// <summary>
        /// 底层链接器实例。
        /// </summary>
        protected TLinker Linker;

        private readonly LinkClientPipeline _pipeline;

        #region ILinker 直通属性

        /// <inheritdoc/>
        public bool Connected
            => GetOptionValue(LinkOptions.ConnectedIdentifier);

        /// <inheritdoc/>
        public EndPoint? LocalEndPoint
            => GetOptionValue(LinkOptions.LocalEndPointIdentifier);

        /// <inheritdoc/>
        public EndPoint? RemoteEndPoint
            => GetOptionValue(LinkOptions.RemoteEndPointIdentifier);

        /// <inheritdoc/>
        public CapacityLimiter CapacityLimiter
            => GetOptionValue(LinkOptions.CapacityLimiterIdentifier);

        /// <inheritdoc/>
        public ValueCounter SendCounter
            => GetOptionValue(LinkOptions.SendCounterIdentifier);

        /// <inheritdoc/>
        public ValueCounter ReceiveCounter
            => GetOptionValue(LinkOptions.ReceiveCounterIdentifier);

        /// <inheritdoc/>
        public ProtocolType ProtocolType
            => GetOptionValue(LinkOptions.ProtocolTypeIdentifier);

        /// <inheritdoc/>
        public SocketType SocketType
            => GetOptionValue(LinkOptions.SocketTypeIdentifier);

        /// <inheritdoc/>
        public AddressFamily AddressFamily
            => GetOptionValue(LinkOptions.AddressFamilyIdentifier);

        /// <inheritdoc/>
        public int ReceiveBufferSize
        {
            get => GetOptionValue(LinkOptions.ReceiveBufferSizeIdentifier);
            set => SetOptionValue(LinkOptions.ReceiveBufferSizeIdentifier, value);
        }

        /// <inheritdoc/>
        public int SendBufferSize
        {
            get => GetOptionValue(LinkOptions.SendBufferSizeIdentifier);
            set => SetOptionValue(LinkOptions.SendBufferSizeIdentifier, value);
        }

        /// <inheritdoc/>
        public int ReceiveTimeout
        {
            get => GetOptionValue(LinkOptions.ReceiveTimeoutIdentifier);
            set => SetOptionValue(LinkOptions.ReceiveTimeoutIdentifier, value);
        }

        /// <inheritdoc/>
        public int SendTimeout
        {
            get => GetOptionValue(LinkOptions.SendTimeoutIdentifier);
            set => SetOptionValue(LinkOptions.SendTimeoutIdentifier, value);
        }

        #endregion ILinker 直通属性

        private CancellationTokenSource? receiveCts;

        private Task? receiveTask;

        /// <summary>
        /// 初始化 <see cref="LinkClient{TLinker}"/> 的新实例。
        /// </summary>
        /// <param name="linker">要使用的链接器实例。</param>
        public LinkClient(TLinker linker) : base(linker)
        {
            Linker = linker ?? throw new ArgumentNullException(nameof(linker));
            _pipeline = new(this);
        }

        /// <inheritdoc/>
        public virtual async ValueTask<Result<LinkOperationValue>> SendAsync<T>(T value, CancellationToken token = default)
        {
            if (!Connected)
                throw new InvalidOperationException("当前链接未建立，无法发送数据。");

            var outCache = ValueCache.FromValue(value);
            await _pipeline.OutboundHandleAsync(outCache, token);

            LinkOperationValue result = new(0, RemoteEndPoint, default);
            var sequence = SequenceBuffer<byte>.GetBuffer();
            while (true)
            {
                while (outCache.TryGetValue(out AbstractBuffer<byte> operationValue))
                {
                    if (sequence.Committed + operationValue.Committed > SendBufferSize)
                    {
                        break;
                    }

                    outCache.TryTakeValue(out operationValue);
                    sequence.Append(operationValue);
                    operationValue.TryRelease();
                    continue;
                }

                try
                {
                    if (sequence.Committed > 0)
                    {
                        var sendResult = await Linker.SendAsync(sequence, token);
                        result = new(sendResult, sendResult.Value.BytesTransferred + result.BytesTransferred);
                        sequence.Clear();
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    return Result.FromException<LinkOperationValue>(ex);
                }
            }

            outCache.Release();
            sequence.TryRelease();
            return Result.Success(result);
        }

        #region Connect/Disconnect

        /// <inheritdoc/>
        public virtual void Connect(EndPoint remoteEndPoint)
        {
            Connect(remoteEndPoint, null!);
        }

        /// <inheritdoc/>
        public void Connect(EndPoint remoteEndPoint, EndPoint localAddress)
        {
            ConnectAsync(remoteEndPoint, localAddress).Await();
        }

        /// <inheritdoc/>
        public virtual ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default)
        {
            return ConnectAsync(remoteEndPoint, null!, token);
        }

        /// <inheritdoc/>
        public virtual async ValueTask ConnectAsync(EndPoint remoteEndPoint, EndPoint localAddress, CancellationToken token = default)
        {
            ThrowIfDisposed();
            await _pipeline.ConnectAsync(remoteEndPoint, localAddress, token);
            await Linker.ConnectAsync(remoteEndPoint, localAddress, token);
        }

        /// <inheritdoc/>
        public virtual void Disconnect()
        {
            DisconnectAsync().Await();
        }

        /// <inheritdoc/>
        public virtual async ValueTask DisconnectAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            await _pipeline.DisconnectAsync(token);
            await Linker.DisconnectAsync(token);
        }

        #endregion Connect/Disconnect

        #region Pipeline Operations

        public ILinkClientPipeline AddLast<T>(string name, T handler) where T : ILinkClientHandler
            => _pipeline.AddLast(name, handler);

        public ILinkClientPipeline AddFirst<T>(string name, T handler) where T : ILinkClientHandler
            => _pipeline.AddFirst(name, handler);

        public ILinkClientPipeline AddBefore<T>(string baseName, string name, T handler) where T : ILinkClientHandler
            => _pipeline.AddBefore(baseName, name, handler);

        public ILinkClientPipeline AddAfter<T>(string baseName, string name, T handler) where T : ILinkClientHandler
            => _pipeline.AddAfter(baseName, name, handler);

        public ILinkClientPipeline Remove<T>(T handler) where T : ILinkClientHandler
            => _pipeline.Remove(handler);

        public ILinkClientHandler Remove(string name)
            => _pipeline.Remove(name);

        public ILinkClientPipeline Replace<T>(string oldName, string newName, T newHandler) where T : ILinkClientHandler
            => _pipeline.Replace(oldName, newName, newHandler);

        public ILinkClientPipeline Replace<T>(ILinkClientHandler oldHandler, string newName, T newHandler) where T : ILinkClientHandler
            => _pipeline.Replace(oldHandler, newName, newHandler);

        public IEnumerator<ILinkClientHandler> GetEnumerator() => _pipeline.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _pipeline.GetEnumerator();

        #endregion Pipeline Operations

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            Linker.DisposeSafe();
        }

        /// <inheritdoc/>
        protected override ValueTask DisposeAsyncManagedResources()
        {
            return Linker.DisposeSafeAsync();
        }
    }
}