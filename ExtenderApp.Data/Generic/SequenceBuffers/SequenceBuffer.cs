using System.Buffers;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 轻量的顺序写入 + 顺序读取封装。
    /// - 通过 <see cref="Sequence{T}"/> 提供写缓冲（ <see cref="GetSpan"/> / <see cref="GetMemory"/> / <see cref="Write(in T)"/> 等）；
    /// - 通过内部 <see cref="SequenceReader{T}"/> 提供只读的顺序读取（ <see cref="TryRead(out T)"/> / <see cref="TryRead(int, out ReadOnlySequence{T})"/> / <see
    ///   cref="TryPeek(out T)"/> 等）；
    /// - 写入后会将读取视图标记为“脏”，下次访问 <see cref="UpdateReader()"/> 时自动刷新到最新快照。 注意：
    /// 1) 本类型为 ref struct，不可装箱、不可捕获到闭包、不可跨异步、不可存入堆结构；
    /// 2) 非线程安全：同一实例请勿在多线程并发读写。
    /// </summary>
    /// <typeparam name="T">元素类型，必须为非托管并实现 <see cref="IEquatable{T}"/>。</typeparam>
    public ref struct SequenceBuffer<T>
        where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        /// 可写序列（拥有期由本实例托管）。为 null 代表只读快照模式。
        /// </summary>
        private readonly Sequence<T>? _sequence;

        /// <summary>
        /// 序列池租约，负责释放从 <see cref="SequencePool{T}"/> 租用的 <see cref="_sequence"/>。
        /// </summary>
        public SequencePool<T>.SequenceRental Rental;

        /// <summary>
        /// 读取视图是否已失效（发生写入或申请写缓冲后置为 true）。
        /// </summary>
        private bool readerDirty;

        /// <summary>
        /// 内部读取器，指向当前只读快照。
        /// </summary>
        private SequenceReader<T> reader;

        /// <summary>
        /// 序列读取器。
        /// </summary>
        public SequenceReader<T> Reader
        {
            get
            {
                UpdateReader();
                return reader;
            }
        }

        /// <summary>
        /// 获取当前读取器所在的跨度索引。
        /// </summary>
        public int CurrentSpanIndex
        {
            get
            {
                UpdateReader();
                return reader.CurrentSpanIndex;
            }
        }

        /// <summary>
        /// 剩余未读取的元素数量。
        /// </summary>
        public long Remaining
        {
            get
            {
                UpdateReader();
                return reader.Remaining;
            }
        }

        /// <summary>
        /// 序列的总长度。
        /// </summary>
        public long Length
        {
            get
            {
                UpdateReader();
                return reader.Length;
            }
        }

        /// <summary>
        /// 已消耗（读取）元素数量。
        /// </summary>
        public long Consumed => reader.Consumed;

        /// <summary>
        /// 当前读取位置。
        /// </summary>
        public SequencePosition Position
        {
            get
            {
                UpdateReader();
                return reader.Position;
            }
        }

        /// <summary>
        /// 是否已到达序列末尾。
        /// </summary>
        public bool End
        {
            get
            {
                UpdateReader();
                return reader.End;
            }
        }

        /// <summary>
        /// 当前读取器绑定的只读序列快照。
        /// </summary>
        public ReadOnlySequence<T> Sequence
        {
            get
            {
                UpdateReader();
                return reader.Sequence;
            }
        }

        /// <summary>
        /// 未读取的只读序列。
        /// </summary>
        public ReadOnlySequence<T> UnreadSequence
        {
            get
            {
                UpdateReader();
                return reader.UnreadSequence;
            }
        }

        /// <summary>
        /// 当前数据片段的只读跨度。
        /// </summary>
        public ReadOnlySpan<T> CurrentSpan
        {
            get
            {
                UpdateReader();
                return reader.CurrentSpan;
            }
        }

        /// <summary>
        /// 当前数据片段中尚未读取的只读跨度。
        /// </summary>
        public ReadOnlySpan<T> UnreadSpan
        {
            get
            {
                UpdateReader();
                return reader.UnreadSpan;
            }
        }

        /// <summary>
        /// 获取当前块是否支持写入（持有可写序列）。
        /// </summary>
        public bool CanWrite => _sequence != null;

        /// <summary>
        /// 是否为空：当未持有可写序列且读取器中无数据时为 true。
        /// </summary>
        public bool IsEmpty => _sequence == null && reader.Length == 0;

        public SequenceBuffer(MemoryBlock<T> block) : this(readOnlySequence: new(block.UnreadMemory))
        {
        }

        /// <summary>
        /// 通过序列池构造并获取一个可写序列的租约。
        /// </summary>
        /// <param name="pool">序列池。</param>
        /// <remarks>生命周期结束时调用 <see cref="Dispose"/> 归还租约。</remarks>
        public SequenceBuffer(SequencePool<T> pool)
        {
            Rental = pool.Rent();
            _sequence = Rental.Value;
        }

        /// <summary>
        /// 使用给定的只读内存构造，对应一个单段序列（只读）。
        /// </summary>
        /// <param name="memory">只读内存。</param>
        public SequenceBuffer(ReadOnlyMemory<T> memory) : this(readOnlySequence: new(memory))
        {
        }

        /// <summary>
        /// 使用只读序列构造，无法进行写入，仅能读取。
        /// </summary>
        /// <param name="readOnlySequence">只读序列快照。</param>
        public SequenceBuffer(ReadOnlySequence<T> readOnlySequence) : this(new SequenceReader<T>(readOnlySequence))
        {
        }

        public SequenceBuffer(SequenceReader<T> reader)
        {
            this.reader = reader;
        }

        public SequenceBuffer(in SequenceBuffer<T> other)
        {
            reader = other.reader;
        }

        /// <summary>
        /// 申请一个可写的 <see cref="Span{T}"/>，用于直接写入。 申请写缓冲后会使读取视图变脏，下一次读取将刷新。
        /// </summary>
        /// <param name="sizeHint">期望大小（提示值，可为 0）。</param>
        /// <exception cref="ObjectDisposedException">当未持有可写序列时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int sizeHint = 0)
        {
            return _sequence!.GetSpan(sizeHint);
        }

        /// <summary>
        /// 申请一个可写的 <see cref="Memory{T}"/>，用于异步/延迟写入。 申请写缓冲后会使读取视图变脏，下一次读取将刷新。
        /// </summary>
        /// <param name="sizeHint">期望大小（提示值，可为 0）。</param>
        /// <exception cref="ObjectDisposedException">当未持有可写序列时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> GetMemory(int sizeHint = 0)
        {
            return _sequence!.GetMemory(sizeHint);
        }

        /// <summary>
        /// 更新读取器以反映最新的写入状态（若已脏）。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateReader()
        {
            if (!readerDirty)
            {
                return;
            }

            var lastReader = reader;
            reader = new SequenceReader<T>(_sequence);
            reader.Advance(lastReader.Consumed);
            readerDirty = false;
        }

        /// <summary>
        /// 提交此前通过 <see cref="GetSpan(int)"/> 或 <see cref="GetMemory(int)"/> 获取的写缓冲中已写入的元素数， 前进写入位置并使读取快照失效。
        /// </summary>
        /// <param name="count">已写入且需要提交的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteAdvance(int count)
        {
            _sequence?.Advance(count);
            readerDirty = true;
        }

        /// <summary>
        /// 将读取位置向前移动指定的元素数，相当于跳过这些元素。
        /// </summary>
        /// <param name="count">要跳过的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadAdvance(long count)
        {
            UpdateReader();
            reader.Advance(count);
        }

        /// <summary>
        /// 尝试将读取位置向前移动指定的元素数。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        /// <returns>若剩余长度不足 <paramref name="count"/>，则不移动并返回 false；否则前进并返回 true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadAdvance(long count)
        {
            if (Remaining < count)
                return false;

            ReadAdvance(count);
            return true;
        }

        /// <summary>
        /// 追加单个元素。
        /// </summary>
        /// <param name="value">要追加的元素。</param>
        /// <exception cref="ObjectDisposedException">当未持有可写序列时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in T value)
        {
            _sequence!.Write(new(in value));
            readerDirty = true;
        }

        /// <summary>
        /// 追加一段只读跨度数据。
        /// </summary>
        /// <param name="value">要追加的数据。</param>
        /// <exception cref="ObjectDisposedException">当未持有可写序列时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(scoped ReadOnlySpan<T> value)
        {
            if (value.IsEmpty)
                return;

            _sequence!.Write(value);
            readerDirty = true;
        }

        /// <summary>
        /// 追加一段只读内存数据。
        /// </summary>
        /// <param name="value">要追加的数据。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in ReadOnlyMemory<T> value)
        {
            Write(value.Span);
        }

        /// <summary>
        /// 追加一个只读序列（可能为多段）。
        /// </summary>
        /// <param name="value">要追加的数据。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in ReadOnlySequence<T> value)
        {
            if (value.IsEmpty)
                return;

            foreach (var segment in value)
            {
                _sequence!.Write(segment.Span);
            }
            readerDirty = true;
        }

        /// <summary>
        /// 从当前位置尝试读取一个元素并前进。
        /// </summary>
        /// <param name="value">输出读取到的元素。</param>
        /// <returns>读取成功返回 true，否则 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead(out T value)
        {
            UpdateReader();
            if (reader.TryRead(out value))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 从当前位置尝试读取 count 个元素，返回切片并前进。
        /// </summary>
        /// <param name="count">需要读取的元素数量。</param>
        /// <param name="value">输出读取到的只读切片。</param>
        /// <returns>若剩余长度不足 count，返回 false 且不前进。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead(int count, out ReadOnlySequence<T> value)
        {
            UpdateReader();
            if (count < 0 || reader.Length < count)
            {
                value = default;
                return false;
            }
            var start = reader.Position;
            reader.Advance(count);
            var end = reader.Position;
            value = reader.Sequence.Slice(start, end);
            return true;
        }

        /// <summary>
        /// 读取数据到目标缓冲区，同时前进读取位置。
        /// </summary>
        /// <param name="value">目标缓冲区</param>
        /// <returns>读取数量</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<T> value)
        {
            if (value.IsEmpty || value.Length == 0)
                return 0;

            UpdateReader();
            int totalRead = 0;
            int index = 0;
            foreach (var segment in Sequence.Slice(CurrentSpanIndex))
            {
                int minLength = Math.Min(segment.Length, value.Length - totalRead);

                segment.Slice(0, minLength).Span.CopyTo(value.Slice(index));
                index += minLength;
            }
            ReadAdvance(totalRead);
            return totalRead;
        }

        /// <summary>
        /// 读取数据到目标缓冲区，同时前进读取位置。
        /// </summary>
        /// <param name="value">目标缓冲区</param>
        /// <returns>读取数量</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Memory<T> value)
        {
            if (value.IsEmpty || value.Length == 0)
                return 0;

            UpdateReader();
            int totalRead = 0;
            int index = 0;
            foreach (var segment in Sequence.Slice(CurrentSpanIndex))
            {
                int minLength = Math.Min(segment.Length, value.Length - totalRead);

                segment.Slice(0, minLength).CopyTo(value.Slice(index));
                index += minLength;
            }
            ReadAdvance(totalRead);
            return totalRead;
        }

        /// <summary>
        /// 尝试将剩余数据复制到目标缓冲区（不改变读取位置）。
        /// </summary>
        /// <param name="span">目标缓冲区。</param>
        /// <returns>复制成功返回 true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(scoped Span<T> span)
        {
            UpdateReader();
            return reader.TryCopyTo(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(MemoryBlock<T> block)
        {
            if (block.IsEmpty)
                return false;

            block.Ensure((int)Length);
            while (!End)
            {
                var span = UnreadSpan;
                block.Write(span);
                ReadAdvance(span.Length);
            }
            return true;
        }

        /// <summary>
        /// 尝试预览一个元素（不前进）。
        /// </summary>
        /// <param name="value">输出预览到的元素。</param>
        /// <returns>预览成功返回 true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T value)
        {
            UpdateReader();
            return reader.TryPeek(out value);
        }

        /// <summary>
        /// 尝试在偏移量处预览一个元素（不前进）。
        /// </summary>
        /// <param name="offset">相对当前位置的偏移量。</param>
        /// <param name="value">输出预览到的元素。</param>
        /// <returns>预览成功返回 true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(long offset, out T value)
        {
            UpdateReader();
            return reader.TryPeek(offset, out value);
        }

        /// <summary>
        /// 将读取位置定位到指定位置。
        /// </summary>
        /// <param name="pos">指定位置</param>
        /// <exception cref="ArgumentOutOfRangeException">当位置小于0或大于已写入长度时触发</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Seek(long pos)
        {
            if (pos < 0 || pos > Length)
                throw new ArgumentOutOfRangeException(nameof(pos), "位置超出范围。");
            long lastPos = Consumed;
            long diff = lastPos - pos;
            if (diff == 0)
                return;

            if (diff < 0)
            {
                Rewind(Math.Abs(diff));
            }
            else
            {
                ReadAdvance(diff);
            }
        }

        /// <summary>
        /// 将读取位置回退指定数量。
        /// </summary>
        /// <param name="count">回退的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rewind(long count)
        {
            UpdateReader();
            reader.Rewind(count);
        }

        /// <summary>
        /// 获取指向当前可写缓冲区起始位置的引用。 等价于 <see cref="GetSpan(int)"/> 后调用 <see cref="Span{T}.GetPinnableReference"/>， 便于通过 <c>ref</c> 方式直接写入，再配合 <see
        /// cref="WriteAdvance(int)"/> 提交已写入的元素数。
        /// </summary>
        /// <param name="sizeHint">期望的最小连续容量（提示值，允许为 0）。</param>
        /// <returns>返回可写缓冲区第一个元素的引用。</returns>
        /// <exception cref="ObjectDisposedException">当未持有可写序列或序列已释放时抛出。</exception>
        /// <remarks>
        /// 使用说明：
        /// - 返回的引用仅在下一次申请写缓冲（如调用 <see cref="GetSpan(int)"/>、 <see cref="GetMemory(int)"/>、 <see cref="Write(in T)"/> 等） 或推进（ <see cref="WriteAdvance(int)"/>）之前有效；请勿缓存或跨越上述调用后继续使用。
        /// - 引用本身未固定（未 pin）；如需与非托管代码交互并要求固定，请在 <c>fixed</c> 语句中使用。
        /// - 写入完成后务必调用 <see cref="WriteAdvance(int)"/> 通知实际写入的元素数量。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetPointer(int sizeHint = 0)
        {
            return ref GetSpan(sizeHint).GetPinnableReference();
        }

        /// <summary>
        /// 将已写入的内容导出为新数组。
        /// </summary>
        /// <returns>包含当前内容的数组。</returns>
        public T[] ToArray()
        {
            if (IsEmpty)
                return Array.Empty<T>();

            UpdateReader();
            var array = new T[(int)Length];
            long consumed = Consumed;
            reader.Rewind(consumed);
            TryCopyTo(array);
            reader.Advance(consumed);
            return array;
        }

        /// <summary>
        /// 创建一个用于“窥视”的副本。 注意：该方法返回的是当前实例的按值副本，用于只读预览；请勿对副本调用 <see cref="Dispose"/> 以避免重复归还租约。
        /// </summary>
        public SequenceBuffer<T> CreatePeekBuffer()
        {
            UpdateReader();
            return reader;
        }

        /// <summary>
        /// 获取当前序列的只读内存列表视图。
        /// </summary>
        /// <returns>只读内存列表</returns>
        public IReadOnlyList<ReadOnlyMemory<T>> ToReadOnlyList()
            => new ReadOnlyList<T>(this);

        /// <summary>
        /// 释放持有的序列资源（若有）。
        /// </summary>
        public void Dispose()
        {
            if (!Rental.IsEmpty)
            {
                Rental.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfCannotWrite()
        {
            if (!CanWrite)
                throw new ObjectDisposedException("当前缓冲不可写入。");
        }

        #region FromSequenceBuffer

        public static implicit operator ReadOnlySequence<T>(in SequenceBuffer<T> buffer)
            => buffer.Sequence;

        public static implicit operator SequenceReader<T>(in SequenceBuffer<T> buffer)
        {
            buffer.UpdateReader();
            return buffer.reader;
        }

        public class ReadOnlyList<TValue> : List<ReadOnlyMemory<TValue>>, IReadOnlyList<ReadOnlyMemory<TValue>>
            where TValue : unmanaged, IEquatable<TValue>
        {
            public ReadOnlyList(in SequenceBuffer<TValue> buffer)
            {
                foreach (var segment in buffer.Sequence)
                {
                    Add(segment);
                }
            }
        }

        #endregion FromSequenceBuffer

        #region ToSequenceBuffer

        public static implicit operator SequenceBuffer<T>(ReadOnlySequence<T> sequence)
            => new SequenceBuffer<T>(sequence);

        public static implicit operator SequenceBuffer<T>(ReadOnlyMemory<T> memory)
            => new SequenceBuffer<T>(memory);

        public static implicit operator SequenceBuffer<T>(MemoryBlock<T> block)
            => new SequenceBuffer<T>(block);

        public static implicit operator SequenceBuffer<T>(SequenceReader<T> reader)
            => new SequenceBuffer<T>(reader);

        #endregion ToSequenceBuffer
    }
}