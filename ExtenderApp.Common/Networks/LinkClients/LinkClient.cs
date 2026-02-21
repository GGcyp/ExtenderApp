using System.Collections;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 链路客户端的抽象基类，封装对 <see cref="ILinker"/> 的委托并提供基础能力。
    /// </summary>
    /// <typeparam name="TLinker">具体的链接器类型。</typeparam>
    public abstract class LinkClient<TLinker> : OptionsObject, ILinkClient
        where TLinker : ILinker
    {
        /// <summary>
        /// 底层链接器实例。
        /// </summary>
        protected TLinker Linker;

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
            set => Linker.ReceiveBufferSize = value;
        }

        /// <inheritdoc/>
        public int SendBufferSize
        {
            get => GetOptionValue(LinkOptions.SendBufferSizeIdentifier);
            set => Linker.SendBufferSize = value;
        }

        /// <inheritdoc/>
        public int ReceiveTimeout
        {
            get => GetOptionValue(LinkOptions.ReceiveTimeoutIdentifier);
            set => Linker.ReceiveTimeout = value;
        }

        /// <inheritdoc/>
        public int SendTimeout
        {
            get => GetOptionValue(LinkOptions.SendTimeoutIdentifier);
            set => Linker.SendTimeout = value;
        }

        #endregion ILinker 直通属性

        private CancellationTokenSource? receiveCts;

        private Task? receiveTask;

        /// <summary>
        /// 初始化 <see cref="LinkClient{TLinker}"/> 的新实例。
        /// </summary>
        /// <param name="linker">要使用的链接器实例。</param>
        protected LinkClient(TLinker linker) : base(linker)
        {
            Linker = linker ?? throw new ArgumentNullException(nameof(linker));
        }

        /// <inheritdoc/>
        protected override ValueTask DisposeAsyncManagedResources()
        {
            return Linker.DisposeSafeAsync();
        }

        /// <inheritdoc/>
        public Result<LinkOperationValue> SendAsync<T>(T value, CancellationToken token = default)
        {
            return default;
        }

        /// <inheritdoc/>
        public virtual void Connect(EndPoint remoteEndPoint)
        {
        }

        /// <inheritdoc/>
        public virtual async ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default)
        {
            ThrowIfDisposed();
        }

        /// <inheritdoc/>
        public virtual void Disconnect()
        {
            ThrowIfDisposed();
        }

        /// <inheritdoc/>
        public virtual ValueTask DisconnectAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            return default;
        }

        #region Options

        public ILinkClientPipeline AddLast(string name, ILinkClientHandler handler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline AddFirst(string name, ILinkClientHandler handler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline AddBefore(string baseName, string name, ILinkClientHandler handler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline AddAfter(string baseName, string name, ILinkClientHandler handler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline Remove(ILinkClientHandler handler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientHandler Remove(string name)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline Replace(string oldName, string newName, ILinkClientHandler newHandler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline Replace(ILinkClientHandler oldHandler, string newName, ILinkClientHandler newHandler)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ILinkClientHandler> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Options
    }
}