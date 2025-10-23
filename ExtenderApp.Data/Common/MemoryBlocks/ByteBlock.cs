using System.Buffers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 面向 byte 的缓冲块，内部直接复用 <see cref="MemoryBlock{T}"/>（T=byte）。
    /// - Length 表示已写入字节数（写指针/写边界）。
    /// - Consumed 表示当前读取位置（读指针）。
    /// 线程不安全；使用完毕需调用 <see cref="Dispose"/> 归还到数组池。
    /// </summary>
    public struct ByteBlock : IDisposable
    {
        /// <summary>
        ///底层内存块。
        /// </summary>
        private MemoryBlock<byte> _block;

        /// <summary>
        /// 已写入字节数（写指针/写边界）。
        /// </summary>
        public int Length => _block.Length;

        /// <summary>
        /// 当前读取位置（读指针）。范围：0到<see cref="Length"/>之间。
        /// </summary>
        public int Position => _block.Consumed;

        /// <summary>
        /// 底层缓冲容量。
        /// </summary>
        public int Capacity => _block.Capacity;

        /// <summary>
        /// 是否无任何已写入数据（Length == 0）。
        /// </summary>
        public bool IsEmpty => _block.IsEmpty;

        /// <summary>
        /// 内存块中剩余可读取的字节数
        /// </summary>
        public long Remaining => _block.Remaining;

        /// <summary>
        /// 当前字节读取位置（读指针）。
        /// </summary>
        public int Consumed => _block.Consumed;

        /// <summary>
        /// 获得已写入范围的只读字节切片。
        /// </summary>
        public ReadOnlySpan<byte> UnreadSpan => _block.UnreadSpan;

        /// <summary>
        /// 获得已写入范围的只读字节内存。
        /// </summary>
        public ReadOnlyMemory<byte> UnreadMemory => _block.UnreadMemory;

        public ByteBlock()
        {
            _block = new MemoryBlock<byte>();
        }

        /// <summary>
        /// 按指定容量创建缓冲。
        /// </summary>
        /// <param name="capacity">初始容量。</param>
        public ByteBlock(int capacity)
        {
            _block = new MemoryBlock<byte>(capacity);
        }

        /// <summary>
        /// 按指定容量与数组池创建缓冲。
        /// </summary>
        /// <param name="capacity">初始容量。</param>
        /// <param name="pool">数组池。</param>
        public ByteBlock(int capacity, ArrayPool<byte> pool)
        {
            _block = new MemoryBlock<byte>(capacity, pool);
        }

        /// <summary>
        /// 以内存内容初始化。
        /// </summary>
        /// <param name="memory">初始数据。</param>
        public ByteBlock(ReadOnlyMemory<byte> memory)
        {
            _block = new MemoryBlock<byte>(memory);
        }

        /// <summary>
        /// 以内存内容初始化，并使用指定数组池。
        /// </summary>
        /// <param name="memory">初始数据。</param>
        /// <param name="pool">数组池。</param>
        public ByteBlock(ReadOnlyMemory<byte> memory, ArrayPool<byte> pool)
        {
            _block = new MemoryBlock<byte>(memory, pool);
        }

        /// <summary>
        /// 以 UnreadSpan 内容初始化。
        /// </summary>
        /// <param name="span">初始数据。</param>
        public ByteBlock(ReadOnlySpan<byte> span)
        {
            _block = new MemoryBlock<byte>(span);
        }

        /// <summary>
        /// 以 UnreadSpan 内容初始化，并使用指定数组池。
        /// </summary>
        /// <param name="span">初始数据。</param>
        /// <param name="pool">数组池。</param>
        public ByteBlock(ReadOnlySpan<byte> span, ArrayPool<byte> pool)
        {
            _block = new MemoryBlock<byte>(span, pool);
        }

        /// <summary>
        /// 复制构造：克隆另一个 <see cref="ByteBlock"/> 的内容与读写指针。
        /// </summary>
        /// <param name="block">源实例（仅拷贝已写入范围 0..Length）。</param>
        public ByteBlock(in ByteBlock block) : this(block._block)
        {
        }

        public ByteBlock(in MemoryBlock<byte> block)
        {
            _block = new(block);
        }

        public ByteBlock(ByteBuffer buffer)
        {
            if (buffer.IsEmpty)
                throw new ArgumentException(nameof(buffer));

            _block = new MemoryBlock<byte>((int)buffer.Length);

            while (!buffer.End)
            {
                var span = buffer.UnreadSpan;
                Write(span);
                buffer.ReadAdvance(span.Length);
            }
        }

        /// <summary>
        /// 获取用于写入的可写内存切片，起点为当前 <see cref="Length"/>。
        /// </summary>
        /// <param name="sizeHint">预计要写入的最小字节数提示，用于触发扩容。</param>
        /// <returns>从写指针开始的可写 <see cref="Memory{T}"/>。</returns>
        public Memory<byte> GetMemory(int sizeHint = 0) => _block.GetMemory(sizeHint);

        /// <summary>
        /// 获取用于写入的可写 UnreadSpan，起点为当前 <see cref="Length"/>。
        /// </summary>
        /// <param name="sizeHint">预计要写入的最小字节数提示，用于触发扩容。</param>
        /// <returns>从写指针开始的可写 <see cref="System.Span{T}"/>。</returns>
        public Span<byte> GetSpan(int sizeHint = 0) => _block.GetSpan(sizeHint);

        /// <summary>
        /// 将写指针前进指定字节数（调用者需保证已实际写入）。
        /// </summary>
        /// <param name="count">推进的字节数。</param>
        public void WriteAdvance(int count) => _block.WriteAdvance(count);

        /// <summary>
        /// 将读指针前进指定字节数。
        /// </summary>
        /// <param name="count">推进的字节数。</param>
        public void ReadAdvance(int count) => _block.ReadAdvance(count);

        /// <summary>
        /// 确保至少还能写入 <paramref name="sizeHint"/> 个元素；不足则扩容。
        /// </summary>
        /// <param name="sizeHint">需要预留的最小写入空间。</param>
        public void Ensure(int sizeHint)
        {
            _block.Ensure(sizeHint);
        }

        /// <summary>
        /// 写入单个字节到当前写指针位置。
        /// </summary>
        /// <param name="value">要写入的字节。</param>
        public void Write(byte value)
            => _block.Write(value);

        /// <summary>
        /// 写入一段连续字节到当前写指针位置。
        /// </summary>
        /// <param name="span">要写入的数据切片。</param>
        public void Write(ReadOnlySpan<byte> span)
            => _block.Write(span);

        /// <summary>
        /// 写入一段连续字节到当前写指针位置。
        /// </summary>
        /// <param name="memory">要写入的数据内存。</param>
        public void Write(ReadOnlyMemory<byte> memory)
            => _block.Write(memory);

        /// <summary>
        /// 写入一个只读字节序列（可能由多段组成）。
        /// </summary>
        /// <param name="value">只读序列。</param>
        public void Write(in ReadOnlySequence<byte> value)
            => _block.Write(value);

        public void Write(in ByteBlock value)
        {
            Write(value.UnreadSpan);
        }

        public void Write(ByteBuffer buffer)
        {
            Ensure((int)buffer.Remaining);
            while (!buffer.End)
            {
                var span = buffer.UnreadSpan;
                Write(span);
                buffer.ReadAdvance(span.Length);
            }
        }

        public bool TryCopyTo(ref ByteBuffer buffer)
        {
            buffer.Write(UnreadSpan);
            return true;
        }

        /// <summary>
        /// 将可读数据（Length - Consumed）复制到目标 UnreadSpan。
        /// </summary>
        /// <param name="destination">目标缓冲。</param>
        /// <returns>若目标有足够空间则返回 true。</returns>
        public bool TryCopyTo(Span<byte> destination)
            => _block.TryCopyTo(destination);

        /// <summary>
        /// 将可读数据（Length - Consumed）复制到目标 UnreadSpan。
        /// </summary>
        /// <param name="destination">目标缓冲。</param>
        /// <param name="written">实际写入的字节数。</param>
        /// <returns>若目标有足够空间则返回 true。</returns>
        public bool TryCopyTo(Span<byte> destination, out int written)
            => _block.TryCopyTo(destination, out written);

        /// <summary>
        /// 查看当前读指针位置的字节而不移动指针。
        /// </summary>
        /// <param name="value">输出字节。</param>
        /// <returns>若存在可读字节则返回 true。</returns>
        public bool TryPeek(out byte value) => _block.TryPeek(out value);

        /// <summary>
        /// 查看相对当前读指针偏移处的字节（不移动指针）。
        /// </summary>
        /// <param name="offset">相对偏移量（从 Consumed 开始，需 ≥ 0）。</param>
        /// <param name="value">输出字节。</param>
        /// <returns>若存在可读字节则返回 true。</returns>
        public bool TryPeek(long offset, out byte value) => _block.TryPeek(offset, out value);

        /// <summary>
        /// 将读指针设置为绝对位置（0..Length）。
        /// </summary>
        /// <param name="count">新的读指针位置。</param>
        /// <exception cref="ArgumentOutOfRangeException">当位置不在 0..Length 范围内时抛出。</exception>
        public void Rewind(int count) => _block.Rewind(count);

        /// <summary>
        /// 重置读/写指针为起点（不清零数据）。
        /// </summary>
        public void Reset() => _block.Reset();

        /// <summary>
        /// 清零已使用区域的数据（不改变读/写指针）。
        /// </summary>
        public void Clear() => _block.Clear();

        /// <summary>
        /// 复制已写入的数据为新数组。
        /// </summary>
        /// <returns>包含已写入数据的新数组。</returns>
        public byte[] ToArray() => _block.ToArray();

        /// <summary>
        /// 克隆一个新的 <see cref="ByteBlock"/>（包含当前已写入数据与读写位置）。
        /// </summary>
        /// <returns>新的 <see cref="ByteBlock"/> 实例。</returns>
        public ByteBlock Clone() => new ByteBlock(this);

        /// <summary>
        /// 归还底层缓冲到数组池。调用后不应再使用此实例。
        /// </summary>
        public void Dispose() => _block.Dispose();

        #region FormByteBlock

        /// <summary>
        /// 隐式转换为已写入范围的只读字节 UnreadSpan。
        /// 注意：该切片引用底层缓冲，生命周期应短于本实例且不得在 <see cref="Dispose"/> 后使用。
        /// </summary>
        /// <param name="block">源实例。</param>
        public static implicit operator ReadOnlySpan<byte>(in ByteBlock block)
            => block.UnreadSpan;

        /// <summary>
        /// 隐式转换为已写入范围的只读字节 UnreadMemory。
        /// 注意：该切片引用底层缓冲，生命周期应短于本实例且不得在 <see cref="Dispose"/> 后使用。
        /// </summary>
        /// <param name="block">源实例。</param>
        public static implicit operator ReadOnlyMemory<byte>(in ByteBlock block)
            => block.UnreadMemory;

        public static explicit operator ByteBuffer(in ByteBlock block)
            => new ByteBuffer(block.UnreadMemory);

        #endregion

        #region ToByteBlock

        public static explicit operator ByteBlock(ReadOnlySpan<byte> span)
            => new ByteBlock(span);

        public static explicit operator ByteBlock(ReadOnlyMemory<byte> memory)
            => new ByteBlock(memory);

        public static explicit operator ByteBlock(ReadOnlySequence<byte> sequence)
            => new ByteBlock((ByteBuffer)sequence);

        public static explicit operator ByteBlock(MemoryBlock<byte> block)
            => new ByteBlock(block);

        public static explicit operator ByteBlock(ByteBuffer buffer)
            => new ByteBlock(buffer);

        #endregion
    }
}
