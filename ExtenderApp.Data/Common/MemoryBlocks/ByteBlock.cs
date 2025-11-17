using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 面向 byte 的缓冲块，内部直接复用 <see cref="MemoryBlock{T}"/>（T=byte）。
    /// - Length 表示已写入字节数（写指针/写边界）。
    /// - Consumed 表示当前读取位置（读指针）。 线程不安全；使用完毕需调用
    /// <see cref="Dispose"/> 归还到数组池。
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
        /// 当前读取位置（读指针）。范围：0到 <see cref="Length"/> 之间。
        /// </summary>
        public int Position => _block.Consumed;

        /// <summary>
        /// 底层缓冲容量。
        /// </summary>
        public int Capacity => _block.Capacity;

        /// <summary>
        /// 可写入的字节数。
        /// </summary>
        public int WritableBytes => _block.WritableBytes;

        /// <summary>
        /// 是否无任何已写入数据（Length == 0）。
        /// </summary>
        public bool IsEmpty => _block.IsEmpty;

        /// <summary>
        /// 内存块中剩余可读取的字节数
        /// </summary>
        public int Remaining => _block.Remaining;

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

        public ByteBlock(byte[] bytes) : this(bytes.AsMemory())
        {
        }

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
        /// <param name="block">
        /// 源实例（仅拷贝已写入范围 0..Length）。
        /// </param>
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
        /// 将写指针前进指定字节数（调用者需保证已实际写入）或全填默认值。
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
        /// 使用指定编码读取剩余全部未读字节为字符串，并将读指针前进相应字节数（不影响写指针）。
        /// </summary>
        /// <param name="encoding">字符编码，默认 UTF-8。</param>
        /// <returns>解析得到的字符串。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当没有可读数据（Remaining == 0）时抛出。
        /// </exception>
        public string ReadString(Encoding? encoding = null)
        {
            return ReadString(Remaining, encoding);
        }

        /// <summary>
        /// 使用指定编码读取给定字节数为字符串，并将读指针前进相应字节数（不影响写指针）。
        /// </summary>
        /// <param name="byteCount">
        /// 要读取并解码的字节数（必须在 1..Remaining 范围内）。
        /// </param>
        /// <param name="encoding">字符编码，默认 UTF-8。</param>
        /// <returns>解析得到的字符串。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="byteCount"/> ≤ 0 或大于可读字节数（Remaining）时抛出。
        /// </exception>
        public string ReadString(int byteCount, Encoding? encoding = null)
        {
            if (byteCount <= 0 || byteCount > Remaining)
                throw new ArgumentOutOfRangeException(nameof(byteCount));
            encoding ??= Encoding.UTF8;
            var span = _block.Read(byteCount);
            var result = encoding.GetString(span);
            return result;
        }

        /// <summary>
        /// 将当前块的可读数据（Length - Consumed）复制到目标 <see cref="ByteBuffer"/>，不改变当前块的读指针。
        /// </summary>
        /// <param name="buffer">目标缓冲，将在其末尾追加写入。</param>
        /// <returns>始终返回 true。</returns>
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
        /// <param name="offset">
        /// 相对偏移量（从 Consumed 开始，需 ≥ 0）。
        /// </param>
        /// <param name="value">输出字节。</param>
        /// <returns>若存在可读字节则返回 true。</returns>
        public bool TryPeek(long offset, out byte value) => _block.TryPeek(offset, out value);

        /// <summary>
        /// 获取当前块的浅拷贝，可用于多次读取而不改变读写指针。
        /// </summary>
        /// <returns>浅拷贝实例</returns>
        public ByteBlock PeekByteBlock() => this;

        /// <summary>
        /// 将读指针设置为绝对位置（0..Length）。
        /// </summary>
        /// <param name="count">新的读指针位置。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当位置不在 0..Length 范围内时抛出。
        /// </exception>
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
        /// 复制剩余未读数据（Length - Consumed）为新数组，不改变读/写指针。
        /// </summary>
        /// <returns>包含未读数据的新数组；若无未读数据则返回空数组。</returns>
        public byte[] ToArray() => _block.ToArray();

        /// <summary>
        /// 复制当前已写入的数据为新数组（范围 [0..Length)，包含已消费部分），不改变读/写指针。
        /// </summary>
        /// <returns>包含 [0..Length) 范围内数据的新数组；若无已写入数据则返回空数组。</returns>
        public byte[] ToAllArray() => _block.ToAllArray();

        /// <summary>
        /// 克隆一个新的 <see cref="ByteBlock"/>（包含当前已写入数据与读写位置）。
        /// </summary>
        /// <returns>
        /// 新的 <see cref="ByteBlock"/> 实例。
        /// </returns>
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 start 或 length 超出已写入范围时抛出。
        /// </exception>
        public void Reverse(int start, int length)
        {
            _block.Reverse(start, length);
        }

        /// <summary>
        /// 归还底层缓冲到数组池。调用后不应再使用此实例。
        /// </summary>
        public void Dispose() => _block.Dispose();

        public override string ToString()
        {
            StringBuilder sb = new(Length);
            ReadOnlySpan<byte> span = UnreadSpan;
            for (int i = 0; i < Length; i++)
            {
                sb.AppendFormat("{0:X2} ", span[i]);
            }
            return sb.ToString();
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
        /// <param name="value">
        /// 要写入的字节数组。为 null 时将导致异常。
        /// </param>
        public void Write(byte[] value)
            => _block.Write(value.AsMemory());

        /// <summary>
        /// 将字符串按指定编码写入当前写指针位置（不包含长度或终止符），不改变读指针。
        /// </summary>
        /// <param name="value">
        /// 要写入的字符串；null 或空字符串时不执行任何操作。
        /// </param>
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
        /// <param name="value">将写入<see cref="ByteBlock"/></param>
        public void Write(in ByteBlock value)
        {
            Write(value.UnreadSpan);
        }

        /// <summary>
        /// 将 <see cref="ByteBuffer"/> 的未读数据逐段写入当前块。
        /// </summary>
        /// <param name="buffer">源缓冲，将读取其未读片段并写入当前块。</param>
        /// <remarks>
        /// 会根据 <paramref name="buffer"/> 的剩余长度预留容量；写入过程中会推进源缓冲的读取位置，不改变当前块的读指针。
        /// </remarks>
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

        /// <summary>
        /// 写入另一个 <see cref="MemoryBlock{byte}"/> 的未读数据到当前块。
        /// </summary>
        /// <param name="block">来源内存块，其 <c>UnreadSpan</c> 将被完整追加。</param>
        public void Write(in MemoryBlock<byte> block)
        {
            Write(block.UnreadSpan);
        }

        /// <summary>
        /// 写入一个 16 位有符号整数（short）。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="isBigEndian">是否采用大端字节序（默认大端，常用于网络协议）。</param>
        /// <remarks>写入字节数：2。</remarks>
        public void Write(short value, bool isBigEndian = true)
        {
            const int shortCount = sizeof(short);
            Span<byte> bytes = GetSpan(shortCount);
            if (isBigEndian)
                BinaryPrimitives.WriteInt16BigEndian(bytes, value);
            else
                BinaryPrimitives.WriteInt16LittleEndian(bytes, value);
            WriteAdvance(shortCount);
        }

        /// <summary>
        /// 写入一个 32 位有符号整数（int）。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="isBigEndian">是否采用大端字节序。</param>
        /// <remarks>写入字节数：4。</remarks>
        public void Write(int value, bool isBigEndian = true)
        {
            const int intCount = sizeof(int);
            Span<byte> bytes = GetSpan(intCount);
            if (isBigEndian)
                BinaryPrimitives.WriteInt32BigEndian(bytes, value);
            else
                BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
            WriteAdvance(intCount);
        }

        /// <summary>
        /// 写入一个 64 位有符号整数（long）。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="isBigEndian">是否采用大端字节序。</param>
        /// <remarks>写入字节数：8。</remarks>
        public void Write(long value, bool isBigEndian = true)
        {
            const int longCount = sizeof(long);
            Span<byte> bytes = GetSpan(longCount);
            if (isBigEndian)
                BinaryPrimitives.WriteInt64BigEndian(bytes, value);
            else
                BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
            WriteAdvance(longCount);
        }

        /// <summary>
        /// 写入一个 32 位单精度浮点数（float）。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="isBigEndian">是否采用大端字节序。</param>
        /// <remarks>写入字节数：4。使用 IEEE 754 位模式。</remarks>
        public void Write(float value, bool isBigEndian = true)
        {
            const int floatCount = sizeof(float);
            Span<byte> bytes = GetSpan(floatCount);
            if (isBigEndian)
                BinaryPrimitives.WriteSingleBigEndian(bytes, value);
            else
                BinaryPrimitives.WriteSingleLittleEndian(bytes, value);
            WriteAdvance(floatCount);
        }

        /// <summary>
        /// 写入一个 64 位双精度浮点数（double）。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="isBigEndian">是否采用大端字节序。</param>
        /// <remarks>写入字节数：8。使用 IEEE 754 位模式。</remarks>
        public void Write(double value, bool isBigEndian = true)
        {
            const int doubleCount = sizeof(double);
            Span<byte> bytes = GetSpan(doubleCount);
            if (isBigEndian)
                BinaryPrimitives.WriteDoubleBigEndian(bytes, value);
            else
                BinaryPrimitives.WriteDoubleLittleEndian(bytes, value);
            WriteAdvance(doubleCount);
        }

        /// <summary>
        /// 写入一个 16 位无符号整数（ushort）。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="isBigEndian">是否采用大端字节序。</param>
        /// <remarks>写入字节数：2。</remarks>
        public void Write(ushort value, bool isBigEndian = true)
        {
            const int ushortCount = sizeof(ushort);
            Span<byte> bytes = GetSpan(ushortCount);
            if (isBigEndian)
                BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
            else
                BinaryPrimitives.WriteUInt16LittleEndian(bytes, value);
            WriteAdvance(ushortCount);
        }

        /// <summary>
        /// 写入一个 32 位无符号整数（uint）。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="isBigEndian">是否采用大端字节序。</param>
        /// <remarks>写入字节数：4。</remarks>
        public void Write(uint value, bool isBigEndian = true)
        {
            const int uintCount = sizeof(uint);
            Span<byte> bytes = GetSpan(uintCount);
            if (isBigEndian)
                BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
            else
                BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
            WriteAdvance(uintCount);
        }

        /// <summary>
        /// 写入一个 64 位无符号整数（ulong）。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="isBigEndian">是否采用大端字节序。</param>
        /// <remarks>写入字节数：8。</remarks>
        public void Write(ulong value, bool isBigEndian = true)
        {
            const int ulongCount = sizeof(ulong);
            Span<byte> bytes = GetSpan(ulongCount);
            if (isBigEndian)
                BinaryPrimitives.WriteUInt64BigEndian(bytes, value);
            else
                BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
            WriteAdvance(ulongCount);
        }

        /// <summary>
        /// 写入一个 <see cref="decimal"/> 值（内部 128 位结构，顺序写入四个 Int32，高位在前）。
        /// </summary>
        /// <param name="value">要写入的十进制数。</param>
        /// <remarks>写入字节数：16。字段顺序与 <see cref="decimal.GetBits(decimal)"/> 返回数组一致。</remarks>
        public void Write(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < bits.Length; i++)
            {
                Write(bits[i], isBigEndian: true);
            }
        }

        /// <summary>
        /// 写入一个布尔值（1 字节：true=0x01，false=0x00）。
        /// </summary>
        /// <param name="value">要写入的布尔值。</param>
        public void Write(bool value)
        {
            Write(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// 写入单个字符。默认使用 UTF-8 编码（长度可变），不写入长度前缀。
        /// </summary>
        /// <param name="value">要写入的字符。</param>
        /// <param name="encoding">字符编码，默认为 UTF-8。</param>
        public void Write(char value, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            Span<char> span = stackalloc char[1] { value };
            var byteCount = encoding.GetByteCount(span);
            var memory = GetMemory(byteCount);
            encoding.GetBytes(span, memory.Span);
            WriteAdvance(byteCount);
        }

        /// <summary>
        /// 写入一段字符跨度。默认 UTF-8 编码，不写长度前缀。
        /// </summary>
        /// <param name="value">要写入的字符跨度。</param>
        /// <param name="encoding">字符编码（默认 UTF-8）。</param>
        public void Write(ReadOnlySpan<char> value, Encoding? encoding = null)
        {
            if (value.IsEmpty)
                return;

            encoding ??= Encoding.UTF8;
            var byteCount = encoding.GetByteCount(value);
            var memory = GetMemory(byteCount);
            encoding.GetBytes(value, memory.Span);
            WriteAdvance(byteCount);
        }

        /// <summary>
        /// 写入一段只读字符内存。默认 UTF-8 编码，不写长度前缀。
        /// </summary>
        /// <param name="value">要写入的字符内存。</param>
        /// <param name="encoding">字符编码（默认 UTF-8）。</param>
        public void Write(ReadOnlyMemory<char> value, Encoding? encoding = null)
        {
            if (value.IsEmpty)
                return;

            encoding ??= Encoding.UTF8;
            var byteCount = encoding.GetByteCount(value.Span);
            var memory = GetMemory(byteCount);
            encoding.GetBytes(value.Span, memory.Span);
            WriteAdvance(byteCount);
        }

        /// <summary>
        /// 写入一个 GUID（16 字节，按其内部字节表示，与 <see cref="Guid.TryWriteBytes"/> 一致）。
        /// </summary>
        /// <param name="value">要写入的 GUID。</param>
        /// <remarks>写入字节数：16。</remarks>
        public void Write(Guid value)
        {
            Span<byte> bytes = stackalloc byte[16];
            value.TryWriteBytes(bytes);
            Write(bytes);
        }

        /// <summary>
        /// 写入一个 <see cref="DateTime"/> 的 Ticks（Int64）。默认为大端序。
        /// </summary>
        /// <param name="value">日期时间值。</param>
        /// <param name="isBigEndian">是否大端字节序。</param>
        /// <remarks>写入字节数：8。</remarks>
        public void Write(DateTime value, bool isBigEndian = true)
        {
            long ticks = value.Ticks;
            Write(ticks, isBigEndian);
        }

        /// <summary>
        /// 写入一个 <see cref="DateTimeOffset"/> 的 Ticks（Int64）。不写偏移量。
        /// </summary>
        /// <param name="value">日期时间偏移值。</param>
        /// <param name="isBigEndian">是否大端字节序。</param>
        /// <remarks>仅写入 <see cref="DateTimeOffset.Ticks"/>；如需保留 Offset 需另行写入。</remarks>
        public void Write(DateTimeOffset value, bool isBigEndian = true)
        {
            long ticks = value.Ticks;
            Write(ticks, isBigEndian);
        }

        /// <summary>
        /// 写入一个 <see cref="TimeSpan"/> 的 Ticks（Int64）。
        /// </summary>
        /// <param name="value">时间间隔值。</param>
        /// <param name="isBigEndian">是否大端字节序。</param>
        /// <remarks>写入字节数：8。</remarks>
        public void Write(TimeSpan value, bool isBigEndian = true)
        {
            long ticks = value.Ticks;
            Write(ticks, isBigEndian);
        }

        #endregion

        #region Read


        /// <summary>
        /// 读取当前未读的第一个字节，并将读指针前进 1（不影响写指针）。
        /// </summary>
        /// <returns>读取到的字节。</returns>
        /// <exception cref="InvalidOperationException">
        /// 当没有可读数据（Remaining == 0）时抛出。
        /// </exception>
        public byte Read()
            => _block.Read();

        /// <summary>
        /// 读取最多 <paramref
        /// name="destination"/>.Length 个未读字节到目标跨度，并将读指针前进相应长度（不影响写指针）。
        /// </summary>
        /// <param name="destination">接收数据的目标跨度。</param>
        /// <returns>实际读取的字节数。</returns>
        /// <exception cref="InvalidOperationException">
        /// 当没有可读数据（Remaining == 0）时抛出。
        /// </exception>
        /// <remarks>
        /// 读取数量为 Math.Min(destination.Length, Remaining)。
        /// </remarks>
        public int Read(Span<byte> destination)
            => _block.Read(destination);

        /// <summary>
        /// 读取最多 <paramref
        /// name="destination"/>.Length 个未读字节到目标内存，并将读指针前进相应长度（不影响写指针）。
        /// </summary>
        /// <param name="destination">接收数据的目标内存。</param>
        /// <returns>实际读取的字节数。</returns>
        /// <exception cref="InvalidOperationException">
        /// 当没有可读数据（Remaining == 0）时抛出。
        /// </exception>
        /// <remarks>
        /// 等同于调用 Read(destination.Span)。实际读取数量为
        /// Math.Min(destination.Length, Remaining)。
        /// </remarks>
        public int Read(Memory<byte> destination)
            => _block.Read(destination);

        /// <summary>
        /// 读取最多 <paramref name="byteCount"/> 个未读字节，并将读指针前进相应长度（不影响写指针）。
        /// </summary>
        /// <param name="byteCount">
        /// 期望读取的字节数；若超过可读数量将按 <see
        /// cref="Remaining"/> 截断。
        /// </param>
        /// <returns>指向所读数据的只读跨度。</returns>
        /// <exception cref="InvalidOperationException">
        /// 当没有可读数据（ <see cref="Remaining"/> == 0）时抛出。
        /// </exception>
        public ReadOnlySpan<byte> Read(int byteCount)
        {
            return _block.Read(byteCount);
        }

        /// <summary>
        /// 泛型读取一个值，并将读指针前进相应字节数（数值类型默认按大端序）。
        /// 支持类型：byte/sbyte、short/ushort、int/uint、long/ulong、float、double、decimal、bool、char、Guid、DateTime、DateTimeOffset、TimeSpan、string。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <exception cref="InvalidOperationException">当剩余数据不足以读取目标类型时抛出。</exception>
        /// <exception cref="NotSupportedException">当目标类型不受支持时抛出。</exception>
        public T Read<T>(bool isBigEndian = true)
        {
            var type = typeof(T);
            object? result;

            if (type == typeof(byte))
            {
                result = Read();
            }
            else if (type == typeof(sbyte))
            {
                result = unchecked((sbyte)Read());
            }
            else if (type == typeof(short))
            {
                result = ReadInt16(isBigEndian);
            }
            else if (type == typeof(ushort))
            {
                result = ReadUInt16(isBigEndian);
            }
            else if (type == typeof(int))
            {
                result = ReadInt32(isBigEndian);
            }
            else if (type == typeof(uint))
            {
                result = ReadUInt32(isBigEndian);
            }
            else if (type == typeof(long))
            {
                result = ReadInt64(isBigEndian);
            }
            else if (type == typeof(ulong))
            {
                result = ReadUInt64(isBigEndian);
            }
            else if (type == typeof(float))
            {
                result = ReadSingle(isBigEndian: true);
            }
            else if (type == typeof(double))
            {
                result = ReadDouble(isBigEndian);
            }
            else if (type == typeof(decimal))
            {
                result = ReadDecimal();
            }
            else if (type == typeof(bool))
            {
                result = ReadBoolean();
            }
            else if (type == typeof(char))
            {
                result = ReadChar(); // 默认 UTF-8
            }
            else if (type == typeof(Guid))
            {
                result = ReadGuid();
            }
            else if (type == typeof(DateTime))
            {
                result = ReadDateTime(isBigEndian);
            }
            else if (type == typeof(DateTimeOffset))
            {
                result = ReadDateTimeOffset(isBigEndian);
            }
            else if (type == typeof(TimeSpan))
            {
                result = ReadTimeSpan(isBigEndian);
            }
            else if (type == typeof(string))
            {
                result = ReadString(); // 读取剩余全部为字符串
            }
            else
            {
                throw new NotSupportedException($"ByteBlock.Read<T> 不支持类型: {type.FullName}");
            }

            return (T)result;
        }

        /// <summary>
        /// 读取一个 16 位有符号整数（short），并将读指针前进 2 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 short 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public short ReadInt16(bool isBigEndian = true)
        {
            var span = Read(sizeof(short));
            return isBigEndian
                ? BinaryPrimitives.ReadInt16BigEndian(span)
                : BinaryPrimitives.ReadInt16LittleEndian(span);
        }

        /// <summary>
        /// 读取一个 32 位有符号整数（int），并将读指针前进 4 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 int 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public int ReadInt32(bool isBigEndian = true)
        {
            var span = Read(sizeof(int));
            return isBigEndian
                ? BinaryPrimitives.ReadInt32BigEndian(span)
                : BinaryPrimitives.ReadInt32LittleEndian(span);
        }

        /// <summary>
        /// 读取一个 64 位有符号整数（long），并将读指针前进 8 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 long 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public long ReadInt64(bool isBigEndian = true)
        {
            var span = Read(sizeof(long));
            return isBigEndian
                ? BinaryPrimitives.ReadInt64BigEndian(span)
                : BinaryPrimitives.ReadInt64LittleEndian(span);
        }

        /// <summary>
        /// 读取一个 16 位无符号整数（ushort），并将读指针前进 2 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 ushort 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public ushort ReadUInt16(bool isBigEndian = true)
        {
            var span = Read(sizeof(ushort));
            return isBigEndian
                ? BinaryPrimitives.ReadUInt16BigEndian(span)
                : BinaryPrimitives.ReadUInt16LittleEndian(span);
        }

        /// <summary>
        /// 读取一个 32 位无符号整数（uint），并将读指针前进 4 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 uint 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public uint ReadUInt32(bool isBigEndian = true)
        {
            var span = Read(sizeof(uint));
            return isBigEndian
                ? BinaryPrimitives.ReadUInt32BigEndian(span)
                : BinaryPrimitives.ReadUInt32LittleEndian(span);
        }

        /// <summary>
        /// 读取一个 64 位无符号整数（ulong），并将读指针前进 8 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 ulong 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public ulong ReadUInt64(bool isBigEndian = true)
        {
            var span = Read(sizeof(ulong));
            return isBigEndian
                ? BinaryPrimitives.ReadUInt64BigEndian(span)
                : BinaryPrimitives.ReadUInt64LittleEndian(span);
        }

        /// <summary>
        /// 读取一个 32 位单精度浮点数（float，IEEE 754），并将读指针前进 4 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 float 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public float ReadSingle(bool isBigEndian = true)
        {
            var span = Read(sizeof(float));
            return isBigEndian
                ? BinaryPrimitives.ReadSingleBigEndian(span)
                : BinaryPrimitives.ReadSingleLittleEndian(span);
        }

        /// <summary>
        /// 读取一个 64 位双精度浮点数（double，IEEE 754），并将读指针前进 8 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 double 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public double ReadDouble(bool isBigEndian = true)
        {
            var span = Read(sizeof(double));
            return isBigEndian
                ? BinaryPrimitives.ReadDoubleBigEndian(span)
                : BinaryPrimitives.ReadDoubleLittleEndian(span);
        }

        /// <summary>
        /// 读取一个 <see cref="decimal"/>（16 字节），按四个 Int32（高位在前）组合，并将读指针前进 16 字节。
        /// </summary>
        /// <returns>解析得到的 decimal 值。</returns>
        /// <remarks>内部通过 <see cref="decimal.GetBits(decimal)"/> 的位布局进行还原，使用大端序读取每个 Int32 段。</remarks>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public decimal ReadDecimal()
        {
            Span<int> bits = stackalloc int[4];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = ReadInt32(isBigEndian: true);
            }
            return new decimal(bits);
        }

        /// <summary>
        /// 读取一个布尔值，并将读指针前进 1 字节。
        /// </summary>
        /// <returns>true 当字节非 0；否则 false。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public bool ReadBoolean()
        {
            byte value = Read();
            return value != 0;
        }

        /// <summary>
        /// 按指定编码读取一个字符，并将读指针前进相应字节数。
        /// </summary>
        /// <param name="encoding">字符编码，默认 UTF-8。</param>
        /// <returns>解码得到的字符。</returns>
        /// <exception cref="InvalidOperationException">当无法解码为单个字符时抛出。</exception>
        /// <remarks>
        /// 会读取最多 <c>encoding.GetMaxByteCount(1)</c> 个字节尝试解码一个字符。对于可变长编码（如 UTF-8），
        /// 可能前进的字节数大于该字符实际需要的字节数，请确保调用方的协议边界合理。
        /// </remarks>
        public char ReadChar(Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            int maxByteCount = encoding.GetMaxByteCount(1);
            var byteSpan = Read(maxByteCount);
            int charCount = encoding.GetCharCount(byteSpan);
            Span<char> charSpan = stackalloc char[charCount];
            int actualCharCount = encoding.GetChars(byteSpan, charSpan);
            if (actualCharCount != 1)
                throw new InvalidOperationException("Unable to decode a single character.");
            return charSpan[0];
        }

        /// <summary>
        /// 读取 16 字节并构造 <see cref="Guid"/>，并将读指针前进 16 字节。
        /// </summary>
        /// <returns>解析得到的 Guid。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public Guid ReadGuid()
        {
            var span = Read(16);
            return new Guid(span);
        }

        /// <summary>
        /// 读取一个 <see cref="DateTime"/> 的 Ticks（Int64），并将读指针前进 8 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>基于 ticks 创建的 DateTime，<see cref="DateTime.Kind"/> 为 Unspecified。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public DateTime ReadDateTime(bool isBigEndian = true)
        {
            long ticks = ReadInt64(isBigEndian);
            return new DateTime(ticks);
        }

        /// <summary>
        /// 读取一个 <see cref="DateTimeOffset"/> 的 Ticks（Int64），并将读指针前进 8 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>以零偏移量构造的 DateTimeOffset（不含原始 Offset 信息）。</returns>
        /// <remarks>仅还原 <see cref="DateTimeOffset.Ticks"/>；若协议需要保留偏移量，请单独编码/读取 Offset。</remarks>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public DateTimeOffset ReadDateTimeOffset(bool isBigEndian = true)
        {
            long ticks = ReadInt64(isBigEndian);
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        /// <summary>
        /// 读取一个 <see cref="TimeSpan"/> 的 Ticks（Int64），并将读指针前进 8 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 TimeSpan。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public TimeSpan ReadTimeSpan(bool isBigEndian = true)
        {
            long ticks = ReadInt64(isBigEndian);
            return new TimeSpan(ticks);
        }

        #endregion

        #region FormByteBlock

        /// <summary>
        /// 隐式转换为已写入范围的只读字节 UnreadSpan。
        /// 注意：该切片引用底层缓冲，生命周期应短于本实例且不得在 <see
        /// cref="Dispose"/> 后使用。
        /// </summary>
        /// <param name="block">源实例。</param>
        public static implicit operator ReadOnlySpan<byte>(in ByteBlock block)
            => block.UnreadSpan;

        /// <summary>
        /// 隐式转换为已写入范围的只读字节 UnreadMemory。
        /// 注意：该切片引用底层缓冲，生命周期应短于本实例且不得在 <see
        /// cref="Dispose"/> 后使用。
        /// </summary>
        /// <param name="block">源实例。</param>
        public static implicit operator ReadOnlyMemory<byte>(in ByteBlock block)
            => block.UnreadMemory;

        public static explicit operator ByteBuffer(in ByteBlock block)
            => new ByteBuffer(block.UnreadMemory);

        #endregion FormByteBlock

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

        #endregion ToByteBlock
    }
}