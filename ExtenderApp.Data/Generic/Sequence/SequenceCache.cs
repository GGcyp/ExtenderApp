using System.Buffers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 针对 <see cref="Sequence{T}"/> 的轻量缓存持有器。
    /// - 延迟从 <see cref="SequencePool{T}"/> 租用序列（租约保存在内部）；
    /// - 提供写入/读取方法（基于 <see cref="Sequence{T}"/> 与临时 <see cref="SequenceReader{T}"/>）；
    /// - 当序列中无未读数据时，会自动释放（将租约归还到池中）。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    public class SequenceCache<T> : DisposableObject where T : unmanaged, IEquatable<T>
    {
        private readonly SequencePool<T> _pool;

        // 当前租约（为空表示未租用）
        private SequencePool<T>.SequenceRental _rental = SequencePool<T>.SequenceRental.Empty;

        // 为方便使用也缓存当前 Sequence 引用（当 _rental.IsEmpty == false 时有效）
        private Sequence<T>? sequence;

        // 已消费（读取）的元素总数（相对于 sequence.Length 的偏移）
        private long consumed;

        public SequenceCache() : this(SequencePool<T>.Shared)
        {
        }

        /// <summary>
        /// 使用指定的序列池构造。如果为 null 则使用 <see cref="SequencePool{T}.Shared"/>.
        /// </summary>
        /// <param name="pool">序列池。</param>
        public SequenceCache(SequencePool<T> pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// 当前是否持有租用的序列。
        /// </summary>
        public bool HasSequence => !_rental.IsEmpty;

        /// <summary>
        /// 当前序列中剩余（未读取）的元素数。
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
        /// 是否为空：既未持有可写序列，又没有未读数据。
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
        /// <returns>可写 Span。</returns>
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
        /// <returns>可写 Memory。</returns>
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
        /// 若不存在数据返回 false。
        /// </summary>
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
        /// 将数据读取到目标跨度并前进（消费），返回实际读取的元素数量（可能小于目标长度）。
        /// </summary>
        /// <param name="destination">目标跨度。</param>
        /// <returns>实际读取数量。</returns>
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

        public SequenceReader<T> GetSequenceReader()
        {
            EnsureNotDisposed();
            if (sequence == null)
                return new SequenceReader<T>(ReadOnlySequence<T>.Empty);
            var reader = new SequenceReader<T>((ReadOnlySequence<T>)sequence);
            reader.Advance(consumed);
            return reader;
        }

        protected override void DisposeManagedResources()
        {
            Reset();
        }

        private void EnsureNotDisposed()
        {
            ThrowIfDisposed();
        }
    }
}