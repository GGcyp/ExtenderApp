using System.Collections.Concurrent;
using System.Threading.Tasks.Sources;

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 表示一个按总容量进行配额控制的闸门。线程安全，支持按权重申请与释放容量，并在容量不足时按 FIFO 排队等待。
    /// </summary>
    /// <remarks>用途示例：发送限流（按字节）、并发门控（按任务权重）、批量处理（容量不足时排队）。 队列保证 FIFO 公平性：只有当队首请求可满足时才发放容量。建议使用 <see cref="Lease"/> 自动归还容量以避免泄漏。</remarks>
    public class CapacityLimiter : DisposableObject
    {
        /// <summary>
        /// 等待者对象池，复用以减少分配。
        /// </summary>
        private static readonly ConcurrentStack<Waiter> _waiterPool = new();

        /// <summary>
        /// 从对象池租用一个 <see cref="Waiter"/> 并初始化其请求信息。
        /// </summary>
        /// <param name="amount">请求的容量数量。</param>
        /// <param name="token">用于取消等待的令牌。</param>
        /// <returns>已初始化的等待者实例。</returns>
        private static Waiter RentWaiter(long amount, CancellationToken token)
        {
            if (!_waiterPool.TryPop(out var waiter))
            {
                waiter = new Waiter();
            }
            waiter.AmountNeeded = amount;
            waiter.Token = token;
            return waiter;
        }

        /// <summary>
        /// 将等待者返回对象池并清理引用字段。
        /// </summary>
        /// <param name="waiter">要回收的等待者。</param>
        private static void ReturnWaiter(Waiter waiter)
        {
            waiter.Token = default; // 确保不持有过期的 CancellationToken
            _waiterPool.Push(waiter);
        }

        /// <summary>
        /// 保护内部状态的锁对象。
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// 总容量（由 <see cref="Capacity"/> 公开）。
        /// </summary>
        private long _capacity;

        /// <summary>
        /// 当前已占用的容量（由 <see cref="Used"/> 公开）。
        /// </summary>
        private long _used;

        /// <summary>
        /// 单次申请上限的内部存储（由 <see cref="MaxSingleAmount"/> 公开）。
        /// </summary>
        private long _maxSingleAmount;

        /// <summary>
        /// 等待队列（FIFO），每项为一个等待的请求及其完成源。
        /// </summary>
        private readonly Queue<Waiter> _queue = new();

        /// <summary>
        /// 使用指定的总容量创建一个 <see cref="CapacityLimiter"/> 实例。
        /// </summary>
        /// <param name="capacity">总容量（必须大于 0）。</param>
        /// <param name="maxSingleAmount">单次申请上限（大于 0 表示启用限制；小于等于 0 表示无上限）。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="capacity"/> 小于等于 0，或 <paramref name="maxSingleAmount"/> 大于 capacity 时抛出。
        /// </exception>
        public CapacityLimiter(long capacity, long maxSingleAmount = 0)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            if (maxSingleAmount > 0 && maxSingleAmount > capacity)
                throw new ArgumentOutOfRangeException(nameof(maxSingleAmount));

            _capacity = capacity;
            _used = 0;
            _maxSingleAmount = maxSingleAmount;
        }

        /// <summary>
        /// 获取当前的单次申请上限。若值小于等于 0，则表示不限制单次申请大小。
        /// </summary>
        public long MaxSingleAmount => Volatile.Read(ref _maxSingleAmount);

        /// <summary>
        /// 设置单次申请上限。
        /// </summary>
        /// <param name="maxSingleAmount">新的单次申请上限（大于 0 启用限制；小于等于 0 表示不限制）。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="maxSingleAmount"/> 大于当前总容量时抛出。</exception>
        /// <exception cref="ObjectDisposedException">当实例已释放时抛出。</exception>
        public void SetMaxSingleAmount(long maxSingleAmount)
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                if (maxSingleAmount > 0 && maxSingleAmount > _capacity)
                    throw new ArgumentOutOfRangeException(nameof(maxSingleAmount));
                Volatile.Write(ref _maxSingleAmount, maxSingleAmount);
            }
        }

        /// <summary>
        /// 获取当前总容量。
        /// </summary>
        public long Capacity => Volatile.Read(ref _capacity);

        /// <summary>
        /// 获取当前已占用的容量数量。
        /// </summary>
        public long Used => Volatile.Read(ref _used);

        /// <summary>
        /// 获取当前可用容量（等于 <see cref="Capacity"/> 减去 <see cref="Used"/>）。
        /// </summary>
        public long Available => Capacity - Used;

        /// <summary>
        /// 更新总容量。新的容量必须大于 0 且不得小于当前已占用量，否则抛出异常。 更新后会尝试唤醒等待队列以分发刚释放出来的容量。
        /// </summary>
        /// <param name="newCapacity">新的总容量。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="newCapacity"/> 小于等于 0 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当 <paramref name="newCapacity"/> 小于当前已占用量时抛出。</exception>
        /// <exception cref="ObjectDisposedException">当实例已释放时抛出。</exception>
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
        /// 尝试立即获取指定数量的容量。如果当前可用且无人排队，则立即分配并返回 true；否则返回 false，不进入等待队列。
        /// </summary>
        /// <param name="amount">请求的容量数量。</param>
        /// <param name="lease">成功时返回对应的 <see cref="Lease"/>，失败时为默认值。</param>
        /// <returns>是否成功分配到容量。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="amount"/> 小于等于 0 或超出单次上限时抛出。</exception>
        /// <exception cref="ObjectDisposedException">当实例已释放时抛出。</exception>
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
        /// 异步申请指定数量的容量。若当前不足则将请求排入 FIFO 等待队列，直到可满足或被取消。
        /// </summary>
        /// <param name="amount">请求的容量数量。</param>
        /// <param name="token">用于取消等待的令牌。</param>
        /// <returns>一个完成时携带 <see cref="Lease"/> 的 <see cref="ValueTask{TResult}"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="amount"/> 小于等于 0 或超出单次上限时抛出。</exception>
        /// <exception cref="ObjectDisposedException">当实例已释放时抛出。</exception>
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
                var waiter = RentWaiter(amount, token);
                _queue.Enqueue(waiter);
                return waiter;
            }
        }

        /// <summary>
        /// 同步方式申请容量。优先尝试快速路径；若不可用则回退到 <see cref="AcquireAsync(long, CancellationToken)"/> 并阻塞等待结果。
        /// </summary>
        /// <param name="amount">请求的容量数量。</param>
        /// <returns>完成时返回对应的 <see cref="Lease"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="amount"/> 小于等于 0 或超出单次上限时抛出。</exception>
        /// <exception cref="ObjectDisposedException">当实例已释放时抛出。</exception>
        /// <exception cref="TaskCanceledException">等待期间若被取消则抛出。</exception>
        public Lease Acquire(long amount)
        {
            // 快路径：尝试立即获取
            if (TryAcquire(amount, out var lease))
                return lease;

            // 回退到异步实现并同步等待
            var vt = AcquireAsync(amount, default);
            if (vt.IsCompletedSuccessfully)
                return vt.Result;

            return vt.GetAwaiter().GetResult();
        }

        /// <summary>
        /// 归还已占用的容量并尝试唤醒等待队列中的请求。
        /// </summary>
        /// <param name="amount">要归还的容量数量。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="amount"/> 小于等于 0 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当归还量大于当前已占用量时抛出。</exception>
        /// <exception cref="ObjectDisposedException">当实例已释放时抛出。</exception>
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
        /// 在持有内部锁的前提下，按 FIFO 顺序尽可能满足队列中的等待请求。 仅供内部调用，调用时必须已持有 <see cref="_lock"/>。
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
                    ReturnWaiter(canceled);
                }
                if (_queue.Count == 0) break;

                var next = _queue.Peek();
                if ((_capacity - _used) >= next.AmountNeeded)
                {
                    _queue.Dequeue();
                    _used += next.AmountNeeded;

                    // 完成并发回 Lease
                    next.SetResult(this);
                }
                else
                {
                    break; // 当前可用量不足以满足队首，保持 FIFO
                }
            }
        }

        /// <summary>
        /// 验证请求/释放数量是否合法（必须大于 0 且不超过单次上限）。
        /// </summary>
        /// <param name="amount">待校验的数量。</param>
        /// <exception cref="ArgumentOutOfRangeException">当数量不合法时抛出。</exception>
        private void ValidateAmount(long amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            var maxSingleAmount = Volatile.Read(ref _maxSingleAmount);
            if (maxSingleAmount > 0 && amount > maxSingleAmount)
                throw new ArgumentOutOfRangeException(nameof(amount));
        }

        /// <summary>
        /// 释放托管资源：取消并清理所有仍在等待队列中的请求。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            lock (_lock)
            {
                // 取消并清理所有未完成的等待者
                while (_queue.Count > 0)
                {
                    var w = _queue.Dequeue();
                    w.SetCanceled();
                }
            }
        }

        /// <summary>
        /// 表示一次已分配的容量租约。使用 <c>using</c> 或 <c>await using</c> 可在作用域结束时自动归还容量。
        /// </summary>
        public readonly struct Lease : IDisposable, IAsyncDisposable
        {
            private readonly CapacityLimiter? _cLimiter;

            /// <summary>
            /// 本次租约包含的容量数量。
            /// </summary>
            public long Amount { get; }

            internal Lease(CapacityLimiter core, long amount)
            {
                _cLimiter = core;
                Amount = amount;
            }

            /// <summary>
            /// 指示此租约是否有效（是否绑定到一个 <see cref="CapacityLimiter"/> 实例）。
            /// </summary>
            public bool IsValid => _cLimiter is not null;

            /// <summary>
            /// 同步释放租约并将容量归还到所属的 <see cref="CapacityLimiter"/>。
            /// </summary>
            public void Dispose()
            {
                _cLimiter?.Release(Amount);
            }

            /// <summary>
            /// 异步释放租约（实现 IAsyncDisposable），并将容量归还。
            /// </summary>
            public ValueTask DisposeAsync()
            {
                _cLimiter?.Release(Amount);
                return ValueTask.CompletedTask;
            }
        }

        /// <summary>
        /// 内部等待者：封装一次待满足的容量请求及其 <see cref="IValueTaskSource{TResult}"/> 完成源。 实现为 class 以避免值复制带来的语义问题。
        /// </summary>
        private class Waiter : IValueTaskSource<Lease>
        {
            /// <summary>
            /// 用于一次性完成的任务源，异步执行延续以避免在持锁路径内执行回调。
            /// </summary>
            private ManualResetValueTaskSourceCore<Lease> vts;

            /// <summary>
            /// 请求所需的容量数量。
            /// </summary>
            public long AmountNeeded;

            /// <summary>
            /// 是否已通过外部 CancellationToken 被取消。
            /// </summary>
            public bool Canceled => Token.IsCancellationRequested;

            public short Version => vts.Version;

            /// <summary>
            /// 与此等待者关联的取消令牌。
            /// </summary>
            public CancellationToken Token;

            /// <summary>
            /// 创建一个新的等待者并初始化内部任务源。
            /// </summary>
            public Waiter()
            {
                vts = new();
            }

            /// <summary>
            /// 以成功结果完成等待者并传回对应的 <see cref="Lease"/>。 如果已被取消或已完成则不会重复完成。
            /// </summary>
            /// <param name="capacityLimiter">产生租约的闸门实例。</param>
            public void SetResult(CapacityLimiter capacityLimiter)
            {
                if (Canceled)
                    return;

                vts.SetResult(new Lease(capacityLimiter, AmountNeeded));
            }

            /// <summary>
            /// 将等待者以取消状态完成（抛出 <see cref="TaskCanceledException"/>）。 幂等：若已完成则忽略异常。
            /// </summary>
            public void SetCanceled()
            {
                try
                {
                    vts.SetException(new TaskCanceledException());
                }
                catch
                {
                    // 已完成则忽略，保证幂等
                }
            }

            /// <inheritdoc/>
            public Lease GetResult(short token)
            {
                try
                {
                    return vts.GetResult(token);
                }
                finally
                {
                    ReturnWaiter(this);
                }
            }

            /// <inheritdoc/>
            public ValueTaskSourceStatus GetStatus(short token)
                => vts.GetStatus(token);

            /// <inheritdoc/>
            public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
                => vts.OnCompleted(continuation, state, token, flags);

            public static implicit operator ValueTask<Lease>(Waiter waiter) => new ValueTask<Lease>(waiter, waiter.Version);
        }
    }
}