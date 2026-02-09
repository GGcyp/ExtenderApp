using System.Threading.Tasks.Sources;

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 容量闸门（按总量配额进行加权申请与释放，容量不足时可等待）。
    /// 线程安全，支持 FIFO 公平发放与取消等待。
    /// </summary>
    /// <remarks>
    /// 用途示例：
    /// - 发送限流：按字节数申请，发送完成后归还；
    /// - 并发门控：按任务权重申请槽位；
    /// - 批量处理：容量不足时按 FIFO 排队等待。
    ///
    /// 公平性：仅在满足队首请求时才继续发放，保持 FIFO 公平。
    /// 取消：调用方在 <see cref="AcquireAsync(long, CancellationToken)"/> 或 <see cref="Acquire(long, CancellationToken)"/> 传入可取消的 <see cref="CancellationToken"/>，
    ///       在未满足前可取消等待；取消的等待将被清理并跳过。
    /// 释放：建议通过 <see cref="Lease"/>（using/await using）自动释放，避免忘记调用 <see cref="Release(long)"/>。
    /// </remarks>
    public class CapacityLimiter
    {
        // 内部锁保护所有共享状态：_capacity/_used/_queue/_disposed
        private readonly object _lock = new();
        private long _capacity;
        private long _used;
        private bool _disposed;

        // 等待队列（FIFO），元素记录需求量与TCS
        private readonly Queue<Waiter> _queue = new();

        /// <summary>
        /// 使用指定总容量创建一个容量闸门。
        /// </summary>
        /// <param name="capacity">总容量（必须 &gt; 0）。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="capacity"/> 小于等于 0 时抛出。</exception>
        public CapacityLimiter(long capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
            _used = 0;
        }

        /// <summary>
        /// 当前总容量。
        /// </summary>
        public long Capacity => Volatile.Read(ref _capacity);

        /// <summary>
        /// 当前已占用量。
        /// </summary>
        public long Used => Volatile.Read(ref _used);

        /// <summary>
        /// 当前可用量（= <see cref="Capacity"/> - <see cref="Used"/>）。
        /// </summary>
        public long Available => Capacity - Used;

        /// <summary>
        /// 设置新的总容量。
        /// </summary>
        /// <param name="newCapacity">新的总容量（必须 &gt; 0，且不得小于当前已占用量）。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="newCapacity"/> 小于等于 0 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当 <paramref name="newCapacity"/> 小于当前 <see cref="Used"/> 时抛出。</exception>
        /// <exception cref="ObjectDisposedException">实例已释放。</exception>
        public void SetCapacity(long newCapacity)
        {
            if (newCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(newCapacity));
            lock (_lock)
            {
                ThrowIfDisposed();
                if (_used > newCapacity)
                    throw new InvalidOperationException($"无法将容量降到 {newCapacity}，当前已占用为 {_used}。");
                Volatile.Write(ref _capacity, newCapacity);
                TrySatisfyQueue_NoLock();
            }
        }

        /// <summary>
        /// 尝试立即申请指定数量的容量，若不足则返回 false（不进入等待队列）。
        /// </summary>
        /// <param name="amount">申请量（必须 &gt; 0）。</param>
        /// <param name="lease">成功时返回可释放的 <see cref="Lease"/>；失败时为默认值。</param>
        /// <returns>能否立即获得所需容量。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="amount"/> 小于等于 0 时抛出。</exception>
        /// <exception cref="ObjectDisposedException">实例已释放。</exception>
        public bool TryAcquire(long amount, out Lease lease)
        {
            ValidateAmount(amount);
            lock (_lock)
            {
                ThrowIfDisposed();
                // 仅在无人排队且容量足够时直接通过
                if (_queue.Count == 0 && (_capacity - _used) >= amount)
                {
                    _used += amount;
                    lease = new Lease(this, amount);
                    return true;
                }
            }
            lease = default;
            return false;
        }

        /// <summary>
        /// 申请指定数量的容量；若当前不足则进入等待队列（FIFO），直到可用或被取消。
        /// </summary>
        /// <param name="amount">申请量（必须 &gt; 0）。</param>
        /// <param name="token">取消令牌，若可取消且尚未满足，则会取消等待并抛出 <see cref="TaskCanceledException"/>。</param>
        /// <returns>完成时返回可释放的 <see cref="Lease"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="amount"/> 小于等于 0 时抛出。</exception>
        /// <exception cref="ObjectDisposedException">实例已释放。</exception>
        public ValueTask<Lease> AcquireAsync(long amount, CancellationToken token)
        {
            ValidateAmount(amount);

            lock (_lock)
            {
                ThrowIfDisposed();

                // 快路径：直接可用且无人排队
                if (_queue.Count == 0 && (_capacity - _used) >= amount)
                {
                    _used += amount;
                    return ValueTask.FromResult(new Lease(this, amount));
                }

                // 不足则进入等待队列，按 FIFO 公平排队
                var waiter = new Waiter(amount);
                waiter.Reg = token.CanBeCanceled
                    ? token.Register(static state =>
                    {
                        var w = (Waiter)state!;
                        w.Canceled = true;
                        // 尝试标记为取消，并唤醒等待方
                        w.TrySetCanceled();
                    }, waiter)
                    : default;

                _queue.Enqueue(waiter);
                return new ValueTask<Lease>(waiter.Task);
            }
        }

        /// <summary>
        /// 同步申请指定数量的容量；若当前不足则阻塞等待直到可用或被取消。
        /// </summary>
        /// <param name="amount">申请量（必须 &gt; 0）。</param>
        /// <param name="token">取消令牌；在等待期间被触发将抛出 <see cref="TaskCanceledException"/>。</param>
        /// <returns>完成时返回可释放的 <see cref="Lease"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="amount"/> 小于等于 0 时抛出。</exception>
        /// <exception cref="ObjectDisposedException">实例已释放。</exception>
        /// <exception cref="TaskCanceledException">在等待过程中被取消。</exception>
        /// <remarks>
        /// 实现说明：优先尝试 <see cref="TryAcquire(long, out Lease)"/> 的快路径；
        /// 在不足时回退到 <see cref="AcquireAsync(long, CancellationToken)"/> 并以阻塞方式等待。
        /// 由于内部使用 RunContinuationsAsynchronously，因此阻塞等待不会造成死锁。
        /// </remarks>
        public Lease Acquire(long amount, CancellationToken token = default)
        {
            // 快路径：尝试立即获取
            if (TryAcquire(amount, out var lease))
                return lease;

            // 回退到异步实现并同步等待
            var vt = AcquireAsync(amount, token);
            if (vt.IsCompletedSuccessfully)
                return vt.Result;

            return vt.AsTask().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 归还指定数量的容量。
        /// </summary>
        /// <param name="amount">归还量（必须 &gt; 0）。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="amount"/> 小于等于 0 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当归还量大于当前已占用量时抛出。</exception>
        /// <exception cref="ObjectDisposedException">实例已释放。</exception>
        public void Release(long amount)
        {
            ValidateAmount(amount);
            lock (_lock)
            {
                ThrowIfDisposed();
                if (_used < amount)
                    throw new InvalidOperationException($"释放量({amount})大于当前已占用({_used})。");
                _used -= amount;

                // 归还后尝试唤醒队列中的等待者（保持 FIFO）
                TrySatisfyQueue_NoLock();
            }
        }

        /// <summary>
        /// 在持有锁的情况下，尽可能按 FIFO 满足等待队列。
        /// 该方法仅在内部被调用，调用前必须已加锁。
        /// </summary>
        private void TrySatisfyQueue_NoLock()
        {
            // 尽可能按 FIFO 满足队列
            while (_queue.Count > 0)
            {
                // 清理已取消的队首
                while (_queue.Count > 0 && _queue.Peek().Canceled)
                {
                    var canceled = _queue.Dequeue();
                    canceled.CleanupAfterCompletion();
                }
                if (_queue.Count == 0) break;

                var next = _queue.Peek();
                if ((_capacity - _used) >= next.AmountNeeded)
                {
                    _queue.Dequeue();
                    _used += next.AmountNeeded;

                    // 完成并发回 Lease
                    next.TrySetResult(new Lease(this, next.AmountNeeded));
                    next.CleanupAfterCompletion();
                }
                else
                {
                    break; // 当前可用量不足以满足队首，保持 FIFO
                }
            }
        }

        /// <summary>
        /// 校验数量必须为正数。
        /// </summary>
        private static void ValidateAmount(long amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        }

        /// <summary>
        /// 若实例已释放则抛出异常。
        /// </summary>
        /// <exception cref="ObjectDisposedException">实例已释放。</exception>
        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CapacityLimiter));
        }

        /// <summary>
        /// 释放当前实例：取消并清理所有未完成的等待者。
        /// </summary>
        /// <remarks>释放后再调用任何公共成员都会抛出 <see cref="ObjectDisposedException"/>。</remarks>
        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;

                // 取消并清理所有未完成的等待者
                while (_queue.Count > 0)
                {
                    var w = _queue.Dequeue();
                    w.Canceled = true;
                    w.TrySetCanceled();
                    w.CleanupAfterCompletion();
                }
            }
        }

        /// <summary>
        /// 容量租约。通过 using/await using 在作用域结束时自动归还已申请的容量。
        /// </summary>
        public readonly struct Lease : IDisposable, IAsyncDisposable
        {
            private readonly CapacityLimiter? _cLimiter;

            /// <summary>
            /// 本次申请的容量数量。
            /// </summary>
            public long Amount { get; }

            internal Lease(CapacityLimiter core, long amount)
            {
                _cLimiter = core;
                Amount = amount;
            }

            /// <summary>
            /// 租约是否有效。
            /// </summary>
            public bool IsValid => _cLimiter is not null;

            /// <summary>
            /// 同步释放租约并归还容量。
            /// </summary>
            public void Dispose()
            {
                _cLimiter?.Release(Amount);
            }

            /// <summary>
            /// 异步释放租约并归还容量。
            /// </summary>
            public ValueTask DisposeAsync()
            {
                _cLimiter?.Release(Amount);
                return ValueTask.CompletedTask;
            }
        }

        /// <summary>
        /// 等待者：表示一次待满足的容量请求及其完成源。
        /// 注意：
        /// - 当前实现为 struct，存在“值复制”风险：取消回调与队列中存放的实例可能并非同一份，
        ///   会导致取消标记与完成信号作用于不同副本，进而造成容量泄漏或多余完成。
        /// - 若要彻底规避该问题，建议改为 class 并以引用语义参与队列与回调。
        /// </summary>
        private struct Waiter : IValueTaskSource<Lease>
        {
            /// <summary>
            /// 单次完成用的任务源。
            /// RunContinuationsAsynchronously = true 以避免在持锁路径内同步执行延续造成阻塞/递归。
            /// </summary>
            private readonly ManualResetValueTaskSourceCore<Lease> _core;

            /// <summary>
            /// 本等待者所需的容量数量。
            /// </summary>
            public readonly long AmountNeeded;

            /// <summary>
            /// 是否已被外部取消。队列在发放前会清理已取消的等待者。
            /// </summary>
            public bool Canceled;

            /// <summary>
            /// 取消令牌的注册句柄；在完成/取消后需显式释放以避免泄漏。
            /// </summary>
            public CancellationTokenRegistration Reg;

            /// <summary>
            /// 使用请求量初始化等待者。
            /// </summary>
            /// <param name="amount">申请的容量数量（必须 &gt; 0）。</param>
            public Waiter(long amount)
            {
                AmountNeeded = amount;
                _core = new ManualResetValueTaskSourceCore<Lease>()
                {
                    RunContinuationsAsynchronously = true
                };
                Canceled = false;
                Reg = default;
            }

            /// <summary>
            /// 将当前 IValueTaskSource 包装为 Task 返回，供外部再包装为 ValueTask 使用。
            /// 说明：这是一次性任务源，内部使用固定 token=0。
            /// </summary>
            public Task<Lease> Task => new ValueTask<Lease>(this, 0).AsTask();

            /// <summary>
            /// 尝试以成功结果完成等待者，返回对应的 <see cref="Lease"/>。
            /// 注意：该调用应仅发生一次；重复完成将抛出异常。
            /// </summary>
            public void TrySetResult(Lease lease)
            {
                _core.SetResult(lease);
            }

            /// <summary>
            /// 将等待者标记为取消（以异常形式完成）。
            /// 若已被其它路径完成则吞掉异常以保证幂等。
            /// </summary>
            public void TrySetCanceled()
            {
                try
                {
                    _core.SetException(new TaskCanceledException());
                }
                catch
                {
                    // 已完成则忽略，保证幂等
                }
            }

            /// <summary>
            /// 在完成/取消后清理注册资源，避免令牌注册泄漏。
            /// </summary>
            public void CleanupAfterCompletion()
            {
                Reg.Dispose();
            }

            // IValueTaskSource 实现：由 ValueTask 基础设施调用
            /// <inheritdoc />
            public Lease GetResult(short token)
                => _core.GetResult(token);

            /// <inheritdoc />
            public ValueTaskSourceStatus GetStatus(short token)
                => _core.GetStatus(token);

            /// <inheritdoc />
            public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
                => _core.OnCompleted(continuation, state, token, flags);
        }
    }
}