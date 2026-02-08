using System.Buffers;

namespace ExtenderApp.Data.Buffer
{
    /// <summary>
    /// 对单个 <see cref="MemoryBlock{T}"/> 的只向前读取器（轻量结构），维护已消费的偏移并提供便捷读取方法。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    public struct MemoryBlockReader<T> : IDisposable, IEquatable<MemoryBlockReader<T>> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        /// 内存块实例，读取器将从该块的已提交区域开始读取。读取器不拥有块的生命周期，但会尝试在释放时调用块的释放方法以协助资源管理。
        /// </summary>
        internal readonly MemoryBlock<T> Block;

        /// <summary>
        /// 当前已消费（已读取）的元素数量（相对于块的已提交区域起点）。
        /// </summary>
        public int Consumed { get; private set; }

        /// <summary>
        /// 构造一个读取器以读取指定的内存块，初始已消费位置为 0。
        /// </summary>
        /// <param name="block">要读取的 <see cref="MemoryBlock{T}"/>，不能为空。</param>
        public MemoryBlockReader(MemoryBlock<T> block)
        {
            Block = block ?? throw new ArgumentNullException(nameof(block));
            Block.Freeze(); // 确保块不可变以安全读取
            Consumed = 0;
        }

        /// <summary>
        /// 剩余可读的元素数量（等于块的已提交长度减去当前已消费数）。
        /// </summary>
        public int Remaining => (int)(Block.Committed - Consumed);

        /// <summary>
        /// 指示读取器是否已读尽当前块的已提交数据。
        /// </summary>
        public bool IsCompleted => Remaining == 0;

        /// <summary>
        /// 返回当前未读的只读跨度（从当前位置到已提交末尾）。
        /// </summary>
        public ReadOnlySpan<T> CommittedSpan => Block.CommittedSpan.Slice(Consumed);

        /// <summary>
        /// 返回当前未读的只读内存（从当前位置到已提交末尾）。
        /// </summary>
        public ReadOnlyMemory<T> UnreadMemory => Block.CommittedMemory.Slice(Consumed);

        /// <summary>
        /// 获取当前未读的数组段（从当前位置到已提交末尾）。
        /// </summary>
        public ArraySegment<T> UnreadSegment => Block.CommittedSegment.Slice(Consumed);

        /// <summary>
        /// 尝试预览下一个元素而不推进读取位置。
        /// </summary>
        /// <param name="item">输出的元素（当返回 true 时有效）。</param>
        /// <returns>若存在下一个元素则返回 true，否则返回 false。</returns>
        public bool TryPeek(out T item)
        {
            if (Remaining <= 0)
            {
                item = default!;
                return false;
            }

            item = CommittedSpan[0];
            return true;
        }

        /// <summary>
        /// 尝试读取下一个元素并推进位置。
        /// </summary>
        /// <param name="item">读取到的元素（当返回 true 时有效）。</param>
        /// <returns>如成功读取返回 true，否则返回 false（例如已无数据）。</returns>
        public bool TryRead(out T item)
        {
            if (!TryPeek(out item))
                return false;

            Advance(1);
            return true;
        }

        /// <summary>
        /// 尝试读取并复制指定长度到目标跨度，如果目标长度大于剩余则返回 false，不推进位置。
        /// </summary>
        /// <param name="destination">目标跨度，用于接收数据。</param>
        /// <returns>当成功复制并推进位置时返回 true；若剩余不足返回 false 且不改变状态。</returns>
        public bool TryRead(Span<T> destination)
        {
            if (destination.Length == 0)
                return true;

            if (Remaining < destination.Length)
                return false;

            CommittedSpan.Slice(0, destination.Length).CopyTo(destination);
            Advance(destination.Length);
            return true;
        }

        /// <summary>
        /// 读取下一个元素并推进位置，若无数据则抛出异常。
        /// </summary>
        /// <returns>读取到的元素。</returns>
        /// <exception cref="InvalidOperationException">当没有更多数据可读时抛出。</exception>
        public T Read()
        {
            T result = CommittedSpan[0];
            Advance(1);
            return result;
        }

