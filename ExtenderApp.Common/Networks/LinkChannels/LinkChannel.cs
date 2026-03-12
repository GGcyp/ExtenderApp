using System.Collections;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.Networks;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkChannels
{
    /// <summary>
    /// 链路通道基类，封装对 <see cref="ILinker"/> 的委托并提供管道处理能力。
    /// </summary>
    public class LinkChannel : OptionsObject, ILinkChannel, ILinkChannelPipeline
    {
        /// <summary>
        /// 底层链接器实例。
        /// </summary>
        public ILinker Linker { get; }

        private readonly LinkChannelPipeline _pipeline;

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

        private readonly object _lock = new object();

        private Task? receiveTask;

        private CancellationTokenSource? receiveCts;

        /// <summary>
        /// 获取当前是否已启动接收任务。
        /// </summary>
        public bool HasReceiveTask => receiveTask != null;

        /// <summary>
        /// 使用指定的链接器初始化 <see cref="LinkChannel"/> 的新实例。
        /// </summary>
        /// <param name="linker">要使用的链接器实例。</param>
        public LinkChannel(ILinker linker) : base(linker)
        {
            _pipeline = new(this);
            Linker = linker;
        }

        /// <inheritdoc/>
        public async ValueTask<Result<LinkOperationValue>> SendAsync<T>(T value, CancellationToken token = default)
        {
            if (!Connected)
                throw new InvalidOperationException("当前链接未建立，无法发送数据。");
            ArgumentNullException.ThrowIfNull(Linker, nameof(Linker));

            var outCache = ValueCache.FromValue(value);
            try
            {
                var result = await _pipeline.OutboundHandleAsync(outCache, token).ConfigureAwait(false);

                if (!result)
                    return Result.FromException<LinkOperationValue>(result.Exception!);
                else if (outCache.TryTakeValue(out LinkOperationValue linkOperationValue))
                    return Result.Success(linkOperationValue);
                else
                    return Result.Success(LinkOperationValue.Empty, "发送操作未能产生有效的 LinkOperationValue 结果。");
            }
            catch (Exception ex)
            {
                return Result.FromException<LinkOperationValue>(ex);
            }
            finally
            {
                outCache.Release();
            }
        }

        /// <summary>
        /// 开始接收数据并通过管道处理。该方法会启动一个后台任务持续接收数据，直到链接关闭或对象被释放。
        /// </summary>
        public void StartReceive()
        {
            if (HasReceiveTask)
                return;

            lock (_lock)
            {
                if (HasReceiveTask)
                    return;

                receiveCts = new CancellationTokenSource();
                receiveTask = ReceiveAsync();
            }
        }

        /// <summary>
        /// 接收数据的核心逻辑方法，在后台任务中运行。该方法会持续调用链接器的接收方法，并将接收到的数据通过管道进行处理，直到链接关闭或对象被释放。
        /// </summary>
        /// <returns>一个表示接收操作的任务。</returns>
        private async Task ReceiveAsync()
        {
            var token = receiveCts!.Token;
            var cache = ValueCache.GetCache();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var result = await _pipeline.InboundHandleAsync(cache, token).ConfigureAwait(false);
                    result.ThrowExceptionWithOriginalStackTraceIfError();

                    cache.Clear();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                lock (_lock)
                {
                    receiveTask = null;
                    receiveCts?.Dispose();
                    receiveCts = null;
                }

                cache.Release();
            }
        }

        #region Connect/Disconnect

        /// <inheritdoc/>
        public virtual Result Connect(EndPoint remoteEndPoint)
        {
            return Connect(remoteEndPoint, null!);
        }

        /// <inheritdoc/>
        public Result Connect(EndPoint remoteEndPoint, EndPoint localAddress)
        {
            return ConnectAsync(remoteEndPoint, localAddress).Await(false);
        }

        /// <inheritdoc/>
        public virtual ValueTask<Result> ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default)
        {
            return ConnectAsync(remoteEndPoint, null!, token);
        }

        /// <inheritdoc/>
        public virtual ValueTask<Result> ConnectAsync(EndPoint remoteEndPoint, EndPoint localAddress, CancellationToken token = default)
        {
            ThrowIfDisposed();
            return _pipeline.ConnectAsync(remoteEndPoint, localAddress, token);
        }

        /// <inheritdoc/>
        public virtual Result Disconnect()
        {
            return DisconnectAsync().Await();
        }

        /// <inheritdoc/>
        public virtual async ValueTask<Result> DisconnectAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            receiveCts?.Cancel();
            if (HasReceiveTask)
                await receiveTask!.ConfigureAwait(false);

            return await _pipeline.DisconnectAsync(token).ConfigureAwait(false);
        }

        #endregion Connect/Disconnect

        #region Pipeline Operations

        /// <summary>
        /// 在管道末尾添加处理器。
        /// </summary>
        /// <typeparam name="T">处理器类型。</typeparam>
        /// <param name="name">处理器名称。</param>
        /// <param name="handler">处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        public ILinkChannelPipeline AddLast<T>(string name, T handler) where T : ILinkChannelHandler
            => _pipeline.AddLast(name, handler);

        /// <summary>
        /// 在管道开头添加处理器。
        /// </summary>
        /// <typeparam name="T">处理器类型。</typeparam>
        /// <param name="name">处理器名称。</param>
        /// <param name="handler">处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        public ILinkChannelPipeline AddFirst<T>(string name, T handler) where T : ILinkChannelHandler
            => _pipeline.AddFirst(name, handler);

        /// <summary>
        /// 在指定处理器之前插入新处理器。
        /// </summary>
        /// <typeparam name="T">处理器类型。</typeparam>
        /// <param name="baseName">基准处理器名称。</param>
        /// <param name="name">新处理器名称。</param>
        /// <param name="handler">新处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        public ILinkChannelPipeline AddBefore<T>(string baseName, string name, T handler) where T : ILinkChannelHandler
            => _pipeline.AddBefore(baseName, name, handler);

        /// <summary>
        /// 在指定处理器之后插入新处理器。
        /// </summary>
        /// <typeparam name="T">处理器类型。</typeparam>
        /// <param name="baseName">基准处理器名称。</param>
        /// <param name="name">新处理器名称。</param>
        /// <param name="handler">新处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        public ILinkChannelPipeline AddAfter<T>(string baseName, string name, T handler) where T : ILinkChannelHandler
            => _pipeline.AddAfter(baseName, name, handler);

        /// <summary>
        /// 从管道中移除指定处理器实例。
        /// </summary>
        /// <typeparam name="T">处理器类型。</typeparam>
        /// <param name="handler">要移除的处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        public ILinkChannelPipeline Remove<T>(T handler) where T : ILinkChannelHandler
            => _pipeline.Remove(handler);

        /// <summary>
        /// 按名称从管道中移除处理器。
        /// </summary>
        /// <param name="name">处理器名称。</param>
        /// <returns>被移除的处理器。</returns>
        public ILinkChannelHandler Remove(string name)
            => _pipeline.Remove(name);

        /// <summary>
        /// 按名称替换管道中的处理器。
        /// </summary>
        /// <typeparam name="T">新处理器类型。</typeparam>
        /// <param name="oldName">旧处理器名称。</param>
        /// <param name="newName">新处理器名称。</param>
        /// <param name="newHandler">新处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        public ILinkChannelPipeline Replace<T>(string oldName, string newName, T newHandler) where T : ILinkChannelHandler
            => _pipeline.Replace(oldName, newName, newHandler);

        /// <summary>
        /// 按实例替换管道中的处理器。
        /// </summary>
        /// <typeparam name="T">新处理器类型。</typeparam>
        /// <param name="oldHandler">旧处理器实例。</param>
        /// <param name="newName">新处理器名称。</param>
        /// <param name="newHandler">新处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        public ILinkChannelPipeline Replace<T>(ILinkChannelHandler oldHandler, string newName, T newHandler) where T : ILinkChannelHandler
            => _pipeline.Replace(oldHandler, newName, newHandler);

        /// <summary>
        /// 返回用于遍历管道处理器的枚举器。
        /// </summary>
        /// <returns>处理器枚举器。</returns>
        public IEnumerator<ILinkChannelHandler> GetEnumerator() => _pipeline.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _pipeline.GetEnumerator();

        #endregion Pipeline Operations

        /// <summary>
        /// 释放托管资源。
        /// </summary>
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