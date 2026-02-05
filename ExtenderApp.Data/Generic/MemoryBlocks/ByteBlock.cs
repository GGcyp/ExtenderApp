using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 面向 byte 的缓冲块，内部直接复用 <see cref="MemoryBlock{T}"/>（T=byte）。
    /// - WrittenCount 表示已写入字节数（写指针/写边界）。
    /// - Consumed 表示当前读取位置（读指针）。
    /// 线程不安全；使用完毕需调用 <see cref="Dispose"/> 归还到数组池。
    /// </summary>
    public struct ByteBlock : IDisposable, IEquatable<ByteBlock>
    {
        /// <summary>
        /// 底层内存块。
        /// </summary>
        private MemoryBlock<byte> _block;

        /// <summary>
        /// 已写入字节数（写指针/写边界）。
        /// </summary>
        public int WrittenCount => _block.WrittenCount;

        /// <summary>
        /// 底层缓冲容量。
        /// </summary>
        public int Capacity => _block.Capacity;

        /// <summary>
        /// 可写入的字节数。
        /// </summary>
        public int WritableBytes => _block.WritableBytes;

        /// <summary>
        /// 是否无任何已写入数据（WrittenCount == 0）。
        /// </summary>
        public bool IsEmpty => _block.IsEmpty;

        /// <summary>
        /// 内存块中剩余可读取的字节数。
        /// </summary>
        public int Remaining => _block.Remaining;

        /// <summary>
        /// 当前字节读取位置（读指针）。
        /// </summary>
        public int Consumed => _block.Consumed;

        /// <summary>
        /// 获得已写入范围的只读字节切片。
        /// 注意：该切片引用底层缓冲，生命周期应短于本实例且不得在 <see cref="Dispose"/> 后使用。
        /// </summary>
        public ReadOnlySpan<byte> UnreadSpan => _block.UnreadSpan;

        /// <summary>
        /// 获得已写入范围的只读字节内存。
        /// 注意：该切片引用底层缓冲，生命周期应短于本实例且不得在 <see cref="Dispose"/> 后使用。
        /// </summary>
        public ReadOnlyMemory<byte> UnreadMemory => _block.UnreadMemory;

        /// <summary>
        /// 获取已写入范围的字节数组段。
        /// </summary>
        public ArraySegment<byte> UnreadSegment => _block.UnreadSegment;

        #region Constructor

        /// <summary>
        /// 默认构造函数，创建空缓冲块。
        /// </summary>
        public ByteBlock()
        {
            _block = new MemoryBlock<byte>();
        }

        /// <summary>
        /// 按指定容量创建缓冲。
        /// </summary>
        /// <param name="capacity">初始容量。</param>
        public ByteBlock(int capacity) : this(capacity, ArrayPool<byte>.Shared)
        {
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
        /// 从字节数组初始化。
        /// </summary>
        /// <param name="bytes">指定字节数组。</param>
        public ByteBlock(byte[] bytes) : this(bytes.AsMemory())
        {
        }

        /// <summary>
        /// 从字节数组初始化，并使用指定数组池。
        /// </summary>
        /// <param name="bytes">指定字节数组。</param>
        /// <param name="pool">数组池。</param>
        public ByteBlock(byte[] bytes, ArrayPool<byte> pool) : this(bytes.AsMemory(), pool)
        {
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
        /// <param name="block">源实例（仅拷贝已写入范围 0..WrittenCount）。</param>
        public ByteBlock(in ByteBlock block) : this(block._block)
        {
        }

        /// <summary>
        /// 以 MemoryBlock 内容初始化。
        /// </summary>
        /// <param name="block">源 MemoryBlock。</param>
        public ByteBlock(in MemoryBlock<byte> block)
        {
            _block = new(block);
        }

        /// <summary>
        /// 从 <see cref="ReadOnlySequence{byte}"/> 初始化。
        /// </summary>
        /// <param name="memories">源 <see cref="ReadOnlySequence{byte}"/>。</param>
        /// <exception cref="ArgumentException">当 <paramref name="memories"/> 为空时抛出。</exception>
        public ByteBlock(in ReadOnlySequence<byte> memories)
        {
            if (memories.IsEmpty)
                throw new ArgumentException(nameof(memories));

            _block = new MemoryBlock<byte>((int)memories.Length);
            Write(memories);
        }

        #endregion Constructor

        /// <summary>
        /// 获取用于写入的可写内存切片，起点为当前 <see cref="WrittenCount"/>。
        /// </summary>
        /// <param name="sizeHint">预计要写入的最小字节数提示，用于触发扩容。</param>
        /// <returns>从写指针开始的可写 <see cref="Memory{T}"/>。</returns>
        public Memory<byte> GetMemory(int sizeHint = 0) => _block.GetMemory(sizeHint);

        /// <summary>
        /// 获取用于写入的可写 Span，起点为当前 <see cref="WrittenCount"/>。
        /// </summary>
        /// <param name="sizeHint">预计要写入的最小字节数提示，用于触发扩容。</param>
        /// <returns>从写指针开始的可写 <see cref="Span{T}"/>。</returns>
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

        #region CopyTo

        /// <summary>
        /// 将当前块的可读数据（WrittenCount - Consumed）复制到目标 <see cref="ByteBuffer"/>，不改变当前块的读指针。
        /// </summary>
        /// <param name="buffer">目标缓冲，将在其末尾追加写入。</param>
        /// <returns>始终返回 true。</returns>
        public bool TryCopyTo(ref ByteBuffer buffer)
        {
            buffer.Write(UnreadSpan);
            return true;
        }

        /// <summary>
        /// 将可读数据（WrittenCount - Consumed）复制到目标 <paramref name="destination"/>。
        /// </summary>
        /// <param name="destination">目标缓冲。</param>
        /// <returns>若目标有足够空间则返回 true。</returns>
        public bool TryCopyTo(Span<byte> destination)
            => _block.TryCopyTo(destination);

        /// <summary>
        /// 将可读数据（WrittenCount - Consumed）复制到目标 <paramref name="destination"/>，并输出写入字节数。
        /// </summary>
        /// <param name="destination">目标缓冲。</param>
        /// <param name="written">实际写入的字节数。</param>
        /// <returns>若目标有足够空间则返回 true。</returns>
        public bool TryCopyTo(Span<byte> destination, out int written)
            => _block.TryCopyTo(destination, out written);

        /// <summary>
        /// 尝试将可读数据复制到目标原生内存块。
        /// </summary>
        /// <param name="memory">目标原生内存块。</param>
        /// <returns>若目标有足够空间则返回 true。</returns>
        public bool TryCopyTo(NativeByteMemory memory)
        {
            if (memory.IsEmpty || Remaining > memory.Length)
                return false;

            return UnreadSpan.Slice(0, memory.Length).TryCopyTo(memory.Span);
        }

        #endregion CopyTo

        #region Peek

        /// <summary>
        /// 查看当前读指针位置的字节而不移动指针。
        /// </summary>
        /// <param name="value">输出字节。</param>
        /// <returns>若存在可读字节则返回 true。</returns>
        public bool TryPeek(out byte value) => _block.TryPeek(out value);

        /// <summary>
        /// 获取当前块的浅拷贝，可用于多次读取而不改变读写指针。
        /// </summary>
        /// <returns>浅拷贝实例。</returns>
        public ByteBlock PeekByteBlock() => this;

        #endregion Peek

        #region Uitil

        /// <summary>
        /// 将读指针设置为绝对位置（0..WrittenCount）。
        /// </summary>
        /// <param name="count">新的读指针位置。</param>
        /// <exception cref="ArgumentOutOfRangeException">当位置不在 0..WrittenCount 范围内时抛出。</exception>
        public void Rewind(int count) => _block.Rewind(count);

        /// <summary>
        /// 重置读/写指针为起点（不清零数据）。
        /// </summary>
        public void Reset() => _block.Reset();

        /// <summary>
        /// 将未消费的数据移动到缓冲起始处并重置读写指针。
        /// </summary>
        public void Compact() => _block.Compact();

        /// <summary>
        /// 清零已使用区域的数据（不改变读/写指针）。
        /// </summary>
        public void Clear() => _block.Clear();

        /// <summary>
        /// 复制剩余未读数据（WrittenCount - Consumed）为新数组，不改变读/写指针。
        /// </summary>
        /// <returns>包含未读数据的新数组；若无未读数据则返回空数组。</returns>
        public byte[] ToArray() => _block.ToArray();

        /// <summary>
        /// 复制当前已写入的数据为新数组（范围 [0..WrittenCount)，包含已消费部分），不改变读/写指针。
        /// </summary>
        /// <returns>包含 [0..WrittenCount) 范围内数据的新数组；若无已写入数据则返回空数组。</returns>
        public byte[] ToAllArray() => _block.ToAllArray();

        /// <summary>
        /// 克隆一个新的 <see cref="ByteBlock"/>（包含当前已写入数据与读写位置）。
        /// </summary>
        /// <returns>新的 <see cref="ByteBlock"/> 实例。</returns>
        public ByteBlock Clone() => new ByteBlock(this);

        /// <summary>
        /// 反转从已写入数据的读指针到写指针范围内的顺序。不改变读/写指针。
        /// </summary>
        public void Reverse()
        {
            _block.Reverse();
        }

        /// <summary>
        /// 反转从已写入数据的读指针到写指针范围内的顺序。不改变读/写指针。
        /// </summary>
        /// <param name="start">开始索引（不能提前于已读取的位置）。</param>
        /// <param name="length">要反转的元素个数（不能多于未读取的个数）。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 start 或 length 超出已写入范围时抛出。</exception>
        public void Reverse(int start, int length)
        {
            _block.Reverse(start, length);
        }

        #endregion Uitil

        /// <summary>
        /// 归还底层缓冲到数组池。调用后不应再使用此实例。
        /// </summary>
        public void Dispose()
        {
            _block.Dispose();
        }

        /// <summary>
        /// 返回已写入范围内字节的十六进制表示（带空格分隔），用于调试。
        /// </summary>
        /// <returns>十六进制字符串表示。</returns>
        public override string ToString()
        {
            StringBuilder sb = new(Remaining);
            ReadOnlySpan<byte> span = UnreadSpan;
            for (int i = 0; i < WrittenCount; i++)
            {
                sb.AppendFormat("{0:X2} ", span[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将未读数据计算 SHA256 哈希码并生成哈希码，用于集合键。
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            Span<byte> span = stackalloc byte[SHA256.HashSizeInBytes];
            SHA256.HashData(UnreadSpan, span);
            return BitConverter.ToInt32(span);
        }

        /// <inheritdoc/>
        public bool Equals(ByteBlock other)
        {
            if (IsEmpty && other.IsEmpty) return true;
            if (IsEmpty || other.IsEmpty) return false;

            return UnreadSpan.SequenceEqual(other.UnreadSpan);
        }

        public static bool operator ==(ByteBlock left, ByteBlock rigth)
        {
            return left.Equals(rigth);
        }

        public static bool operator !=(ByteBlock left, ByteBlock rigth)
        {
            return left.Equals(rigth);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is ByteBlock block && Equals(block);
        }

        #region Write

        /// <summary>
        /// 写入单个字节到当前写指针位置。
        /// </summary>
        /// <param name="value">要写入的字节。</param>
        public void Write(byte value)
            => _block.Write(value);

        /// <summary>
        /// 写入整个字节数组到当前写指针位置。
        /// </summary>
        /// <param name="value">要写入的字节数组。为 null 时将导致异常。</param>
        public void Write(byte[] value)
            => _block.Write(value.AsMemory());

        /// <summary>
        /// 写入字节数组的一部分到当前写指针位置。
        /// </summary>
        /// <param name="value">要写入的字节数组。</param>
        /// <param name="offset">数组中开始写入的偏移量。</param>
        /// <param name="count">要写入的字节数。</param>
        public void Write(byte[] value, int offset, int count)
            => _block.Write(new ReadOnlyMemory<byte>(value, offset, count));

        /// <summary>
        /// 写入字节数组段到当前写指针位置。
        /// </summary>
        /// <param name="segment">要写入的字节数组段。</param>
        public void Write(ArraySegment<byte> segment)
            => _block.Write(segment.AsMemory());

        /// <summary>
        /// 将字符串按指定编码写入当前写指针位置（不包含长度或终止符），不改变读指针。
        /// </summary>
        /// <param name="value">要写入的字符串；null 或空字符串时不执行任何操作。</param>
        /// <param name="encoding">字符编码，默认 UTF-8。</param>
        public void Write(string value, Encoding? encoding = null)
        {
            if (string.IsNullOrEmpty(value))
                return;

            encoding ??= Encoding.UTF8;
            var byteCount = encoding.GetByteCount(value);
            var memory = GetMemory(byteCount);
            encoding.GetBytes(value, memory.Span);
            WriteAdvance(byteCount);
        }

        /// <summary>
        /// 写入多个相同字节到当前写指针位置。
        /// </summary>
        /// <param name="value">需要写入的字节。</param>
        /// <param name="count">写入个数。</param>
        public void Write(byte value, int count)
        {
            Ensure(count);
            Span<byte> span = GetSpan(count).Slice(0, count);
            span.Fill(value);
            WriteAdvance(count);
        }

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

        /// <summary>
        /// 将另一个 <see cref="ByteBlock"/> 的未读数据写入当前块。
        /// </summary>
        /// <param name="value">来源 <see cref="ByteBlock"/>，其未读数据将被复制到当前块。</param>
        public void Write(in ByteBlock value)
        {
            Write(value.UnreadSpan);
        }

        /// <summary>
        /// 写入另一个 <see cref="MemoryBlock{byte}"/> 的未读数据到当前块。
        /// </summary>
        /// <param name="block">来源内存块，其 <c>UnreadSpan</c> 将被完整追加。</param>
        public void Write(in MemoryBlock<byte> block)
        {
            Write(block.UnreadSpan);
        }

        /// <summary>
        /// 将 <see cref="NativeByteMemory"/> 的内容写入当前块。
        /// </summary>
        /// <param name="memory">指定原生内存块。</param>
        public void Write(NativeByteMemory memory)
        {
            Write(memory.Span);
        }

        #endregion Write

        #region Read

        /// <summary>
        /// 读取当前未读的第一个字节，并将读指针前进 1（不影响写指针）。
        /// </summary>
        /// <returns>读取到的字节。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据（Remaining == 0）时抛出。</exception>
        public byte Read()
            => _block.Read();

        /// <summary>
        /// 读取最多 <paramref name="destination"/>.WrittenCount 个未读字节到目标跨度，并将读指针前进相应长度（不影响写指针）。
        /// </summary>
        /// <param name="destination">接收数据的目标跨度。</param>
        /// <returns>实际读取的字节数（等于 Math.Min(destination.WrittenCount, Remaining)）。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据（Remaining == 0）时抛出。</exception>
        public int Read(Span<byte> destination)
            => _block.Read(destination);

        /// <summary>
        /// 读取最多 <paramref name="destination"/>.WrittenCount 个未读字节到目标内存，并将读指针前进相应长度（不影响写指针）。
        /// </summary>
        /// <param name="destination">接收数据的目标内存。</param>
        /// <returns>实际读取的字节数（等于 Math.Min(destination.WrittenCount, Remaining)）。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据（Remaining == 0）时抛出。</exception>
        public int Read(Memory<byte> destination)
            => _block.Read(destination);

        /// <summary>
        /// 读取最多 <paramref name="byteCount"/> 个未读字节，并将读指针前进相应长度（不影响写指针）。
        /// </summary>
        /// <param name="byteCount">期望读取的字节数；若超过可读数量将按 <see cref="Remaining"/> 截断。</param>
        /// <returns>指向所读数据的只读跨度。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据（Remaining == 0）时抛出。</exception>
        public ReadOnlySpan<byte> Read(int byteCount)
        {
            return _block.Read(byteCount);
        }

        #endregion Read

        #region FormByteBlock

        /// <summary>
        /// 隐式转换为已写入范围的只读字节 <see cref="UnreadSpan"/>。
        /// 注意：该切片引用底层缓冲，生命周期应短于本实例且不得在 <see cref="Dispose"/> 后使用。
        /// </summary>
        /// <param name="block">源实例。</param>
        public static implicit operator ReadOnlySpan<byte>(in ByteBlock block)
            => block.UnreadSpan;

        /// <summary>
        /// 隐式转换为已写入范围的只读字节内存 <see cref="UnreadMemory"/>。
        /// 注意：该切片引用底层缓冲，生命周期应短于本实例且不得在 <see cref="Dispose"/> 后使用。
        /// </summary>
        /// <param name="block">源实例。</param>
        public static implicit operator ReadOnlyMemory<byte>(in ByteBlock block)
            => block.UnreadMemory;

        #endregion FormByteBlock

        #region ToByteBlock

        /// <summary>
        /// 将只读字节切片显式转换为新的 <see cref="ByteBlock"/>（拷贝数据）。
        /// </summary>
        /// <param name="span">源字节切片。</param>
        public static explicit operator ByteBlock(ReadOnlySpan<byte> span)
            => new ByteBlock(span);

        /// <summary>
        /// 将只读字节内存显式转换为新的 <see cref="ByteBlock"/>（拷贝数据）。
        /// </summary>
        /// <param name="memory">源字节内存。</param>
        public static explicit operator ByteBlock(ReadOnlyMemory<byte> memory)
            => new ByteBlock(memory);

        /// <summary>
        /// 将多段字节序列显式转换为新的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="sequence">源 <see cref="ReadOnlySequence{Byte}"/>。</param>
        public static explicit operator ByteBlock(ReadOnlySequence<byte> sequence)
            => new ByteBlock((ByteBuffer)sequence);

        /// <summary>
        /// 将 <see cref="MemoryBlock{byte}"/> 显式转换为 <see cref="ByteBlock"/>（拷贝其已写入范围）。
        /// </summary>
        /// <param name="block">源内存块。</param>
        public static explicit operator ByteBlock(MemoryBlock<byte> block)
            => new ByteBlock(block);

        #endregion ToByteBlock
    }
}