        /// <summary>
        /// 将尽可能多的数据复制到目标跨度并推进相应的读取位置，返回实际复制的元素数量。
        /// </summary>
        /// <param name="destination">目标跨度。</param>
        /// <returns>实际复制并消费的元素数量（可能小于目标长度）。</returns>
        public int Read(Span<T> destination)
        {
            if (destination.Length == 0)
                return 0;

            int toCopy = Math.Min(Remaining, destination.Length);
            if (toCopy == 0)
                return 0;

            CommittedSpan.Slice(0, toCopy).CopyTo(destination);
            Advance(toCopy);
            return toCopy;
        }

        /// <summary>
        /// 将尽可能多的数据复制到目标 <see cref="Memory{T}"/> 并推进读取位置，返回实际复制的元素数量。
        /// </summary>
        /// <param name="destination">目标内存，用于接收数据。</param>
        /// <returns>实际复制并消费的元素数量。</returns>
        public int Read(Memory<T> destination)
        {
            return Read(destination.Span);
        }

        /// <summary>
        /// 将尽可能多的数据复制到目标 <see cref="ArraySegment{T}"/> 并推进读取位置，返回实际复制的元素数量。
        /// </summary>
        /// <param name="destination">目标数组段，用于接收数据。</param>
        /// <returns>实际复制并消费的元素数量。</returns>
        public int Read(ArraySegment<T> destination)
        {
            return Read(destination.AsSpan());
        }

        //public int Read(BufferBase<T> destination)
        //{
        //    if (destination == null)
        //        throw new ArgumentNullException(nameof(destination));
        //    int toCopy = Math.Min(Remaining, destination.Available);
        //    if (toCopy == 0)
        //        return 0;

        //    CommittedSpan.Slice(0, toCopy).CopyTo(destination.GetMemory(toCopy).Span);
        //    destination.Advance(toCopy);
        //    Advance(toCopy);
        //    return toCopy;
        //}

        /// <summary>
        /// 将读取位置向前推进指定数量的元素。
        /// </summary>
        /// <param name="count">推进的元素数量（必须为非负且不超过剩余）。</param>
        public void Advance(int count)
        {
            if (count < 0 || Consumed + count > Block.Committed)
                throw new ArgumentOutOfRangeException(nameof(count));

            Consumed += count;
        }

        /// <summary>
        /// 将读取位置回退指定数量的元素。
        /// </summary>
        /// <param name="count">回退的元素数量（必须为非负且不超过当前已消费）。</param>
        public void Rewind(int count)
        {
            if (count < 0 || Consumed - count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            Consumed -= count;
        }

        /// <summary>
        /// 将读取器重置为初始状态（已消费位置设为 0）。
        /// </summary>
        public void Reset()
        {
            Consumed = 0;
        }

        public bool Equals(MemoryBlockReader<T> other)
        {
            return Block.Equals(other.Block) && Consumed == other.Consumed;
        }

        public static bool operator ==(MemoryBlockReader<T> left, MemoryBlockReader<T> right)
            => left.Equals(right);

        public static bool operator !=(MemoryBlockReader<T> left, MemoryBlockReader<T> right)
            => !left.Equals(right);

        public override bool Equals(object? obj)
            => obj is MemoryBlockReader<T> other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Block, Consumed);

        public override string ToString()
            => $"MemoryBlockReader(Committed={Consumed}, Remaining={Remaining}, IsCompleted={IsCompleted})";

        public void Dispose()
        {
            Block.TryRelease();
        }

        public static implicit operator MemoryBlock<T>(MemoryBlockReader<T> reader)
            => reader.Block;

        public static implicit operator ReadOnlySpan<T>(MemoryBlockReader<T> reader)
            => reader.CommittedSpan;

        public static implicit operator ReadOnlyMemory<T>(MemoryBlockReader<T> reader)
            => reader.UnreadMemory;

        public static implicit operator ArraySegment<T>(MemoryBlockReader<T> reader)
            => reader.UnreadSegment;

        public static implicit operator ReadOnlySequence<T>(MemoryBlockReader<T> reader)
            => new ReadOnlySequence<T>(reader.UnreadMemory);

        public static implicit operator SequenceReader<T>(MemoryBlockReader<T> reader)
            => new SequenceReader<T>(reader);

        public static implicit operator MemoryBlockReader<T>(MemoryBlock<T> block)
            => new MemoryBlockReader<T>(block);
    }
}