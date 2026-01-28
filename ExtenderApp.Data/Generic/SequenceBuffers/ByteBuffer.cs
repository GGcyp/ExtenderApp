using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

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
        public static ByteBuffer CreateBuffer() => new ByteBuffer(SequencePool<byte>.Shared);

        /// <summary>
        /// 内部泛型块实现，封装实际的写入/读取逻辑。
        /// </summary>
        private SequenceBuffer<byte> _buffer;

        /// <summary>
        /// 当前绑定的只读序列快照。
        /// </summary>
        public ReadOnlySequence<byte> Sequence => _buffer.Sequence;

        /// <summary>
        /// 获取当前持有的序列租约（若有）。
        /// </summary>
        public SequencePool<byte>.SequenceRental Rental => _buffer.Rental;

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
        /// 未读取的只读序列快照。
        /// </summary>
        public ReadOnlySequence<byte> UnreadSequence => _buffer.UnreadSequence;

        /// <summary>
        /// 获取当前读取器实例。
        /// </summary>
        public SequenceReader<byte> Reader => _buffer.Reader;

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
        /// 创建一个空的字节缓冲实例。
        /// 需要创建可用的字节缓存，请使用其他构造函数。
        /// 例如<see cref="CreateBuffer"/>
        /// </summary>
        public ByteBuffer()
        {
        }

        /// <summary>
        /// 复制构造函数，创建一个包含另一个 <see cref="ByteBuffer"/> 实例内容副本的新实例。
        /// </summary>
        /// <param name="buffer">要复制其内容的源字节缓冲。</param>
        public ByteBuffer(ByteBuffer buffer) : this(buffer._buffer)
        {
        }

        /// <summary>
        /// 通过已有的 <see cref="SequenceBuffer{T}"/> 构造。
        /// </summary>
        /// <param name="buffer">已有的缓冲</param>
        public ByteBuffer(SequenceBuffer<byte> buffer) : this(SequencePool<byte>.Shared)
        {
            Write(buffer);
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
        /// 通过 <see cref="SequenceReader{T}"/> 构造，创建一个只读的 <see cref="ByteBuffer"/> 实例。
        /// </summary>
        /// <param name="reader">序列读取器。</param>
        public ByteBuffer(SequenceReader<byte> reader)
        {
            _buffer = new SequenceBuffer<byte>(reader);
        }

        /// <summary>
        /// 申请一个可写的 <see cref="Span{T}"/>，用于直接写入。
        /// 申请写缓冲后会使读取视图变脏，下一次读取将刷新。
        /// </summary>
        /// <param name="sizeHint">期望大小（提示值，可为 0）。</param>
        /// <exception cref="ObjectDisposedException">当未持有可写序列时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            return _buffer.GetMemory(sizeHint);
        }

        #region Write

        /// <summary>
        /// 追加单个元素。
        /// </summary>
        /// <param name="value">要追加的元素。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            _buffer.Write(value);
        }

        /// <summary>
        /// 追加一个字节数组。
        /// </summary>
        /// <param name="bytes">要追加的字节数组。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            _buffer.Write(bytes.AsMemory());
        }

        /// <summary>
        /// 追加一个字节数组的数组。
        /// </summary>
        /// <param name="bytes">要追加的字节数组的数组。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[][] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            foreach (var item in bytes)
            {
                _buffer.Write(item.AsMemory());
            }
        }

        /// <summary>
        /// 追加一个 <see cref="ByteBlock"/> 的未读数据。
        /// </summary>
        /// <param name="block">要追加的字节块。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ByteBlock block)
        {
            Write(block.UnreadSpan);
        }

        /// <summary>
        /// 追加一段只读跨度数据。
        /// </summary>
        /// <param name="value">要追加的数据。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(scoped ReadOnlySpan<byte> value)
        {
            _buffer.Write(value);
        }

        /// <summary>
        /// 追加一段只读内存数据。
        /// </summary>
        /// <param name="value">要追加的数据。</param>
       	[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlyMemory<byte> value)
        {
            _buffer.Write(value);
        }

        /// <summary>
        /// 追加一段只读序列数据。
        /// </summary>
        /// <param name="value">要追加的数据。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySequence<byte> value)
        {
            _buffer.Write(value);
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

        #endregion Write

        #region Read

        /// <summary>
        /// 从当前位置尝试读取一个元素并前进。
        /// </summary>
        /// <param name="value">输出读取到的元素。</param>
        /// <returns>读取成功返回 true，否则 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead(int count, out ReadOnlySequence<byte> value)
        {
            return _buffer.TryRead(count, out value);
        }

        /// <summary>
        /// 读取当前未读的第一个字节，并将读指针前进 1（不影响写指针）。
        /// </summary>
        /// <returns>读取到的字节。</returns>
        /// <exception cref="InvalidOperationException">
        /// 当没有可读数据（Remaining == 0）时抛出。
        /// </exception>
        public byte Read()
        {
            if (!_buffer.TryRead(out byte value))
            {
                return 0;
            }
            return value;
        }

        /// <summary>
        /// 读取最多 <paramref name="destination"/>.Length 个未读字节到目标跨度，并将读指针前进相应长度（不影响写指针）。
        /// </summary>
        /// <param name="destination">接收数据的目标跨度。</param>
        /// <returns>实际读取的字节数。</returns>
        /// <exception cref="InvalidOperationException">
        /// 当没有可读数据（Remaining == 0）时抛出。
        /// </exception>
        public int Read(Span<byte> destination)
            => _buffer.Read(destination);

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
            => _buffer.Read(destination);

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
        public ByteBlock Read(int byteCount)
        {
            int length = Math.Min(byteCount, (int)Remaining);
            ByteBlock block = new(length);
            var span = block.GetSpan(length);
            Read(span.Slice(0, length));
            block.WriteAdvance(length);
            return block;
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
            object result = type switch
            {
                _ when type == typeof(byte) => Read(),
                _ when type == typeof(sbyte) => unchecked((sbyte)Read()),
                _ when type == typeof(short) => ReadInt16(isBigEndian),
                _ when type == typeof(ushort) => ReadUInt16(isBigEndian),
                _ when type == typeof(int) => ReadInt32(isBigEndian),
                _ when type == typeof(uint) => ReadUInt32(isBigEndian),
                _ when type == typeof(long) => ReadInt64(isBigEndian),
                _ when type == typeof(ulong) => ReadUInt64(isBigEndian),
                _ when type == typeof(float) => ReadSingle(isBigEndian),
                _ when type == typeof(double) => ReadDouble(isBigEndian),
                _ when type == typeof(decimal) => ReadDecimal(),
                _ when type == typeof(bool) => ReadBoolean(),
                _ when type == typeof(char) => ReadChar(), // 默认 UTF-8
                _ when type == typeof(Guid) => ReadGuid(),
                _ when type == typeof(DateTime) => ReadDateTime(isBigEndian),
                _ when type == typeof(DateTimeOffset) => ReadDateTimeOffset(isBigEndian),
                _ when type == typeof(TimeSpan) => ReadTimeSpan(isBigEndian),
                _ when type == typeof(string) => ReadString(), // 读取剩余全部为字符串
                _ => throw new NotSupportedException($"ByteBuffer.Read<T> 不支持类型: {type.FullName}")
            };

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
            var block = Read(sizeof(short));
            var result = block.ReadInt16(isBigEndian);
            block.Dispose();
            return result;
        }

        /// <summary>
        /// 读取一个 32 位有符号整数（int），并将读指针前进 4 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 int 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public int ReadInt32(bool isBigEndian = true)
        {
            var block = Read(sizeof(int));
            var result = block.ReadInt32(isBigEndian);
            block.Dispose();
            return result;
        }

        /// <summary>
        /// 读取一个 64 位有符号整数（long），并将读指针前进 8 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 long 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public long ReadInt64(bool isBigEndian = true)
        {
            var block = Read(sizeof(long));
            var result = block.ReadInt64(isBigEndian);
            block.Dispose();
            return result;
        }

        /// <summary>
        /// 读取一个 16 位无符号整数（ushort），并将读指针前进 2 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 ushort 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public ushort ReadUInt16(bool isBigEndian = true)
        {
            var block = Read(sizeof(ushort));
            var result = block.ReadUInt16(isBigEndian);
            block.Dispose();
            return result;
        }

        /// <summary>
        /// 读取一个 32 位无符号整数（uint），并将读指针前进 4 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 uint 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public uint ReadUInt32(bool isBigEndian = true)
        {
            var block = Read(sizeof(uint));
            var result = block.ReadUInt32(isBigEndian);
            block.Dispose();
            return result;
        }

        /// <summary>
        /// 读取一个 64 位无符号整数（ulong），并将读指针前进 8 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 ulong 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public ulong ReadUInt64(bool isBigEndian = true)
        {
            var block = Read(sizeof(ulong));
            var result = block.ReadUInt64(isBigEndian);
            block.Dispose();
            return result;
        }

        /// <summary>
        /// 读取一个 32 位单精度浮点数（float，IEEE 754），并将读指针前进 4 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 float 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public float ReadSingle(bool isBigEndian = true)
        {
            var block = Read(sizeof(float));
            var result = block.ReadSingle(isBigEndian);
            block.Dispose();
            return result;
        }

        /// <summary>
        /// 读取一个 64 位双精度浮点数（double，IEEE 754），并将读指针前进 8 字节。
        /// </summary>
        /// <param name="isBigEndian">是否按大端序解码（默认大端）。</param>
        /// <returns>解析得到的 double 值。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public double ReadDouble(bool isBigEndian = true)
        {
            var block = Read(sizeof(double));
            var result = block.ReadDouble(isBigEndian);
            block.Dispose();
            return result;
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
        public char ReadChar(Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            int maxByteCount = encoding.GetMaxByteCount(1);
            var block = Read(maxByteCount);
            var result = block.ReadChar(encoding);
            block.Dispose();
            return result;
        }

        /// <summary>
        /// 读取 16 字节并构造 <see cref="Guid"/>，并将读指针前进 16 字节。
        /// </summary>
        /// <returns>解析得到的 Guid。</returns>
        /// <exception cref="InvalidOperationException">当没有可读数据时抛出。</exception>
        public Guid ReadGuid()
        {
            var block = Read(16);
            var result = block.ReadGuid();
            block.Dispose();
            return result;
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
            return ReadString((int)Remaining, encoding);
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
            var block = Read(byteCount);
            var result = block.ReadString(encoding);
            block.Dispose();
            return result;
        }

        #endregion Read

        #region CopyTo

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
        /// 尝试将剩余数据复制到<see cref="ByteBuffer"/>（不改变读取位置）。
        /// </summary>
        /// <param name="block">目标缓存</param>
        /// <returns>复制成功返回 true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(ref ByteBuffer buffer)
        {
            if (!buffer.CanWrite)
                return false;

            int length = (int)Length;
            if (TryCopyTo(buffer.GetSpan(length).Slice(0, length)))
            {
                buffer.WriteAdvance(length);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试将剩余数据复制到<see cref="Stream"/>（不改变读取位置）。
        /// </summary>
        /// <param name="stream">目标流</param>
        /// <returns>复制成功返回 true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (IsEmpty)
                throw new Exception("当前字节缓冲无任何数据，无法复制。");

            while (!End)
            {
                ReadOnlySpan<byte> span = UnreadSpan;
                stream.Write(span);
                ReadAdvance(span.Length);
            }

            return true;
        }

        #endregion CopyTo

        /// <summary>
        /// 尝试预览一个元素（不前进）。
        /// </summary>
        /// <param name="value">输出预览到的元素。</param>
        /// <returns>预览成功返回 true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
       	[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(long offset, out byte value)
        {
            return _buffer.TryPeek(offset, out value);
        }

        /// <summary>
        /// 将读取位置定位到指定位置。
        /// </summary>
        /// <param name="pos">指定位置</param>
        /// <exception cref="ArgumentOutOfRangeException">当位置小于0或大于已写入长度时触发</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Seek(long pos)
        {
            _buffer.Seek(pos);
        }

        /// <summary>
        /// 将读取位置回退指定数量。
        /// </summary>
        /// <param name="count">回退的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public ByteBuffer CreatePeekBuffer() => _buffer.Reader;

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

        /// <summary>
        /// 如果当前缓冲不可写入，则抛出 <see cref="ObjectDisposedException"/>。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfCannotWrite()
        {
            if (!CanWrite)
                throw new ObjectDisposedException("当前字节缓冲不可写入。");
        }

        #region FormByteBuffer

        public static implicit operator ReadOnlySequence<byte>(in ByteBuffer buffer)
            => buffer.UnreadSequence;

        public static implicit operator SequenceReader<byte>(in ByteBuffer buffer)
            => buffer._buffer;

        #endregion FormByteBuffer

        #region ToByteBuffer

        public static implicit operator ByteBuffer(in SequenceBuffer<byte> buffer)
            => new ByteBuffer(buffer);

        public static explicit operator ByteBuffer(ReadOnlySequence<byte> sequence)
            => new ByteBuffer(sequence);

        public static implicit operator ByteBuffer(SequencePool<byte> pool)
            => new ByteBuffer(pool);

        public static implicit operator ByteBuffer(SequenceReader<byte> reader)
            => new ByteBuffer(reader);

        public static explicit operator ByteBuffer(ReadOnlyMemory<byte> memory)
            => new ByteBuffer(memory);

        #endregion ToByteBuffer
    }
}