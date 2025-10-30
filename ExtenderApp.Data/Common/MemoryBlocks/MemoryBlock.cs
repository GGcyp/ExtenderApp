using System.Buffers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 使用 ArrayPool&lt;T&gt; 管理的可增长顺序缓冲。
    /// - Length 表示已写入元素的数量（写指针/写边界）。
    /// - Consumed 表示当前读取位置（读指针）。
    /// 线程不安全；需要在使用完毕后调用 <see cref="Dispose"/> 归还数组到池。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    public struct MemoryBlock<T>
    {
        private const int DefaultCapacity = 4 * 1024;

        /// <summary>
        /// 内存块使用的数组池。
        /// </summary>
        private readonly ArrayPool<T> _pool;

        /// <summary>
        /// 内存块的底层数组。
        /// </summary>
        private T[]? array;

        /// <summary>
        /// 已写入元素的数量（写指针/写边界）。
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// 当前读取位置（读指针）。
        /// </summary>
        public int Consumed { get; private set; }

        /// <summary>
        /// 剩余未读数据的数量（Length - Consumed）。
        /// </summary>
        public int Remaining => Length - Consumed;

        /// <summary>
        /// 当前底层缓冲容量。array 为空时为 0。
        /// </summary>
        public int Capacity => array?.Length ?? 0;

        /// <summary>
        /// 是否无任何已写入数据（Length == 0）。
        /// </summary>
        public bool IsEmpty => Length == 0;

        /// <summary>
        /// 获取已写入范围的只读 UnreadSpan。
        /// </summary>
        public ReadOnlySpan<T> UnreadSpan => array.AsSpan(Consumed, (int)Remaining);

        /// <summary>
        /// 获取已写入范围的只读 UnreadMemory。
        /// </summary>
        public ReadOnlyMemory<T> UnreadMemory => array.AsMemory(Consumed, (int)Remaining);

        public MemoryBlock()
        {
            _pool = ArrayPool<T>.Shared;
        }

        /// <summary>
        /// 按指定容量租用缓冲。
        /// </summary>
        /// <param name="capacity">初始容量。</param>
        public MemoryBlock(int capacity) : this(capacity, ArrayPool<T>.Shared)
        {
        }

        /// <summary>
        /// 按指定容量与池租用缓冲。
        /// </summary>
        /// <param name="capacity">初始容量。</param>
        /// <param name="pool">数组池。</param>
        public MemoryBlock(int capacity, ArrayPool<T> pool)
        {
            if (pool is null)
                throw new ArgumentNullException(nameof(pool));

            _pool = pool;
            if (capacity > 0)
            {
                capacity = capacity < DefaultCapacity ? DefaultCapacity : capacity;
                array = _pool.Rent(capacity);
            }
            else
                array = array = Array.Empty<T>();

            Consumed = 0;
            Length = 0;
        }

        /// <summary>
        /// 以内存内容初始化。
        /// </summary>
        /// <param name="memory">初始数据。</param>
        public MemoryBlock(ReadOnlyMemory<T> memory) : this(memory, ArrayPool<T>.Shared)
        {
        }

        /// <summary>
        /// 以内存内容初始化，并使用指定池。
        /// </summary>
        /// <param name="memory">初始数据。</param>
        /// <param name="pool">数组池。</param>
        public MemoryBlock(ReadOnlyMemory<T> memory, ArrayPool<T> pool) : this(memory.Length, pool)
        {
            if (memory.IsEmpty)
                throw new ArgumentNullException(nameof(memory));

            memory.CopyTo(array);
            Length = memory.Length;
        }

        /// <summary>
        /// 以 UnreadSpan 内容初始化。
        /// </summary>
        /// <param name="span">初始数据。</param>
        public MemoryBlock(ReadOnlySpan<T> span) : this(span, ArrayPool<T>.Shared)
        {
        }

        /// <summary>
        /// 以 UnreadSpan 内容初始化，并使用指定池。
        /// </summary>
        /// <param name="span">初始数据。</param>
        /// <param name="pool">数组池。</param>
        public MemoryBlock(ReadOnlySpan<T> span, ArrayPool<T> pool) : this(span.Length, pool)
        {
            if (span.IsEmpty)
                throw new ArgumentNullException(nameof(span));

            span.CopyTo(array);
            Length = span.Length;
        }

        /// <summary>
        /// 复制构造：基于另一个块的内容创建新实例（租用同等容量并拷贝已写入数据）。
        /// </summary>
        /// <param name="block">源块（仅拷贝已写入范围 0..Length）。</param>
        public MemoryBlock(in MemoryBlock<T> block)
        {
            if (block.IsEmpty)
                throw new ArgumentNullException(nameof(block));

            _pool = block._pool;
            Consumed = block.Consumed;
            Length = block.Length;
            array = _pool.Rent(block.Capacity);
            Array.Copy(block.array!, array, Length);
        }

        /// <summary>
        /// 获取用于写入的可写内存切片。切片起点为当前 <see cref="Length"/>。
        /// </summary>
        /// <param name="sizeHint">预计需要写入的最小容量提示，用于触发扩容。</param>
        /// <returns>从当前写入位置开始的 <see cref="Memory{T}"/> 切片。</returns>
        public Memory<T> GetMemory(int sizeHint = 0)
        {
            Ensure(sizeHint);
            return array.AsMemory(Length);
        }

        /// <summary>
        /// 获取用于写入的可写 UnreadSpan。切片起点为当前 <see cref="Length"/>。
        /// </summary>
        /// <param name="sizeHint">预计需要写入的最小容量提示，用于触发扩容。</param>
        /// <returns>从当前写入位置开始的 <see cref="Span{T}"/> 切片。</returns>
        public Span<T> GetSpan(int sizeHint = 0)
        {
            Ensure(sizeHint);
            return array.AsSpan(Length);
        }

        /// <summary>
        /// 将写指针向前推进指定数量（调用者保证已写入该数量）。
        /// </summary>
        /// <param name="count">推进的元素个数。</param>
        public void WriteAdvance(int count)
        {
            Length += count;
        }

        /// <summary>
        /// 将读指针向前推进指定数量。
        /// </summary>
        /// <param name="count">推进的元素个数。</param>
        public void ReadAdvance(int count)
        {
            Consumed += count;
        }

        /// <summary>
        /// 写入单个元素到当前写指针位置。
        /// </summary>
        /// <param name="item">要写入的元素。</param>
        public void Write(T item)
        {
            Ensure(1);
            array[Length] = item;
            WriteAdvance(1);
        }

        /// <summary>
        /// 写入一段连续数据到当前写指针位置。
        /// </summary>
        /// <param name="span">要写入的数据切片。</param>
        public void Write(ReadOnlySpan<T> span)
        {
            Ensure(span.Length);
            span.CopyTo(array.AsSpan(Length));
            WriteAdvance(span.Length);
        }

        /// <summary>
        /// 写入一段连续数据到当前写指针位置。
        /// </summary>
        /// <param name="memory">要写入的数据内存。</param>
        public void Write(ReadOnlyMemory<T> memory)
        {
            if (memory.IsEmpty || memory.Length == 0)
                return;

            Write(memory.Span);
        }

        /// <summary>
        /// 写入一个只读序列（可能由多段组成）。
        /// </summary>
        /// <param name="value">只读序列。</param>
        public void Write(in ReadOnlySequence<T> value)
        {
            if (value.IsEmpty)
                return;

            Ensure((int)value.Length);
            foreach (var segment in value)
            {
                segment.Span.CopyTo(array.AsSpan(Length));
                WriteAdvance(segment.Length);
            }
        }

        /// <summary>
        /// 将可读数据（Length - Consumed）复制到目标 UnreadSpan。
        /// </summary>
        /// <param name="destination">目标缓冲</param>
        /// <returns>若目标有足够空间则返回 true。</returns>
        public bool TryCopyTo(Span<T> destination)
        {
            return TryCopyTo(destination, out _);
        }

        /// <summary>
        /// 将可读数据（Length - Consumed）复制到目标 UnreadSpan。
        /// </summary>
        /// <param name="destination">目标缓冲。</param>
        /// <param name="written">实际写入的元素数量。</param>
        /// <returns>若目标有足够空间则返回 true。</returns>
        public bool TryCopyTo(Span<T> destination, out int written)
        {
            int length = Length - Consumed;
            if (length <= destination.Length)
            {
                array.AsSpan(Consumed, length).CopyTo(destination);
                written = length;
                return true;
            }
            written = 0;
            return false;
        }

        /// <summary>
        /// 查看当前读指针位置的元素而不移动指针。
        /// </summary>
        /// <param name="value">输出元素。</param>
        /// <returns>若存在可读元素则返回 true。</returns>
        public bool TryPeek(out T value)
        {
            int pos = Consumed;
            value = default;
            if (pos >= Length)
            {
                return false;
            }

            value = array[pos];
            return true;
        }

        /// <summary>
        /// 查看相对当前读指针 <paramref name="offset"/> 偏移处的元素（不移动指针）。
        /// </summary>
        /// <param name="offset">相对偏移量（从当前 Consumed 开始计算，需 ≥ 0）。</param>
        /// <param name="value">输出元素。</param>
        /// <returns>若存在可读元素则返回 true。</returns>
        public bool TryPeek(long offset, out T value)
        {
            value = default;
            if (offset < 0)
            {
                return false;
            }

            long index = (long)Consumed + offset;
            if (index >= Length)
            {
                return false;
            }

            value = array[(int)index];
            return true;
        }

        /// <summary>
        /// 确保至少还能写入 <paramref name="sizeHint"/> 个元素；不足则扩容。
        /// </summary>
        /// <param name="sizeHint">需要预留的最小写入空间。</param>
        public void Ensure(int sizeHint)
        {
            int newCapacity = Length + sizeHint;
            if (newCapacity <= Capacity)
            {
                return;
            }

            var oldArray = array;
            array = _pool.Rent(newCapacity);
            if (Length > 0)
            {
                Array.Copy(oldArray, 0, array, 0, Length);
            }
            _pool.Return(oldArray);
        }

        /// <summary>
        /// 将读指针设置为绝对位置 <paramref name="count"/>。范围：0..Length。
        /// </summary>
        /// <param name="count">新的读指针位置。</param>
        /// <exception cref="ArgumentOutOfRangeException">当位置不在 0..Length 范围内时抛出。</exception>
        public void Rewind(int count)
        {
            if (count < 0 || count > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count), string.Format("超过已写入范围限制：{0}", count));
            }

            Consumed = count;
        }

        /// <summary>
        /// 重置读/写指针为起点（不清零数据）。
        /// </summary>
        public void Reset()
        {
            Consumed = 0;
            Length = 0;
        }

        /// <summary>
        /// 清零已使用区域的数据（不改变读/写指针）。
        /// </summary>
        public void Clear()
        {
            if (Length > 0)
            {
                Array.Clear(array, 0, Length);
            }
        }

        /// <summary>
        /// 将未消费的数据移动到缓冲起始处并重置读写指针。
        /// - 场景：当 Consumed > 0 且还有剩余未读数据时，调用此方法把 unread 数据拷贝到 array[0..Remaining-1]，
        ///   以便回收前面的已消费区域用于后续写入。
        /// - 若所有数据都已消费，则行为等同于 Reset()。
        /// </summary>
        public void Compact()
        {
            // 如果没有底层数组或没有已写数据，直接返回
            if (array == null || Length == 0 || Consumed == 0)
                return;

            int unread = Length - Consumed;
            if (unread <= 0)
            {
                // 全部消费，重置指针以重用整个缓冲
                Reset();
                return;
            }

            // 将未读取数据前移到起始位置
            Array.Copy(array, Consumed, array, 0, unread);
            Consumed = 0;
            Length = unread;
        }

        /// <summary>
        /// 复制已写入的数据到新数组并返回。
        /// </summary>
        /// <returns>包含已写入数据的新数组。</returns>
        public T[] ToArray()
        {
            if (Length == 0)
                return Array.Empty<T>();
            var newArray = new T[Length];
            Array.Copy(array, newArray, Length);
            return newArray;
        }

        /// <summary>
        /// 克隆一个新的块，包含当前已写入的数据。
        /// </summary>
        /// <returns>新的 <see cref="MemoryBlock{T}"/> 实例。</returns>
        public MemoryBlock<T> Clone() => new MemoryBlock<T>(ToArray().AsSpan());

        /// <summary>
        /// 归还底层数组到池。调用后本实例不应再被使用。
        /// </summary>
        public void Dispose()
        {
            if (array != null)
            {
                _pool.Return(array);
                array = null;
                Consumed = 0;
                Length = 0;
            }
        }

        #region FormMemoryBlock

        /// <summary>隐式转换为已写入范围的只读 UnreadSpan。</summary>
        /// <param name="block">源块。</param>
        public static implicit operator ReadOnlySpan<T>(in MemoryBlock<T> block)
            => block.UnreadSpan;

        /// <summary>隐式转换为已写入范围的只读 UnreadMemory。</summary>
        /// <param name="block">源块。</param>
        public static implicit operator ReadOnlyMemory<T>(in MemoryBlock<T> block)
            => block.UnreadMemory;

        public static implicit operator ReadOnlySequence<T>(in MemoryBlock<T> block)
            => new ReadOnlySequence<T>(block.UnreadMemory);

        #endregion

        #region ToMemoryBlock


        public static implicit operator MemoryBlock<T>(ReadOnlySpan<T> span)
            => new MemoryBlock<T>(span);

        public static implicit operator MemoryBlock<T>(ReadOnlyMemory<T> memory)
            => new MemoryBlock<T>(memory);

        public static implicit operator MemoryBlock<T>(in ReadOnlySequence<T> sequence)
            => new MemoryBlock<T>(sequence);

        #endregion
    }
}