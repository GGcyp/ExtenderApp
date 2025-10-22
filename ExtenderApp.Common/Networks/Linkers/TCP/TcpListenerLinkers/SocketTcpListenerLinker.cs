using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 基于 <see cref="Socket"/> 的 TCP 监听器实现。
    /// 通过 <see cref="ITcpListenerLinker.OnAccept"/> 事件将新连接以 <see cref="ITcpLinker"/> 的形式发布。
    /// 支持配置并行 Accept 数量（<see cref="AcceptConcurrency"/>）与运行时暂停/恢复（<see cref="Pause"/> / <see cref="Resume"/>）。
    /// </summary>
    internal class SocketTcpListenerLinker : TcpListenerLinker
    {
        private static readonly ObjectPool<AwaitableSocketEventArgs> _pool
            = ObjectPool.CreateDefaultPool<AwaitableSocketEventArgs>();

        private readonly Socket _listenerSocket;

        private bool _isBinded;
        private volatile int _started; // 0/1：防止重复启动
        private CancellationTokenSource? _cts;
        private Task[]? _acceptTasks;

        /// <summary>
        /// 可配置的并行 Accept 数量（“监听核心数”）。
        /// 默认值为 1。必须在调用 <see cref="Listen(int)"/> 之前设置，启动后不可更改。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">设置的值小于 1。</exception>
        /// <exception cref="InvalidOperationException">监听已开始仍尝试修改。</exception>
        private int _acceptConcurrency = 1;
        public int AcceptConcurrency
        {
            get => Volatile.Read(ref _acceptConcurrency);
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(value));
                if (Volatile.Read(ref _started) == 1) throw new InvalidOperationException("监听已开始，无法修改 AcceptConcurrency。");
                _acceptConcurrency = value;
            }
        }

        // 暂停/恢复闸门（默认开启）
        private readonly AsyncManualResetEvent _pauseGate = new(initialState: true);

        /// <summary>
        /// 当前是否处于“暂停接受新连接”状态。
        /// 暂停状态下不会发起新的 Accept；已挂起的 Accept 最多还会完成 <see cref="AcceptConcurrency"/> 个。
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// 获取监听器的本地终结点（本地地址与端口）。
        /// 在 <see cref="Bind(EndPoint)"/> 成功调用并开始监听后有效。
        /// </summary>
        public override EndPoint? ListenerPoint => _listenerSocket.LocalEndPoint;

        /// <summary>
        /// 使用指定监听 <see cref="Socket"/> 与链接器工厂构造监听器。
        /// </summary>
        /// <param name="socket">处于可绑定/监听状态的 <see cref="Socket"/>。</param>
        /// <param name="linkerFactory">用于将已接入的 <see cref="Socket"/> 包装为 <see cref="ITcpLinker"/> 的工厂。</param>
        /// <exception cref="ArgumentNullException"><paramref name="socket"/> 或 <paramref name="linkerFactory"/> 为 null。</exception>
        public SocketTcpListenerLinker(Socket socket, ILinkerFactory<ITcpLinker> linkerFactory) : base(linkerFactory)
        {
            _listenerSocket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        /// <summary>
        /// 使用指定并行 Accept 数量构造监听器的便捷重载。
        /// </summary>
        /// <param name="socket">监听用 <see cref="Socket"/>。</param>
        /// <param name="linkerFactory">链接器工厂。</param>
        /// <param name="acceptConcurrency">并行 Accept 数量（≥1）。</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="acceptConcurrency"/> 小于 1。</exception>
        public SocketTcpListenerLinker(Socket socket, ILinkerFactory<ITcpLinker> linkerFactory, int acceptConcurrency) : this(socket, linkerFactory)
        {
            AcceptConcurrency = acceptConcurrency;
        }

        /// <summary>
        /// 绑定本地终结点。
        /// </summary>
        /// <param name="endPoint">要绑定的本地地址与端口。</param>
        /// <exception cref="ArgumentNullException"><paramref name="endPoint"/> 为 null。</exception>
        /// <exception cref="SocketException">底层套接字绑定失败。</exception>
        public override void Bind(EndPoint endPoint)
        {
            if (endPoint is null)
                throw new ArgumentNullException(nameof(endPoint));

            _listenerSocket.Bind(endPoint);
            _isBinded = true;
        }

        /// <summary>
        /// 开始监听并启动并行 Accept 循环。
        /// </summary>
        /// <param name="backlog">监听队列长度（传递给 <see cref="Socket.Listen(int)"/>）。</param>
        /// <remarks>
        /// - 幂等：多次调用仅首次生效。<br/>
        /// - 接入通知：每当有新连接接入，将通过 <see cref="ITcpListenerLinker.OnAccept"/> 发布一个 <see cref="ITcpLinker"/> 实例。<br/>
        /// - 并行度：Accept 并行数量由 <see cref="AcceptConcurrency"/> 决定，需在启动前设置。<br/>
        /// - 暂停/恢复：可通过 <see cref="Pause"/> 与 <see cref="Resume"/> 控制是否发起新的 Accept。
        /// </remarks>
        /// <exception cref="InvalidOperationException">未调用 <see cref="Bind(EndPoint)"/> 即开始监听，或监听已开始。</exception>
        /// <exception cref="SocketException">底层开始监听失败。</exception>
        public override void Listen(int backlog)
        {
            if (!_isBinded)
                throw new InvalidOperationException("Socket未绑定，无法监听。");

            // 仅支持 TCP 监听，UDP 不支持 Listen/Accept
            if (_listenerSocket.SocketType != SocketType.Stream || _listenerSocket.ProtocolType != ProtocolType.Tcp)
                throw new NotSupportedException("SocketListenerLinker 仅支持 TCP（Stream）套接字，UDP 不支持 Listen/Accept。");

            if (Interlocked.Exchange(ref _started, 1) == 1)
                return;

            _listenerSocket.Listen(backlog <= 0 ? 10 : backlog);

            var tokenSource = new CancellationTokenSource();
            _cts = tokenSource;
            var token = tokenSource.Token;

            int acceptConcurrency = AcceptConcurrency;
            _acceptTasks = new Task[acceptConcurrency];
            for (int i = 0; i < acceptConcurrency; i++)
            {
                _acceptTasks[i] = Task.Run(() => AcceptWorkerAsync(token), token);
            }
        }

        /// <summary>
        /// 暂停接受新连接（不关闭监听 <see cref="Socket"/>）。
        /// 已挂起的 Accept 最多仍会完成 <see cref="AcceptConcurrency"/> 个，之后不再发起新的 Accept。
        /// </summary>
        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            _pauseGate.Reset();
        }

        /// <summary>
        /// 恢复接受新连接。
        /// 触发后新的 Accept 将继续挂起并接入。
        /// </summary>
        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            _pauseGate.Set();
        }

        private async Task AcceptWorkerAsync(CancellationToken token)
        {
            // 持续挂起 Accept，接入后通过事件通知
            var args = _pool.Get();
            while (!token.IsCancellationRequested)
            {
                // 等待恢复后再开始下一轮 Accept
                await _pauseGate.WaitAsync().ConfigureAwait(false);
                if (token.IsCancellationRequested) break;

                Socket? accepted = null;
                try
                {
                    accepted = await args.AcceptAsync(_listenerSocket, token).ConfigureAwait(false);
                }
                catch (SocketException)
                {
                    // 监听Socket可能短暂异常，继续尝试
                    continue;
                }

                if (accepted is null)
                    continue;

                // 将已接入的 Socket 包装为 _linker，并通过事件发布
                var linker = linkerFactory.CreateLinker(accepted);

                try
                {
                    // 事件只作通知钩子，重活请移交到调用方
                    RaiseOnAccept(linker);
                }
                catch
                {
                    // 隔离事件处理器异常，避免中断接入循环
                }
            }
            _pool.Release(args);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            // 确保不因暂停而阻塞退出
            try { _pauseGate.Set(); } catch { /* ignore */ }

            try { _cts?.Cancel(); } catch { /* ignore */ }
            try { _listenerSocket.Dispose(); } catch { /* ignore */ }

            if (_acceptTasks is { Length: > 0 })
            {
                try { Task.WhenAll(_acceptTasks).Wait(TimeSpan.FromSeconds(1)); } catch { /* ignore */ }
            }

            _cts?.Dispose();
            _cts = null;
        }

        /// <summary>
        /// 轻量级的异步“手动复位事件”（Async Manual Reset Event）。
        /// 用于实现“暂停/恢复”闸门：
        /// - Set()：打开闸门，允许等待者继续；
        /// - Reset()：关闭闸门，使后续等待者阻塞；
        /// - WaitAsync()：等待闸门被打开。
        /// </summary>
        /// <remarks>
        /// 设计要点：
        /// - 基于 <see cref="TaskCompletionSource{TResult}"/> 的无锁/低锁实现；
        /// - Set 为幂等操作，重复调用不会抛异常；
        /// - Reset 仅在当前任务已完成时重建新的 <see cref="TaskCompletionSource{TResult}"/>，避免不必要的分配；
        /// - 适用于“许多等待者同时被释放”的广播式唤醒场景。
        /// </remarks>
        private sealed class AsyncManualResetEvent
        {
            /// <summary>
            /// 当前“闸门”的任务源：未完成表示闸门关闭；完成表示闸门打开。
            /// </summary>
            private volatile TaskCompletionSource<bool> _tcs;

            /// <summary>
            /// 初始化异步手动复位事件。
            /// </summary>
            /// <param name="initialState">
            /// 初始状态：true 表示闸门打开（无需等待即可通过）；false 表示闸门关闭（等待 Set()）。
            /// </param>
            public AsyncManualResetEvent(bool initialState = false)
            {
                _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
                if (initialState)
                    _tcs.TrySetResult(true);
            }

            /// <summary>
            /// 异步等待闸门被打开。
            /// </summary>
            public Task WaitAsync() => _tcs.Task;

            /// <summary>
            /// 打开闸门：释放所有当前等待者，并使后续调用 <see cref="WaitAsync"/> 的等待者直接通过。
            /// </summary>
            public void Set()
            {
                _tcs.TrySetResult(true);
            }

            /// <summary>
            /// 关闭闸门：后续调用 <see cref="WaitAsync"/> 的等待者将被阻塞，直到下次 <see cref="Set"/>。
            /// </summary>
            public void Reset()
            {
                while (true)
                {
                    var tcs = _tcs;
                    if (!tcs.Task.IsCompleted) return;
                    var newTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    if (Interlocked.CompareExchange(ref _tcs, newTcs, tcs) == tcs)
                        return;
                }
            }
        }
    }
}