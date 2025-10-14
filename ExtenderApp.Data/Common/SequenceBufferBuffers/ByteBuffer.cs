using System.Buffers;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 面向 byte 的顺序写入 + 顺序读取封装，基于 <see cref="SequenceBuffer{T}"/> 实现的轻量适配器。
    /// - 提供写缓冲（<see cref="GetSpan"/> / <see cref="GetMemory"/> / <see cref="Write(in byte)"/> 等）；
    /// - 提供只读顺序读取（<see cref="TryRead(out byte)"/> / <see cref="TryRead(int, out ReadOnlySequence{byte})"/> / <see cref="TryPeek(out byte)"/> 等）；
    /// - 写入后下次读取会自动基于最新快照。
    /// 注意：
    /// 1) 本类型为 ref struct，不可装箱、不可捕获到闭包、不可跨异步、不可存入堆结构；
    /// 2) 非线程安全：同一实例请勿在多线程并发读写；
    /// 3) 生命周期结束请调用 <see cref="Dispose"/> 释放底层租约（若由池构造）。
    /// </summary>
    public ref struct ByteBuffer
    {
        /// <summary>
        /// 内部泛型块实现，封装实际的写入/读取逻辑。
        /// </summary>
        private SequenceBuffer<byte> _buffer;

        /// <summary>
        /// 当前绑定的只读序列快照。
        /// </summary>
        public ReadOnlySequence<byte> Sequence => _buffer.Sequence;

        /// <summary>
        /// 获得当前数据片段的索引。
        /// </summary>
        public int CurrentSpanIndex => _buffer.CurrentSpanIndex;

        /// <summary>
        /// 序列总长度。
        /// </summary>
        public long Length => _buffer.Length;

        /// <summary>
        /// 剩余未读取的元素数量。
        /// </summary>
        public long Remaining => _buffer.Remaining;

        /// <summary>
        /// 是否已经到达序列末尾。
        /// </summary>
        public bool End => _buffer.End;

        /// <summary>
        /// 已消耗（读取）的元素数量。
        /// </summary>
        public long Consumed => _buffer.Consumed;

        /// <summary>
        /// 当前读取位置。
        /// </summary>
        public SequencePosition Position => _buffer.Position;

        /// <summary>
        /// 当前数据片段的只读跨度。
        /// </summary>
        public ReadOnlySpan<byte> CurrentSpan => _buffer.CurrentSpan;

        /// <summary>
        /// 当前数据片段中尚未读取的只读跨度。
        /// </summary>
        public ReadOnlySpan<byte> UnreadSpan => _buffer.UnreadSpan;

        /// <summary>
        /// 获取下一个元素（不前进），若无数据则抛出 <see cref="System.IO.EndOfStreamException"/>。
        /// </summary>
        public byte NextCode
        {
            get
            {
                if (!_buffer.TryPeek(out byte code))
                {
                    throw new EndOfStreamException();
                }
                return code;
            }
        }

        /// <summary>
        /// 获得当前是否可写：当持有可写序列且未释放时为 true。
        /// </summary>
        public bool CanWrite => _buffer.CanWrite;

        /// <summary>
        /// 是否为空：当未持有可写序列且读取器中无数据时为 true。
        /// </summary>
        public bool IsEmpty => _buffer.IsEmpty;

        /// <summary>
        /// 通过已有的 <see cref="ByteBuffer"/> 构造（拷贝构造）。
        /// </summary>
        /// <param name="other">已有的字节缓冲</param>
        public ByteBuffer(in ByteBuffer other) : this(other._buffer)
        {
        }

        /// <summary>
        /// 通过已有的 <see cref="SequenceBuffer{T}"/> 构造。
        /// </summary>
        /// <param name="buffer">已有的缓冲</param>
        public ByteBuffer(in SequenceBuffer<byte> buffer)
        {
            _buffer = buffer;
        }

        /// <summary>
        /// 通过序列池构造并获取一个可写序列的租约。
        /// </summary>
        /// <param name="pool">序列池。</param>
        /// <remarks>生命周期结束时调用 <see cref="Dispose"/> 归还租约。</remarks>
        public ByteBuffer(SequencePool<byte> pool)
        {
            _buffer = new SequenceBuffer<byte>(pool);
        }

        /// <summary>
        /// 使用可写的 <see cref="Sequence{T}"/> 构造，支持后续写入。
        /// </summary>
        /// <param name="sequence">可写序列。</param>
        public ByteBuffer(Sequence<byte> sequence)
        {
            _buffer = new SequenceBuffer<byte>(sequence);
        }

        /// <summary>
        /// 使用给定的只读内存构造，对应一个单段序列（只读）。
        /// </summary>
        /// <param name="memory">只读内存。</param>
        public ByteBuffer(ReadOnlyMemory<byte> memory)
        {
            _buffer = new SequenceBuffer<byte>(memory);
        }

        /// <summary>
        /// 使用只读序列构造，无法进行写入，仅能读取。
        /// </summary>
        /// <param name="readSequence">只读序列快照。</param>
        public ByteBuffer(ReadOnlySequence<byte> readSequence)
        {
            _buffer = new SequenceBuffer<byte>(readSequence);
        }

        /// <summary>
        /// 申请一个可写的 <see cref="Span{T}"/>，用于直接写入。
        /// 申请写缓冲后会使读取视图变脏，下一次读取将刷新。
        /// </summary>
        /// <param name="sizeHint">期望大小（提示值，可为 0）。</param>
        /// <exception cref="ObjectDisposedException">当未持有可写序列时抛出。</exception>
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return _buffer.GetSpan(sizeHint);
        }

        /// <summary>
        /// 申请一个可写的 <see cref="Memory{T}"/>，用于异步/延迟写入。
        /// 申请写缓冲后会使读取视图变脏，下一次读取将刷新。
        /// </summary>
        /// <param name="sizeHint">期望大小（提示值，可为 0）。</param>
        /// <exception cref="ObjectDisposedException">当未持有可写序列时抛出。</exception>
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            return _buffer.GetMemory(sizeHint);
        }

        /// <summary>
        /// 追加单个元素。
        /// </summary>
        /// <param name="value">要追加的元素。</param>
        public void Write(in byte value)
        {
            _buffer.Write(value);
        }

        /// <summary>
        /// 追加一段只读跨度数据。
        /// </summary>
        /// <param name="value">要追加的数据。</param>
        public void Write(in ReadOnlySpan<byte> value)
        {
            _buffer.Write(value);
        }

        /// <summary>
        /// 追加一段只读内存数据。
        /// </summary>
        /// <param name="value">要追加的数据。</param>
        public void Write(in ReadOnlyMemory<byte> value)
        {
            _buffer.Write(value);
        }

        /// <summary>
        /// 追加一段只读序列数据。
        /// </summary>
        /// <param name="value">要追加的数据。</param>
        public void Write(in ReadOnlySequence<byte> value)
        {
            _buffer.Write(value);
        }

        /// <summary>
        /// 从当前位置尝试读取一个元素并前进。
        /// </summary>
        /// <param name="value">输出读取到的元素。</param>
        /// <returns>读取成功返回 true，否则 false。</returns>
        public bool TryRead(out byte value)
        {
            return _buffer.TryRead(out value);
        }

        /// <summary>
        /// 从当前位置尝试读取 count 个元素，返回切片并前进。
        /// </summary>
        /// <param name="count">需要读取的元素数量。</param>
        /// <param name="value">输出读取到的只读切片。</param>
        /// <returns>若剩余长度不足 count，返回 false 且不前进。</returns>
        public bool TryRead(int count, out ReadOnlySequence<byte> value)
        {
            return _buffer.TryRead(count, out value);
        }

        /// <summary>
        /// 尝试将剩余数据复制到目标缓冲区（不改变读取位置）。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <returns>复制成功返回 true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(scoped Span<byte> buffer)
        {
            return _buffer.TryCopyTo(buffer);
        }

        /// <summary>
        /// 尝试预览一个元素（不前进）。
        /// </summary>
        /// <param name="value">输出预览到的元素。</param>
        /// <returns>预览成功返回 true。</returns>
        public bool TryPeek(out byte value)
        {
            return _buffer.TryPeek(out value);
        }

        /// <summary>
        /// 尝试在偏移量处预览一个元素（不前进）。
        /// </summary>
        /// <param name="offset">相对当前位置的偏移量。</param>
        /// <param name="value">输出预览到的元素。</param>
        /// <returns>预览成功返回 true。</returns>
        public bool TryPeek(long offset, out byte value)
        {
            return _buffer.TryPeek(offset, out value);
        }

        /// <summary>
        /// 将读取位置定位到指定位置。
        /// </summary>
        /// <param name="pos">指定位置</param>
        /// <exception cref="ArgumentOutOfRangeException">当位置小于0或大于已写入长度时触发</exception>
        public void Seek(long pos)
        {
            _buffer.Seek(pos);
        }

        /// <summary>
        /// 将读取位置回退指定数量。
        /// </summary>
        /// <param name="count">回退的元素数量。</param>
        public void Rewind(long count)
        {
            _buffer.Rewind(count);
        }

        /// <summary>
        /// 提交此前通过 <see cref="GetSpan(int)"/> 或 <see cref="GetMemory(int)"/> 获取的写缓冲中已写入的字节数，
        /// 前进写入位置并使读取快照失效。
        /// </summary>
        /// <param name="count">已写入且需要提交的字节数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteAdvance(int count)
        {
            _buffer.WriteAdvance(count);
        }

        /// <summary>
        /// 将读取位置向前移动指定的字节数，相当于跳过这些字节。
        /// </summary>
        /// <param name="count">要跳过的字节数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadAdvance(long count)
        {
            _buffer.ReadAdvance(count);
        }

        /// <summary>
        /// 尝试将读取位置向前移动指定的字节数。
        /// </summary>
        /// <param name="count">要前进的字节数量。</param>
        /// <returns>若剩余长度不足 <paramref name="count"/>，则不移动并返回 false；否则前进并返回 true。</returns>
        public bool TryReadAdvance(long count)
        {
            return _buffer.TryReadAdvance(count);
        }

        /// <summary>
        /// 获取指向当前可写缓冲区起始位置的引用。
        /// 等价于 <see cref="GetSpan(int)"/> 后调用 <see cref="Span{byte}.GetPinnableReference"/>，
        /// 便于通过 <c>ref</c> 方式直接写入，再配合 <see cref="WriteAdvance(int)"/> 提交已写入的元素数。
        /// </summary>
        /// <param name="sizeHint">期望的最小连续容量（提示值，允许为 0）。</param>
        /// <returns>返回可写缓冲区第一个元素的引用。</returns>
        /// <exception cref="ObjectDisposedException">当未持有可写序列或序列已释放时抛出。</exception>
        /// <remarks>
        /// 使用说明：
        /// - 返回的引用仅在下一次申请写缓冲（如调用 <see cref="GetSpan(int)"/>、<see cref="GetMemory(int)"/>、<see cref="Write(in byte)"/> 等）
        ///   或推进（<see cref="WriteAdvance(int)"/>）之前有效；请勿缓存或跨越上述调用后继续使用。
        /// - 引用本身未固定（未 pin）；如需与非托管代码交互并要求固定，请在 <c>fixed</c> 语句中使用。
        /// - 写入完成后务必调用 <see cref="WriteAdvance(int)"/> 通知实际写入的元素数量。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref byte GetPointer(int sizeHint = 0)
        {
            return ref GetSpan(sizeHint).GetPinnableReference();
        }

        /// <summary>
        /// 将已写入的内容导出为字节数组。
        /// </summary>
        /// <returns>包含当前内容的字节数组。</returns>
        public byte[] ToArray()
        {
            return _buffer.ToArray();
        }

        /// <summary>
        /// 创建一个用于“窥视”的副本。
        /// 注意：返回的是当前实例的按值副本，用于只读预览；请勿对副本调用 <see cref="Dispose"/> 以避免重复释放。
        /// </summary>
        public ByteBuffer CreatePeekBuffer() => this;

        /// <summary>
        /// 获取当前序列的只读内存列表视图。
        /// </summary>
        /// <returns>只读内存列表</returns>
        public IReadOnlyList<ReadOnlyMemory<byte>> ToReadOnlyList() => _buffer.ToReadOnlyList();

        /// <summary>
        /// 释放底层序列资源（若有）。
        /// </summary>
        public void Dispose()
        {
            _buffer.Dispose();
        }

        public static implicit operator ReadOnlySequence<byte>(in ByteBuffer buffer)
            => buffer.Sequence;

        public static implicit operator SequenceReader<byte>(in ByteBuffer buffer)
            => buffer._buffer;
    }
}
