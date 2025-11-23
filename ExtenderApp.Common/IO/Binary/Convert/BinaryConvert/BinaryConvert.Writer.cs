using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary
{
    /// <summary>
    /// BinaryConvert 类的一部分，提供将基础类型与容器头编码为二进制（兼容 MessagePack）的方法。
    /// </summary>
    /// <remarks>
    /// 约定：
    /// - 多字节数值统一使用大端（网络序）写入。
    /// - 所有 TryWrite* 方法在 <paramref name="destination"/> 空间不足时返回 false，不进行部分写入；
    ///   <paramref name="bytesWritten"/> 返回本次编码所需的总字节数（调用方可据此扩容）。
    /// - 写入容器头（数组/映射/二进制/字符串/扩展）只写“头部”，不包含随后的有效载荷。
    /// - 对整数采用最紧凑的编码：优先使用 fixint/fixstr/fixarray/fixmap 等固定格式，超出范围则使用对应的 8/16/32/64 位格式。
    /// </remarks>
    public partial class BinaryConvert
    {
        /// <summary>
        /// 尝试将 Nil（空值）类型码写入目标缓冲区。
        /// </summary>
        /// <param name="destination">目标字节数组。</param>
        /// <param name="bytesWritten">所需/实际写入的总字节数（恒为 1）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWriteNil(Span<byte> destination, out int bytesWritten)
        {
            bytesWritten = BinaryLength.Nil;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.Nil;
            return true;
        }

        /// <summary>
        /// 尝试写入数组头（不包含数组元素）。
        /// </summary>
        /// <param name="destination">目标字节数组。</param>
        /// <param name="count">数组元素个数。</param>
        /// <param name="bytesWritten">所需/实际写入的总字节数（1/3/5）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        /// <remarks>
        /// 编码选择：fixarray（≤15）/ array16（≤65535）/ array32。
        /// </remarks>
        public bool TryWriteArrayHeader(Span<byte> destination, uint count, out int bytesWritten)
        {
            if (count <= BinaryRang.MaxFixArrayCount)
            {
                bytesWritten = 1;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = (byte)(BinaryCode.MinFixArray | count);
                return true;
            }
            else if (count <= ushort.MaxValue)
            {
                bytesWritten = 3;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = BinaryCode.Array16;
                WriteBigEndian(destination.Slice(1), (ushort)count);
                return true;
            }

            bytesWritten = 5;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.Array32;
            WriteBigEndian(destination.Slice(1), count);
            return true;
        }

        /// <summary>
        /// 尝试写入映射头（不包含键值对实体）。
        /// </summary>
        /// <param name="destination">目标字节数组。</param>
        /// <param name="count">映射项个数。</param>
        /// <param name="bytesWritten">所需/实际写入的总字节数（1/3/5）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        /// <remarks>
        /// 编码选择：fixmap（≤15）/ map16（≤65535）/ map32。
        /// </remarks>
        public bool TryWriteMapHeader(Span<byte> destination, uint count, out int bytesWritten)
        {
            if (count <= BinaryRang.MaxFixMapCount)
            {
                bytesWritten = 1;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = (byte)(BinaryCode.MinFixMap | count);
                return true;
            }
            else if (count <= ushort.MaxValue)
            {
                bytesWritten = BinaryLength.UInt16;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = BinaryCode.Map16;
                WriteBigEndian(destination.Slice(1), (ushort)count);
                return true;
            }

            bytesWritten = 5;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.Map32;
            WriteBigEndian(destination.Slice(1), count);
            return true;
        }

        /// <summary>
        /// 尝试写入正固定整数（positive fixint），不进行范围验证。
        /// </summary>
        /// <param name="destination">目标字节数组。</param>
        /// <param name="value">0..127 的数值（调用方需保证范围正确）。</param>
        /// <param name="bytesWritten">所需/实际写入的总字节数（1）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        private bool TryWriteFixIntUnsafe(Span<byte> destination, byte value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.Byte;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = unchecked(value);
            return true;
        }

        /// <summary>
        /// 尝试写入二进制数据（bin）的头部，不包含数据体。
        /// </summary>
        /// <param name="destination">目标字节数组。</param>
        /// <param name="length">二进制数据长度（字节）。</param>
        /// <param name="bytesWritten">所需/实际写入的总字节数（2/3/5）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        /// <remarks>
        /// 编码选择：bin8（≤255）/ bin16（≤65535）/ bin32。
        /// </remarks>
        public bool TryWriteBinHeader(Span<byte> destination, uint length, out int bytesWritten)
        {
            switch (length)
            {
                case <= byte.MaxValue:
                    bytesWritten = BinaryLength.Byte;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryCode.Bin8;
                    destination[1] = (byte)length;
                    return true;
                case <= UInt16.MaxValue:
                    bytesWritten = BinaryLength.UInt16;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryCode.Bin16;
                    WriteBigEndian(destination.Slice(1), (ushort)length);
                    return true;
                default:
                    bytesWritten = BinaryLength.UInt32;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryCode.Bin32;
                    WriteBigEndian(destination.Slice(1), length);
                    return true;
            }
        }

        /// <summary>
        /// 尝试写入字符串（str）的头部，不包含字符串字节。
        /// </summary>
        /// <param name="destination">目标字节数组。</param>
        /// <param name="byteCount">按目标编码后的字符串字节数（非字符数）。</param>
        /// <param name="bytesWritten">所需/实际写入的总字节数（1/2/3/5）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        /// <remarks>
        /// 编码选择：fixstr（≤31）/ str8（≤255）/ str16（≤65535）/ str32。
        /// </remarks>
        public bool TryWriteStringHeader(Span<byte> destination, uint byteCount, out int bytesWritten)
        {
            if (byteCount <= BinaryRang.MaxFixStringLength)
            {
                bytesWritten = 1;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = (byte)(BinaryCode.MinFixStr | byteCount);
                return true;
            }
            else if (byteCount <= byte.MaxValue)
            {
                bytesWritten = 2;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = BinaryCode.Str8;
                destination[1] = unchecked((byte)byteCount);
                return true;
            }
            else if (byteCount <= ushort.MaxValue)
            {
                bytesWritten = 3;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = BinaryCode.Str16;
                WriteBigEndian(destination.Slice(1), (ushort)byteCount);
                return true;
            }

            bytesWritten = 5;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.Str32;
            WriteBigEndian(destination.Slice(1), byteCount);
            return true;
        }

        /// <summary>
        /// 尝试写入扩展类型（ext）的头部，不包含扩展数据体。
        /// </summary>
        /// <param name="destination">目标字节数组。</param>
        /// <param name="extensionHeader">扩展头部信息（类型码与长度）。</param>
        /// <param name="bytesWritten">所需/实际写入的总字节数（2/3/4/6）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        /// <remarks>
        /// 编码选择：fixext 1/2/4/8/16；否则 ext8/16/32。类型码为 <see cref="sbyte"/>，最终按单字节写出。
        /// </remarks>
        public bool TryWriteExtensionFormatHeader(Span<byte> destination, ExtensionHeader extensionHeader, out int bytesWritten)
        {
            int dataLength = (int)extensionHeader.Length;
            byte typeCode = unchecked((byte)extensionHeader.TypeCode);
            switch (dataLength)
            {
                case 1 or 2 or 4 or 8 or 16:
                    bytesWritten = 2;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = dataLength switch
                    {
                        1 => BinaryCode.FixExt1,
                        2 => BinaryCode.FixExt2,
                        4 => BinaryCode.FixExt4,
                        8 => BinaryCode.FixExt8,
                        16 => BinaryCode.FixExt16,
                        _ => throw new Exception(string.Format("无法执行的编码:{0}", dataLength)),
                    };
                    destination[1] = unchecked(typeCode);
                    return true;
                case <= byte.MaxValue:
                    bytesWritten = 3;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryCode.Ext8;
                    destination[1] = unchecked((byte)dataLength);
                    destination[2] = unchecked(typeCode);
                    return true;
                case <= ushort.MaxValue:
                    bytesWritten = 4;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryCode.Ext16;
                    WriteBigEndian(destination.Slice(1), (ushort)dataLength);
                    destination[3] = unchecked(typeCode);
                    return true;
                default:
                    bytesWritten = 6;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryCode.Ext32;
                    WriteBigEndian(destination.Slice(1), dataLength);
                    destination[5] = unchecked(typeCode);
                    return true;
            }
        }

        /// <summary>
        /// 尝试写入负固定整数（negative fixint），不进行范围验证。
        /// </summary>
        /// <param name="destination">目标字节数组。</param>
        /// <param name="value">-32..-1 的数值（调用方需保证范围正确）。</param>
        /// <param name="bytesWritten">所需/实际写入的总字节数（1）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        private bool TryWriteNegativeFixIntUnsafe(Span<byte> destination, sbyte value, out int bytesWritten)
        {
            bytesWritten = 1;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = unchecked((byte)value);
            return true;
        }

        #region TryWrite

        /// <summary>
        /// 尝试将 8 位有符号整数写入，自动选择最紧凑编码（fixint 或 int8）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWrite(Span<byte> destination, sbyte value, out int bytesWritten)
        {
            if (value >= 0)
            {
                return TryWrite(destination, unchecked((byte)value), out bytesWritten);
            }

            return value >= BinaryRang.MinFixNegativeInt ?
                TryWriteNegativeFixIntUnsafe(destination, value, out bytesWritten) :
                TryWriteInt8(destination, value, out bytesWritten);
        }

        /// <summary>
        /// 尝试将 8 位无符号整数写入，自动选择最紧凑编码（fixint 或 uint8）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWrite(Span<byte> destination, byte value, out int bytesWritten)
        {
            if (value < BinaryRang.MaxFixPositiveInt)
            {
                return TryWriteFixIntUnsafe(destination, unchecked((byte)value), out bytesWritten);
            }
            return TryWriteUInt8(destination, value, out bytesWritten);
        }

        /// <summary>
        /// 尝试将 16 位有符号整数写入，自动在 negative fixint/int8/int16 之间选择。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWrite(Span<byte> destination, short value, out int bytesWritten)
        {
            if (value >= 0)
            {
                return TryWrite(destination, unchecked((ushort)value), out bytesWritten);
            }
            else if (value >= BinaryRang.MinFixNegativeInt)
            {
                return TryWriteNegativeFixIntUnsafe(destination, unchecked((sbyte)value), out bytesWritten);
            }
            else if (value >= sbyte.MinValue)
            {
                return TryWriteInt8(destination, unchecked((sbyte)value), out bytesWritten);
            }
            return TryWriteInt16(destination, value, out bytesWritten);
        }

        /// <summary>
        /// 尝试将 32 位有符号整数写入，自动在 negative fixint/int8/int16/int32 之间选择。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWrite(Span<byte> destination, int value, out int bytesWritten)
        {
            if (value >= 0)
            {
                return TryWrite(destination, unchecked((uint)value), out bytesWritten);
            }
            else if (value >= BinaryRang.MinFixNegativeInt)
            {
                return TryWriteNegativeFixIntUnsafe(destination, unchecked((sbyte)value), out bytesWritten);
            }
            else if (value >= sbyte.MinValue)
            {
                return TryWriteInt8(destination, unchecked((sbyte)value), out bytesWritten);
            }
            else if (value >= short.MinValue)
            {
                return TryWriteInt16(destination, unchecked((short)value), out bytesWritten);
            }
            return TryWriteInt32(destination, value, out bytesWritten);
        }

        /// <summary>
        /// 尝试将 64 位有符号整数写入，自动在 negative fixint/int8/int16/int32/int64 之间选择。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWrite(Span<byte> destination, long value, out int bytesWritten)
        {
            if (value >= 0)
            {
                return TryWrite(destination, unchecked((ulong)value), out bytesWritten);
            }

            if (value >= BinaryRang.MinFixNegativeInt)
            {
                return TryWriteNegativeFixIntUnsafe(destination, unchecked((sbyte)value), out bytesWritten);
            }

            return SlowPath(destination, value, out bytesWritten);
            bool SlowPath(Span<byte> destination, long value, out int bytesWritten)
            {
                switch (value)
                {
                    case >= 0: return TryWrite(destination, (ulong)value, out bytesWritten);
                    case >= sbyte.MinValue: return TryWriteInt8(destination, (sbyte)value, out bytesWritten);
                    case >= short.MinValue: return TryWriteInt16(destination, (short)value, out bytesWritten);
                    case >= int.MinValue: return TryWriteInt32(destination, (int)value, out bytesWritten);
                    default: return TryWriteInt64(destination, value, out bytesWritten);
                }
            }
        }

        /// <summary>
        /// 尝试将 16 位无符号整数写入，自动在 fixint/uint8/uint16 之间选择。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWrite(Span<byte> destination, ushort value, out int bytesWritten)
        {

            if (value < BinaryRang.MaxFixPositiveInt)
            {
                return TryWriteFixIntUnsafe(destination, unchecked((byte)value), out bytesWritten);
            }

            switch (value)
            {
                case <= byte.MaxValue:
                    return TryWriteUInt8(destination, unchecked((byte)value), out bytesWritten);
                default:
                    return TryWriteUInt16(destination, value, out bytesWritten);
            }
        }

        /// <summary>
        /// 尝试将 32 位无符号整数写入，自动在 fixint/uint8/uint16/uint32 之间选择。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWrite(Span<byte> destination, uint value, out int bytesWritten)
        {
            if (value <= BinaryRang.MaxFixPositiveInt)
            {
                return TryWriteFixIntUnsafe(destination, unchecked((byte)value), out bytesWritten);
            }

            switch (value)
            {
                case <= byte.MaxValue:
                    return TryWriteUInt8(destination, unchecked((byte)value), out bytesWritten);
                case <= ushort.MaxValue:
                    return TryWriteUInt16(destination, unchecked((ushort)value), out bytesWritten);
                default:
                    return TryWriteUInt32(destination, value, out bytesWritten);
            }
        }

        /// <summary>
        /// 尝试将 64 位无符号整数写入，自动在 fixint/uint8/uint16/uint32/uint64 之间选择。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWrite(Span<byte> destination, ulong value, out int bytesWritten)
        {
            if (value <= (ulong)BinaryRang.MaxFixPositiveInt)
            {
                return TryWriteFixIntUnsafe(destination, unchecked((byte)value), out bytesWritten);
            }

            return SlowPath(destination, value, out bytesWritten);

            bool SlowPath(Span<byte> destination, ulong value, out int bytesWritten)
            {
                switch (value)
                {
                    case <= byte.MaxValue:
                        return TryWriteUInt8(destination, unchecked((byte)value), out bytesWritten);
                    case <= ushort.MaxValue:
                        return TryWriteUInt16(destination, unchecked((ushort)value), out bytesWritten);
                    case <= uint.MaxValue:
                        return TryWriteUInt32(destination, unchecked((uint)value), out bytesWritten);
                    default:
                        return TryWriteUInt64(destination, value, out bytesWritten);
                }
            }
        }

        /// <summary>
        /// 尝试写入单精度浮点数（IEEE754），类型码 + 4 字节大端。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（5）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public unsafe bool TryWrite(Span<byte> destination, float value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.Float32;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.Float32;
            WriteBigEndian(destination.Slice(1), *(int*)&value);
            return true;
        }

        /// <summary>
        /// 尝试写入双精度浮点数（IEEE754），类型码 + 8 字节大端。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（9）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public unsafe bool TryWrite(Span<byte> destination, double value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.Float64;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.Float64;
            WriteBigEndian(destination.Slice(1), *(long*)&value);
            return true;
        }

        /// <summary>
        /// 尝试写入布尔值（true/false 类型码）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的布尔值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（1）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWrite(Span<byte> destination, bool value, out int bytesWritten)
        {
            bytesWritten = 1;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = value ? BinaryCode.True : BinaryCode.False;
            return true;
        }

        /// <summary>
        /// 尝试写入 UTF-16 单字符，按其 UTF-16 码值作为无符号 16 位整数进行编码。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的字符。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWrite(Span<byte> destination, char value, out int bytesWritten)
            => TryWrite(destination, (ushort)value, out bytesWritten);

        /// <summary>
        /// 尝试写入时间戳（ext 类型，type=-1），遵循 MessagePack Timestamp 规范。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的时间（Local 将转换为 UTC，Unspecified 保持不变）。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（6/10/15）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        /// <remarks>
        /// 编码选择：
        /// - fixext 4：仅秒（秒 ∈ [0, 2^32-1] 且纳秒为 0）
        /// - fixext 8：纳秒（30bit）+ 秒（34bit）
        /// - ext8(12)：纳秒（32bit）+ 秒（64bit）
        /// </remarks>
        public bool TryWrite(Span<byte> destination, DateTime value, out int bytesWritten)
        {
            // FixExt4(-1) => seconds |  [1970-01-01 00:00:00 UTC, 2106-02-07 06:28:16 UTC) range
            // FixExt8(-1) => nanoseconds + seconds | [1970-01-01 00:00:00.000000000 UTC, 2514-05-30 01:53:04.000000000 UTC) range
            // Ext8(12,-1) => nanoseconds + seconds | [-584554047284-02-23 16:59:44 UTC, 584554051223-11-09 07:00:16.000000000 UTC) range

            // 规范需要 UTC；Local 转 UTC，Unspecified 不改动。
            if (value.Kind == DateTimeKind.Local)
            {
                value = value.ToUniversalTime();
            }

            var secondsSinceBclEpoch = value.Ticks / TimeSpan.TicksPerSecond;
            var seconds = secondsSinceBclEpoch - BinaryDateTime.BclSecondsAtUnixEpoch;
            var nanoseconds = (value.Ticks % TimeSpan.TicksPerSecond) * BinaryDateTime.NanosecondsPerTick;


            if ((seconds >> 34) == 0)
            {
                var data64 = unchecked((ulong)((nanoseconds << 34) | seconds));
                if ((data64 & 0xffffffff00000000L) == 0)
                {
                    bytesWritten = 6;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    var data32 = (UInt32)data64;
                    destination[0] = BinaryCode.FixExt4;
                    destination[1] = unchecked((byte)BinaryDateTime.DateTime);
                    WriteBigEndian(destination.Slice(2), data32);
                }
                else
                {
                    bytesWritten = 10;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryCode.FixExt8;
                    destination[1] = unchecked((byte)BinaryDateTime.DateTime);
                    WriteBigEndian(destination.Slice(2), data64);
                }
            }
            else
            {
                bytesWritten = 15;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = BinaryCode.Ext8;
                destination[1] = 12;
                destination[2] = unchecked((byte)BinaryDateTime.DateTime);
                WriteBigEndian(destination.Slice(3), (uint)nanoseconds);
                WriteBigEndian(destination.Slice(7), seconds);
            }

            return true;
        }

        #endregion

        #region int

        /// <summary>
        /// 将 8 位有符号整数按 Int8 格式写入（类型码 + 数据 1 字节）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（2）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWriteInt8(Span<byte> destination, sbyte value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.Int8;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.Int8;
            destination[1] = unchecked((byte)value);
            return true;
        }

        /// <summary>
        /// 将 16 位有符号整数按 Int16 格式写入（类型码 + 大端 2 字节）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（3）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWriteInt16(Span<byte> destination, short value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.Int16;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.Int16;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        /// <summary>
        /// 将 32 位有符号整数按 Int32 格式写入（类型码 + 大端 4 字节）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（5）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWriteInt32(Span<byte> destination, int value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.Int32;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.Int32;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        /// <summary>
        /// 将 64 位有符号整数按 Int64 格式写入（类型码 + 大端 8 字节）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（9）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWriteInt64(Span<byte> destination, long value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.Int64;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.Int64;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        #endregion

        #region uint

        /// <summary>
        /// 将 8 位无符号整数按 UInt8 格式写入（类型码 + 数据 1 字节）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（2）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWriteUInt8(Span<byte> destination, byte value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.UInt8;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.UInt8;
            destination[1] = value;
            return true;
        }

        /// <summary>
        /// 将 16 位无符号整数按 UInt16 格式写入（类型码 + 大端 2 字节）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（3）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWriteUInt16(Span<byte> destination, ushort value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.UInt16;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.UInt16;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        /// <summary>
        /// 将 32 位无符号整数按 UInt32 格式写入（类型码 + 大端 4 字节）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数（5）。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWriteUInt32(Span<byte> destination, uint value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.UInt32;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.UInt32;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        /// <summary>
        /// 将 64 位无符号整数按 UInt64 格式写入（类型码 + 大端 8 字节）。
        /// </summary>
        /// <param name="destination">目标字节序列。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="bytesWritten">实际/所需写入的字节数。</param>
        /// <returns>成功写入返回 true；空间不足返回 false。</returns>
        public bool TryWriteUInt64(Span<byte> destination, ulong value, out int bytesWritten)
        {
            bytesWritten = BinaryLength.UInt16;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryCode.UInt64;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        #endregion

        #region WriteBigEndian

        /// <summary>
        /// 写入无符号 16 位整数的大端字节序（不写类型码）。
        /// </summary>
        /// <param name="destination">目标字节数组（至少 2 字节）。</param>
        /// <param name="value">待写入的值。</param>
        private void WriteBigEndian(Span<byte> destination, ushort value)
        {
            unchecked
            {
                destination[1] = (byte)value;
                destination[0] = (byte)(value >> 8);
            }
        }

        /// <summary>
        /// 写入无符号 32 位整数的大端字节序（不写类型码）。
        /// </summary>
        /// <param name="destination">目标字节数组（至少 4 字节）。</param>
        /// <param name="value">待写入的值。</param>
        private void WriteBigEndian(Span<byte> destination, uint value)
        {
            unchecked
            {
                destination[3] = (byte)value;
                destination[2] = (byte)(value >> 8);
                destination[1] = (byte)(value >> 16);
                destination[0] = (byte)(value >> 24);
            }
        }

        /// <summary>
        /// 写入无符号 64 位整数的大端字节序（不写类型码）。
        /// </summary>
        /// <param name="destination">目标字节数组（至少 8 字节）。</param>
        /// <param name="value">待写入的值。</param>
        private void WriteBigEndian(Span<byte> destination, ulong value)
        {
            unchecked
            {
                destination[7] = (byte)value;
                destination[6] = (byte)(value >> 8);
                destination[5] = (byte)(value >> 16);
                destination[4] = (byte)(value >> 24);
                destination[3] = (byte)(value >> 32);
                destination[2] = (byte)(value >> 40);
                destination[1] = (byte)(value >> 48);
                destination[0] = (byte)(value >> 56);
            }
        }

        /// <summary>
        /// 写入有符号 16 位整数的大端字节序（不写类型码）。
        /// </summary>
        private void WriteBigEndian(Span<byte> destination, short value)
            => WriteBigEndian(destination, unchecked((ushort)value));

        /// <summary>
        /// 写入有符号 32 位整数的大端字节序（不写类型码）。
        /// </summary>
        private void WriteBigEndian(Span<byte> destination, int value)
            => WriteBigEndian(destination, unchecked((uint)value));

        /// <summary>
        /// 写入有符号 64 位整数的大端字节序（不写类型码）。
        /// </summary>
        private void WriteBigEndian(Span<byte> destination, long value)
            => WriteBigEndian(destination, unchecked((ulong)value));

        #endregion

        #region GetByteCount

        /// <summary>
        /// 获取 Nil 编码后的字节数。
        /// </summary>
        /// <returns>编码后的字节数，恒为 1。</returns>
        public int GetByteCountNil() => BinaryLength.Nil;

        /// <summary>
        /// 获取数组头编码后的字节数。
        /// </summary>
        /// <param name="count">数组元素个数。</param>
        /// <returns>根据元素个数，返回 1 (fixarray), 3 (array16), 或 5 (array32)。</returns>
        public int GetByteCountArrayHeader(uint count)
        {
            if (count <= BinaryRang.MaxFixArrayCount) return 1;
            if (count <= ushort.MaxValue) return 3;
            return 5;
        }

        /// <summary>
        /// 获取映射头编码后的字节数。
        /// </summary>
        /// <param name="count">映射项个数。</param>
        /// <returns>根据映射项个数，返回 1 (fixmap), 3 (map16), 或 5 (map32)。</returns>
        public int GetByteCountMapHeader(uint count)
        {
            if (count <= BinaryRang.MaxFixMapCount) return 1;
            if (count <= ushort.MaxValue) return 3;
            return 5;
        }

        /// <summary>
        /// 获取二进制数据头编码后的字节数。
        /// </summary>
        /// <param name="length">二进制数据长度（字节）。</param>
        /// <returns>根据数据长度，返回 2 (bin8), 3 (bin16), 或 5 (bin32)。</returns>
        public int GetByteCountBinHeader(uint length)
        {
            if (length <= byte.MaxValue) return 2;
            if (length <= ushort.MaxValue) return 3;
            return 5;
        }

        /// <summary>
        /// 获取字符串头编码后的字节数。
        /// </summary>
        /// <param name="byteCount">字符串的字节数（非字符数）。</param>
        /// <returns>根据字节数，返回 1 (fixstr), 2 (str8), 3 (str16), 或 5 (str32)。</returns>
        public int GetByteCountStringHeader(uint byteCount)
        {
            if (byteCount <= BinaryRang.MaxFixStringLength) return 1;
            if (byteCount <= byte.MaxValue) return 2;
            if (byteCount <= ushort.MaxValue) return 3;
            return 5;
        }

        /// <summary>
        /// 获取扩展格式头编码后的字节数。
        /// </summary>
        /// <param name="extensionHeader">扩展头信息（包含类型码和长度）。</param>
        /// <returns>根据数据长度，返回 2 (fixext), 3 (ext8), 4 (ext16), 或 6 (ext32)。</returns>
        public int GetByteCountExtensionFormatHeader(ExtensionHeader extensionHeader)
        {
            return (int)extensionHeader.Length switch
            {
                1 or 2 or 4 or 8 or 16 => 2,
                <= byte.MaxValue => 3,
                <= ushort.MaxValue => 4,
                _ => 6,
            };
        }

        /// <summary>
        /// 获取 8 位有符号整数在最紧凑编码下的字节数。
        /// </summary>
        /// <param name="value">要计算的值。</param>
        /// <returns>编码后的字节数 (1 或 2)。</returns>
        public int GetByteCount(sbyte value)
        {
            if (value >= 0) return GetByteCount((byte)value);
            if (value >= BinaryRang.MinFixNegativeInt) return 1;
            return 2; // Int8
        }

        /// <summary>
        /// 获取 8 位无符号整数在最紧凑编码下的字节数。
        /// </summary>
        /// <param name="value">要计算的值。</param>
        /// <returns>编码后的字节数 (1 或 2)。</returns>
        public int GetByteCount(byte value)
        {
            if (value <= BinaryRang.MaxFixPositiveInt) return 1;
            return 2; // UInt8
        }

        /// <summary>
        /// 获取 16 位有符号整数在最紧凑编码下的字节数。
        /// </summary>
        /// <param name="value">要计算的值。</param>
        /// <returns>编码后的字节数 (1, 2, 或 3)。</returns>
        public int GetByteCount(short value)
        {
            if (value >= 0) return GetByteCount((ushort)value);
            if (value >= BinaryRang.MinFixNegativeInt) return 1;
            if (value >= sbyte.MinValue) return 2; // Int8
            return 3; // Int16
        }

        /// <summary>
        /// 获取 16 位无符号整数在最紧凑编码下的字节数。
        /// </summary>
        /// <param name="value">要计算的值。</param>
        /// <returns>编码后的字节数 (1, 2, 或 3)。</returns>
        public int GetByteCount(ushort value)
        {
            if (value <= BinaryRang.MaxFixPositiveInt) return 1;
            if (value <= byte.MaxValue) return 2; // UInt8
            return 3; // UInt16
        }

        /// <summary>
        /// 获取 32 位有符号整数在最紧凑编码下的字节数。
        /// </summary>
        /// <param name="value">要计算的值。</param>
        /// <returns>编码后的字节数 (1, 2, 3, 或 5)。</returns>
        public int GetByteCount(int value)
        {
            if (value >= 0) return GetByteCount((uint)value);
            if (value >= BinaryRang.MinFixNegativeInt) return 1;
            if (value >= sbyte.MinValue) return 2; // Int8
            if (value >= short.MinValue) return 3; // Int16
            return 5; // Int32
        }

        /// <summary>
        /// 获取 32 位无符号整数在最紧凑编码下的字节数。
        /// </summary>
        /// <param name="value">要计算的值。</param>
        /// <returns>编码后的字节数 (1, 2, 3, 或 5)。</returns>
        public int GetByteCount(uint value)
        {
            if (value <= BinaryRang.MaxFixPositiveInt) return 1;
            if (value <= byte.MaxValue) return 2; // UInt8
            if (value <= ushort.MaxValue) return 3; // UInt16
            return 5; // UInt32
        }

        /// <summary>
        /// 获取 64 位有符号整数在最紧凑编码下的字节数。
        /// </summary>
        /// <param name="value">要计算的值。</param>
        /// <returns>编码后的字节数 (1, 2, 3, 5, 或 9)。</returns>
        public int GetByteCount(long value)
        {
            if (value >= 0) return GetByteCount((ulong)value);
            if (value >= BinaryRang.MinFixNegativeInt) return 1;
            if (value >= sbyte.MinValue) return 2; // Int8
            if (value >= short.MinValue) return 3; // Int16
            if (value >= int.MinValue) return 5; // Int32
            return 9; // Int64
        }

        /// <summary>
        /// 获取 64 位无符号整数在最紧凑编码下的字节数。
        /// </summary>
        /// <param name="value">要计算的值。</param>
        /// <returns>编码后的字节数 (1, 2, 3, 5, 或 9)。</returns>
        public int GetByteCount(ulong value)
        {
            if (value <= (ulong)BinaryRang.MaxFixPositiveInt) return 1;
            if (value <= byte.MaxValue) return 2; // UInt8
            if (value <= ushort.MaxValue) return 3; // UInt16
            if (value <= uint.MaxValue) return 5; // UInt32
            return 9; // UInt64
        }

        /// <summary>
        /// 获取单精度浮点数编码后的字节数。
        /// </summary>
        /// <param name="value">要计算的值（未使用，仅用于方法重载）。</param>
        /// <returns>编码后的字节数，恒为 5。</returns>
        public int GetByteCount(float value) => BinaryLength.Float32;

        /// <summary>
        /// 获取双精度浮点数编码后的字节数。
        /// </summary>
        /// <param name="value">要计算的值（未使用，仅用于方法重载）。</param>
        /// <returns>编码后的字节数，恒为 9。</returns>
        public int GetByteCount(double value) => BinaryLength.Float64;

        /// <summary>
        /// 获取布尔值编码后的字节数。
        /// </summary>
        /// <param name="value">要计算的值（未使用，仅用于方法重载）。</param>
        /// <returns>编码后的字节数，恒为 1。</returns>
        public int GetByteCount(bool value) => 1;

        /// <summary>
        /// 获取字符在作为无符号16位整数编码时的字节数。
        /// </summary>
        /// <param name="value">要计算的字符。</param>
        /// <returns>编码后的字节数。</returns>
        public int GetByteCount(char value) => GetByteCount((ushort)value);

        /// <summary>
        /// 获取 DateTime 在 Timestamp 格式下的字节数。
        /// </summary>
        /// <param name="value">要计算的日期时间值。</param>
        /// <returns>根据其值范围，返回 6 (timestamp 32), 10 (timestamp 64), 或 15 (timestamp 96)。</returns>
        public int GetByteCount(DateTime value)
        {
            if (value.Kind == DateTimeKind.Local)
            {
                value = value.ToUniversalTime();
            }

            long secondsSinceBclEpoch = value.Ticks / TimeSpan.TicksPerSecond;
            long seconds = secondsSinceBclEpoch - BinaryDateTime.BclSecondsAtUnixEpoch;
            long nanoseconds = (value.Ticks % TimeSpan.TicksPerSecond) * BinaryDateTime.NanosecondsPerTick;

            if ((seconds >> 34) == 0)
            {
                // 纳秒为0且秒数在uint范围内，使用 timestamp 32
                if (nanoseconds == 0 && seconds >= 0 && seconds <= uint.MaxValue)
                {
                    return 6; // fixext 4
                }
                return 10; // fixext 8
            }

            return 15; // ext 8
        }

        #endregion
    }
}
