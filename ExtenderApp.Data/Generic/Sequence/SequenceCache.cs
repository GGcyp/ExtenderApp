using System.Buffers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 针对 <see cref="Sequence{T}"/> 的轻量缓存持有器。
    /// - 延迟从 <see cref="SequencePool{T}"/> 租用序列（租约保存在内部）；
    /// - 提供写入/读取方法（基于 <see cref="Sequence{T}"/> 与临时 <see cref="SequenceReader{T}"/>）；
    /// - 当序列中无未读数据时，会自动释放（将租约归还到池中）。
    /// </summary>
    /// <typeparam name="T">元素类型，必须是无偏置的非托管类型并实现 <see cref="IEquatable{T}"/>。</typeparam>
    public class SequenceCache<T> : DisposableObject where T : unmanaged, IEquatable<T>
    {
        private readonly SequencePool<T> _pool;

        // 当前租约（为空表示未租用）
        private SequencePool<T>.SequenceRental _rental = SequencePool<T>.SequenceRental.Empty;

        // 为方便使用也缓存当前 Sequence 引用（当 _rental.IsEmpty == false 时有效）
        private Sequence<T>? sequence;

        // 已消费（读取）的元素总数（相对于 Sequence.WrittenCount 的偏移）
        private long consumed;

        /// <summary>
        /// 初始化 <see cref="SequenceCache{T}"/> 类的新实例，使用 <see cref="SequencePool{T}.Shared"/>。
        /// </summary>
        public SequenceCache() : this(SequencePool<T>.Shared)
        {
        }

        /// <summary>
        /// 使用指定的序列池构造。如果为 null 则使用 <see cref="SequencePool{T}.Shared"/>。
        /// </summary>
        /// <param name="pool">序列池。</param>
        public SequenceCache(SequencePool<T> pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// 获取当前是否持有租用的序列。
        /// </summary>
        public bool HasSequence => !_rental.IsEmpty;

        /// <summary>
        /// 获取当前序列中剩余（未读取）的元素数。
        /// </summary>
        public long Remaining
        {
            get
            {
                if (sequence == null)
                    return 0;
                return Math.Max(0L, sequence.Length - consumed);
            }
        }

        /// <summary>
        /// 获取一个值，指示当前是否为空：既未持有可写序列，又没有未读数据。
        /// </summary>
        public bool IsEmpty => !HasSequence || Remaining == 0;

        /// <summary>
        /// 确保已租用序列（延迟租用）。
        /// </summary>
        private void EnsureSequence()
        {
            if (_rental.IsEmpty)
            {
                _rental = _pool.Rent();
                sequence = _rental.Value;
                consumed = 0;
            }
        }

        /// <summary>
        /// 尝试在剩余为空时释放（归还）当前租约。
        /// </summary>
        private void TryReleaseIfEmpty()
        {
            if (!_rental.IsEmpty && sequence != null && sequence.Length == consumed)
            {
                _rental.Dispose();
                _rental = SequencePool<T>.SequenceRental.Empty;
                sequence = null;
                consumed = 0;
            }
        }

        /// <summary>
        /// 申请可写 Span（会延迟租用序列）。
        /// </summary>
        /// <param name="sizeHint">期望的最小连续容量。</param>
        /// <returns>返回可用的 <see cref="Span{T}"/>。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Span<T> GetSpan(int sizeHint = 0)
        {
            EnsureNotDisposed();
            EnsureSequence();
            return sequence!.GetSpan(sizeHint);
        }

        /// <summary>
        /// 申请可写 Memory（会延迟租用序列）。
        /// </summary>
        /// <param name="sizeHint">期望的最小连续容量。</param>
        /// <returns>返回可用的 <see cref="Memory{T}"/>。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Memory<T> GetMemory(int sizeHint = 0)
        {
            EnsureNotDisposed();
            EnsureSequence();
            return sequence!.GetMemory(sizeHint);
        }

        /// <summary>
        /// 将先前通过 <see cref="GetSpan(int)"/> / <see cref="GetMemory(int)"/> 获取的写缓冲提交为已写入长度。
        /// </summary>
        /// <param name="count">已写入的元素数量。</param>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        /// <exception cref="InvalidOperationException">尚未租用序列以写入时抛出。</exception>
        public void WriteAdvance(int count)
        {
            EnsureNotDisposed();
            if (_rental.IsEmpty)
                throw new InvalidOperationException("尚未租用序列以写入。");

            sequence!.Advance(count);
        }

        /// <summary>
        /// 直接写入一段只读跨度数据（会延迟租用序列）。
        /// </summary>
        /// <param name="data">要写入的数据。</param>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public void Write(ReadOnlySpan<T> data)
        {
            EnsureNotDisposed();
            if (data.IsEmpty)
                return;

            EnsureSequence();
            sequence!.Write(data);
            WriteAdvance(data.Length);
        }

        /// <summary>
        /// 从序列当前位置尝试读取一个元素并前进（消费）。
        /// </summary>
        /// <param name="value">输出读取到的值；若读取失败则为默认值。</param>
        /// <returns>若成功读取返回 true；否则返回 false。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public bool TryRead(out T value)
        {
            EnsureNotDisposed();
            value = default!;
            if (sequence == null || Remaining == 0)
                return false;

            var reader = new SequenceReader<T>((ReadOnlySequence<T>)sequence);
            reader.Advance(consumed);
            if (!reader.TryRead(out value))
                return false;

            consumed = reader.Consumed;
            TryReleaseIfEmpty();
            return true;
        }

        /// <summary>
        /// 将数据读取到目标跨度并前进（消费）。
        /// </summary>
        /// <param name="destination">目标跨度。</param>
        /// <returns>实际读取并填充到 <paramref name="destination"/> 的元素数量。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public int Read(Span<T> destination)
        {
            EnsureNotDisposed();
            if (sequence == null || destination.IsEmpty || Remaining == 0)
                return 0;

            var reader = new SequenceReader<T>((ReadOnlySequence<T>)sequence);
            reader.Advance(consumed);

            int totalRead = 0;
            var seq = reader.Sequence.Slice(reader.Position);
            foreach (var seg in seq)
            {
                var span = seg.Span;
                int toCopy = Math.Min(span.Length, destination.Length - totalRead);
                if (toCopy <= 0)
                    break;

                span.Slice(0, toCopy).CopyTo(destination.Slice(totalRead, toCopy));
                totalRead += toCopy;
                if (totalRead == destination.Length)
                    break;
            }

            consumed += totalRead;
            TryReleaseIfEmpty();
            return totalRead;
        }

        /// <summary>
        /// 将读取位置向前移动指定元素数（相当于消费）。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        /// <exception cref="InvalidOperationException">没有可租用的序列时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">尝试前进的长度超过了剩余可用数据长度。</exception>
        public void ReadAdvance(long count)
        {
            EnsureNotDisposed();
            if (count <= 0)
                return;
            if (sequence == null)
                throw new InvalidOperationException("无序列可读取。");
            if (count > Remaining)
                throw new ArgumentOutOfRangeException(nameof(count), "尝试前进超过剩余长度。");

            consumed += count;
            TryReleaseIfEmpty();
        }

        /// <summary>
        /// 清空并立即释放当前租约（若存在）。
        /// </summary>
        public void Reset()
        {
            if (!_rental.IsEmpty)
            {
                _rental.Dispose();
                _rental = SequencePool<T>.SequenceRental.Empty;
                sequence = null;
                consumed = 0;
            }
        }

        /// <summary>
        /// 获取针对当前缓存序列的 <see cref="SequenceReader{T}"/>。
        /// </summary>
        /// <returns>返回定位在当前消费偏移处的 <see cref="SequenceReader{T}"/>。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public SequenceReader<T> GetSequenceReader()
        {
            EnsureNotDisposed();
            if (sequence == null)
                return new SequenceReader<T>(ReadOnlySequence<T>.Empty);
            var reader = new SequenceReader<T>((ReadOnlySequence<T>)sequence);
            reader.Advance(consumed);
            return reader;
        }

        /// <inheritdoc/>
        protected override void DisposeManagedResources()
        {
            Reset();
        }

        /// <summary>
        /// 检查对象是否已释放，若已释放则抛出异常。
        /// </summary>
        private void EnsureNotDisposed()
        {
            ThrowIfDisposed();
        }
    }
}