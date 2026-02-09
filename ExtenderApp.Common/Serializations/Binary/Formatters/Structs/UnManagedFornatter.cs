using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 非托管结构体的二进制格式化器基类（支持到 8 字节内的 unmanaged 类型）。
    /// 根据目标类型在运行时选择两种序列化路径：基于整数位拷贝的 long 路径或基于数值的 double 路径（用于 float/double）。
    /// 采用类型标记 + 紧凑数据长度的编码格式以减少存储开销。
    /// </summary>
    /// <typeparam name="T">目标结构体类型，必须为 unmanaged 且推荐大小不超过 8 字节。</typeparam>
    internal sealed class UnManagedFornatter<T> : BinaryFormatter<T> where T : unmanaged
    {
        /// <summary>
        /// 序列化委托签名：将值序列化写入 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <param name="buffer">目标字节缓冲区。</param>
        /// <param name="value">待序列化的值。</param>
        private delegate void SerializeDelegate(ref ByteBuffer buffer, T value);

        /// <summary>
        /// 反序列化委托签名：从 <see cref="ByteBuffer"/> 读取并返回值。
        /// </summary>
        /// <param name="buffer">来源字节缓冲区。</param>
        /// <returns>反序列化得到的值。</returns>
        private delegate T DeserializeDelegate(ref ByteBuffer buffer);

        /// <summary>
        /// 计算序列化长度委托签名（返回包含类型标记的总字节数）。
        /// </summary>
        /// <param name="value">待测长度的值。</param>
        /// <returns>序列化后的总字节数。</returns>
        private delegate long GetLengthDelegate(T value);

        private readonly SerializeDelegate _serializeInvoker;
        private readonly DeserializeDelegate _deserializeInvoker;
        private readonly GetLengthDelegate _getLengthInvoker;

        /// <summary>
        /// 按 mark 索引的固定数据长度表（不含类型标记字节）。
        /// 数组长度为 256（byte 的取值范围）；未定义的 mark 使用默认 0 表示不可识别。
        /// 由静态初始化函数填充常见数值类型的字节长度，以便快速通过 mark 获取数据长度。
        /// </summary>
        private static readonly byte[] LengthArray = CreatLengthArray();

        /// <summary>
        /// 构造并返回长度查表数组（长度 256），表项为对应标记的数据长度（以字节为单位，不包含类型标记本身）。
        /// 未识别的 mark 保持为 0。
        /// </summary>
        /// <returns>长度为 256 的字节数组。</returns>
        private static byte[] CreatLengthArray()
        {
            // byte 的取值范围 0..255，需要 256 个槽
            var arr = new byte[byte.MaxValue + 1];

            // 浮点类型
            arr[BinaryOptions.Float32] = (byte)GetSize<Single>();
            arr[BinaryOptions.Float64] = (byte)GetSize<Double>();

            // 无符号整数
            arr[BinaryOptions.UInt8] = (byte)GetSize<Byte>();
            arr[BinaryOptions.UInt16] = (byte)GetSize<UInt16>();
            arr[BinaryOptions.UInt32] = (byte)GetSize<UInt32>();
            arr[BinaryOptions.UInt64] = (byte)GetSize<UInt64>();

            // 有符号整数
            arr[BinaryOptions.Int8] = (byte)GetSize<SByte>();
            arr[BinaryOptions.Int16] = (byte)GetSize<Int16>();
            arr[BinaryOptions.Int32] = (byte)GetSize<Int32>();
            arr[BinaryOptions.Int64] = (byte)GetSize<Int64>();

            // 其他标记（如 String/TArray/MapHeader）不是固定数据长度，不在表中填充
            return arr;
        }

        private readonly byte _mark;

        /// <summary>
        /// 默认的序列化字节长度（类型标记 + 数据），基于泛型类型 T 的静态大小计算。
        /// </summary>
        public override int DefaultLength { get; }

        /// <summary>
        /// 构造函数：根据泛型 <typeparamref name="T"/> 决定使用哪条序列化/反序列化路径并初始化委托缓存。
        /// </summary>
        public UnManagedFornatter()
        {
            // 初始化分发委托：浮点走 double 路径，其他类型走 long 路径
            if (typeof(T) == typeof(double) || typeof(T) == typeof(float))
            {
                _serializeInvoker = SerializeAsDouble;
                _deserializeInvoker = DeserializeAsDouble;
                _getLengthInvoker = GetLengthAsDouble;
            }
            else
            {
                _serializeInvoker = SerializeAsLong;
                _deserializeInvoker = DeserializeAsLong;
                _getLengthInvoker = GetLengthAsLong;
            }

            DefaultLength = GetLength<T>();
            _mark = BinaryOptions.GetByteByType<T>();
        }

        /// <summary>
        /// 从 <paramref name="buffer"/> 反序列化出类型 <typeparamref name="T"/> 的值。
        /// 在读取前会校验缓冲内的类型标记是否可安全转换为目标类型。
        /// </summary>
        /// <param name="buffer">来源字节缓冲区（以 ref 传入，读取会改变内部位置）。</param>
        /// <returns>反序列化得到的 <typeparamref name="T"/> 值。</returns>
        /// <exception cref="InvalidOperationException">当缓冲内的标记表示的类型比目标类型更大且不可安全转换时抛出。</exception>
        public override T Deserialize(ref ByteBuffer buffer)
        {
            byte mark = buffer.NextCode;
            if (GetSizeByMark(mark) > GetSizeByMark(_mark))
                throw new InvalidOperationException($"无法将数据内{BinaryOptions.GetNameByMark(mark)}类型转换成{BinaryOptions.GetNameByMark(_mark)}");

            return _deserializeInvoker(ref buffer);
        }

        /// <summary>
        /// 将值序列化写入到 <paramref name="buffer"/> 中。
        /// </summary>
        /// <param name="buffer">目标字节缓冲区（以 ref 传入，将被写入数据）。</param>
        /// <param name="value">要序列化的值。</param>
        public override void Serialize(ref ByteBuffer buffer, T value)
        {
            _serializeInvoker(ref buffer, value);
        }

        /// <summary>
        /// 计算给定 <paramref name="value"/> 序列化后的总字节数（包含类型标记）。
        /// </summary>
        /// <param name="value">要计算长度的值。</param>
        /// <returns>序列化后的总字节数（长整型）。</returns>
        public override long GetLength(T value)
        {
            return _getLengthInvoker(value);
        }

        #region Double 路径 (支持浮点压缩)

        /// <summary>
        /// 将浮点类型（float/double）按 float32/float64 最紧凑规则写入缓冲区。
        /// 使用位拷贝把 <typeparamref name="T"/> 写入到 <see cref="double"/> 的内存表示中，再走 double 写入逻辑。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="value">待序列化的值（T 为 float 或 double）。</param>
        private void SerializeAsDouble(ref ByteBuffer buffer, T value)
        {
            double data = 0d;
            Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref data), value);
            Write(ref buffer, data);
        }

        /// <summary>
        /// 从缓冲区读取并恢复为 <typeparamref name="T"/>。支持从 integer/float 标识读取并还原为浮点数位或数值。
        /// </summary>
        /// <param name="buffer">来源缓冲区。</param>
        /// <returns>恢复后的 <typeparamref name="T"/> 实例。</returns>
        private T DeserializeAsDouble(ref ByteBuffer buffer)
        {
            byte mark = buffer.Read();
            double data = ReadAsDouble(ref buffer, mark);
            return Unsafe.ReadUnaligned<T>(ref Unsafe.As<double, byte>(ref data));
        }

        /// <summary>
        /// 计算浮点类型值的序列化长度（含类型标记），会在可能且无损的情况下把 double 压缩为 float32。
        /// </summary>
        /// <param name="value">要计算长度的值（T 为 float 或 double）。</param>
        /// <returns>序列化后的总字节数。</returns>
        private long GetLengthAsDouble(T value)
        {
            double data = 0;
            Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref data), value);
            return GetLength(data, out _);
        }

        #endregion Double 路径 (支持浮点压缩)

        #region Long 路径 (位拷贝 + 整数压缩)

        /// <summary>
        /// 将任意非浮点 unmanaged 类型按位拷贝到 long 的低位后使用整数压缩写入。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="value">待序列化的值（T 非浮点类型）。</param>
        private void SerializeAsLong(ref ByteBuffer buffer, T value)
        {
            long data = 0;
            Unsafe.WriteUnaligned(ref Unsafe.As<long, byte>(ref data), value);
            Write(ref buffer, data);
        }

        /// <summary>
        /// 从缓冲区按整数标识读取并把位模式写回到 <typeparamref name="T"/> 实例。
        /// </summary>
        /// <param name="buffer">来源缓冲区。</param>
        /// <returns>恢复后的 <typeparamref name="T"/> 实例。</returns>
        private T DeserializeAsLong(ref ByteBuffer buffer)
        {
            byte mark = buffer.Read();
            long data = ReadAsLong(ref buffer, mark);
            return Unsafe.ReadUnaligned<T>(ref Unsafe.As<long, byte>(ref data));
        }

        /// <summary>
        /// 计算按 long 路径序列化的长度（含类型标记）。
        /// </summary>
        /// <param name="value">要计算长度的值。</param>
        /// <returns>序列化后的总字节数。</returns>
        private long GetLengthAsLong(T value)
        {
            long data = 0;
            Unsafe.WriteUnaligned(ref Unsafe.As<long, byte>(ref data), value);
            return GetLength(data, out _);
        }

        #endregion Long 路径 (位拷贝 + 整数压缩)

        #region Read

        /// <summary>
        /// 读取并返回用于浮点路径的 double 值（支持 float32/float64 及整数压缩还原）。
        /// </summary>
        /// <param name="buffer">来源缓冲区。</param>
        /// <param name="mark">类型标记。</param>
        /// <returns>还原为 double 的数值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ReadAsDouble(ref ByteBuffer buffer, byte mark)
        {
            if (mark == BinaryOptions.Float32) return Read<float>(ref buffer);
            if (mark == BinaryOptions.Float64) return Read<double>(ref buffer);
            // 允许浮点从压缩的整数读取并数值还原
            return ReadAsLong(ref buffer, mark);
        }

        /// <summary>
        /// 根据类型标记从缓冲区读取并返回 64 位整数表示（按位或数值填充到 long）。
        /// </summary>
        /// <param name="buffer">来源缓冲区。</param>
        /// <param name="mark">类型标记。</param>
        /// <returns>以 long 表示的读取结果（保持位模式/数值一致性）。</returns>
        /// <exception cref="InvalidDataException">当标识无法映射为 64 位整数时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ReadAsLong(ref ByteBuffer buffer, byte mark)
        {
            if (mark == BinaryOptions.Int8) return (sbyte)buffer.Read();
            if (mark == BinaryOptions.UInt8) return buffer.Read();
            if (mark == BinaryOptions.Int16) return Read<short>(ref buffer);
            if (mark == BinaryOptions.UInt16) return Read<ushort>(ref buffer);
            if (mark == BinaryOptions.Int32) return Read<int>(ref buffer);
            if (mark == BinaryOptions.UInt32) return Read<uint>(ref buffer);
            if (mark == BinaryOptions.Int64 ||
                mark == BinaryOptions.UInt64) return Read<long>(ref buffer);
            throw new InvalidDataException($"标识 {mark} 无法读取为 64 位整数。");
        }

        /// <summary>
        /// 从缓冲区读取指定类型的原始字节并转换为对应的值类型。
        /// </summary>
        /// <typeparam name="TValue">目标值类型（unmanaged）。</typeparam>
        /// <param name="buffer">来源缓冲区。</param>
        /// <returns>读取并转换得到的 <typeparamref name="TValue"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue Read<TValue>(ref ByteBuffer buffer) where TValue : unmanaged
        {
            int size = GetSize<TValue>();
            Span<byte> span = stackalloc byte[size];
            buffer.Read(span);
            if (BitConverter.IsLittleEndian)
            {
                span.Reverse();
            }

            return MemoryMarshal.Read<TValue>(span);
        }

        #endregion Read

        #region Write

        /// <summary>
        /// 将 <see cref="double"/> 按最紧凑格式写入缓冲区（可能写 float32 或 float64）。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="value">要写入的 double 值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Write(ref ByteBuffer buffer, double value)
        {
            GetLength(value, out var mark);
            if (mark == BinaryOptions.Float32) Write(ref buffer, (float)value);
            else Write(ref buffer, value);
        }

        /// <summary>
        /// 将 long 按最紧凑整数格式写入缓冲区（包含类型标记）。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="value">要写入的 long 值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Write(ref ByteBuffer buffer, long value)
        {
            GetLength(value, out var mark);
            buffer.Write(mark);

            if (mark == BinaryOptions.Int8 || mark == BinaryOptions.UInt8) Write(ref buffer, (byte)value);
            else if (mark == BinaryOptions.Int16) Write(ref buffer, (short)value);
            else if (mark == BinaryOptions.UInt16) Write(ref buffer, (ushort)value);
            else if (mark == BinaryOptions.Int32) Write(ref buffer, (int)value);
            else if (mark == BinaryOptions.UInt32) Write(ref buffer, (uint)value);
            else if (mark == BinaryOptions.UInt64) Write(ref buffer, (ulong)value);
            else if (mark == BinaryOptions.Int64) Write(ref buffer, value);
            else Write(ref buffer, (ulong)value);
        }

        /// <summary>
        /// 将值类型字节写入缓冲区（不包含类型标记，由调用方负责）。
        /// </summary>
        /// <typeparam name="TValue">要写入的值类型（unmanaged）。</typeparam>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="value">要写入的值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Write<TValue>(ref ByteBuffer buffer, TValue value)
            where TValue : unmanaged
        {
            int size = GetSize<TValue>();
            Span<byte> span = buffer.GetSpan(size);
            MemoryMarshal.Write(span, value);
            buffer.Advance(size);

            if (BitConverter.IsLittleEndian)
            {
                span.Reverse();
            }
        }

        #endregion Write

        #region GetLength

        /// <summary>
        /// 获取 double 按最紧凑格式编码所需字节数并返回使用的标识（如能无损降为 float32 则使用 float32 标识）。
        /// </summary>
        /// <param name="value">要计算的 double 值。</param>
        /// <param name="mark">输出所使用的类型标记。</param>
        /// <returns>序列化所需的总字节数（包含类型标记）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetLength(double value, out byte mark)
        {
            if (value < float.MaxValue && value > float.MinValue && (double)(float)value == value)
            {
                mark = BinaryOptions.Float32;
                return GetLength<float>();
            }
            else
            {
                mark = BinaryOptions.Float64;
                return GetLength<double>();
            }
        }

        /// <summary>
        /// 获取 long 按最紧凑整数编码所需字节数并返回使用的标识。
        /// </summary>
        /// <param name="value">要计算的 long 值。</param>
        /// <param name="mark">输出所使用的类型标记。</param>
        /// <returns>序列化所需的总字节数（包含类型标记）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetLength(long value, out byte mark)
        {
            if (value > 0)
            {
                return GetLength((ulong)value, out mark);
            }
            else if (value > SByte.MinValue)
            {
                mark = BinaryOptions.Int8;
                return GetLength<SByte>();
            }
            else if (value > Int16.MinValue)
            {
                mark = BinaryOptions.Int16;
                return GetLength<Int16>();
            }
            else if (value > Int32.MinValue)
            {
                mark = BinaryOptions.Int32;
                return GetLength<Int32>();
            }
            else
            {
                mark = BinaryOptions.Int64;
                return GetLength<Int64>();
            }
        }

        /// <summary>
        /// 获取 ulong 按最紧凑整数编码所需字节数并返回使用的标识。
        /// </summary>
        /// <param name="value">要计算的 ulong 值。</param>
        /// <param name="mark">输出所使用的类型标记。</param>
        /// <returns>序列化所需的总字节数（包含类型标记）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetLength(ulong value, out byte mark)
        {
            if (value <= Byte.MaxValue)
            {
                mark = BinaryOptions.UInt8;
                return GetLength<Byte>();
            }
            else if (value <= UInt16.MaxValue)
            {
                mark = BinaryOptions.UInt16;
                return GetLength<UInt16>();
            }
            else if (value <= UInt32.MaxValue)
            {
                mark = BinaryOptions.UInt32;
                return GetLength<UInt32>();
            }
            else
            {
                mark = BinaryOptions.UInt64;
                return GetLength<UInt64>();
            }
        }

        /// <summary>
        /// 获取指定数值类型的序列化总字节数（类型标记 + 数据）。
        /// </summary>
        /// <typeparam name="TValue">目标数值类型。</typeparam>
        /// <returns>序列化所需的总字节数（包含 1 字节类型标记）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetLength<TValue>()
        {
            return GetSize<TValue>() + 1;
        }

        #endregion GetLength

        /// <summary>
        /// 根据 mark 快速返回该 mark 对应的数据字节大小（不含类型标记字节）。
        /// 若 mark 未在固定表中定义，则抛出 <see cref="InvalidOperationException"/>。
        /// </summary>
        /// <param name="mark">类型标记字节。</param>
        /// <returns>对应的数据字节大小。</returns>
        /// <exception cref="InvalidOperationException">当 mark 无效或未映射到已知类型大小时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetSizeByMark(byte mark)
        {
            int result = LengthArray[mark];
            if (result <= 0)
                throw new InvalidOperationException($"标识 {mark} 无法映射为已知类型大小。");

            return result;
        }

        /// <summary>
        /// 获取指定类型的字节大小（不包含类型标记）。
        /// </summary>
        /// <typeparam name="TValue">目标类型。</typeparam>
        /// <returns>指定类型占用的字节数。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetSize<TValue>()
        {
            return Unsafe.SizeOf<TValue>();
        }
    }
}