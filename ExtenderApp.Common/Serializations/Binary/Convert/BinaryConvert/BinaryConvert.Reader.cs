using ExtenderApp.Data;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace ExtenderApp.Common.Serializations.Binary
{
    public partial class BinaryConvert
    {
        /// <summary>
        /// 尝试读取 nil 令牌。
        /// </summary>
        /// <param name="source">输入字节序列。</param>
        /// <param name="tokenSize">输出：已消费的令牌长度，固定为 1。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadNil(ReadOnlySpan<byte> source, out int tokenSize)
        {
            tokenSize = 1;
            if (source.Length == 0)
            {
                return DecodeResult.EmptyBuffer;
            }

            return source[0] == BinaryCode.Nil ? DecodeResult.Success : DecodeResult.TokenMismatch;
        }

        /// <summary>
        /// 尝试读取数组头并获取元素数量。
        /// </summary>
        /// <param name="source">输入字节序列（起始于数组头）。</param>
        /// <param name="count">输出：数组元素数量。</param>
        /// <param name="tokenSize">输出：数组头占用的字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadArrayHeader(ReadOnlySpan<byte> source, out uint count, out int tokenSize)
        {
            tokenSize = 1;
            if (source.Length == 0)
            {
                count = 0;
                return DecodeResult.EmptyBuffer;
            }

            var code = source[0];

            if (code >= BinaryCode.MinFixArray && code < BinaryCode.MaxFixArray)
            {
                count = (byte)(source[0] & 0xF);
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.Array16)
            {
                tokenSize = 3;
                if (TryReadBigEndian(source.Slice(1), out ushort ushortValue))
                {
                    count = ushortValue;
                    return DecodeResult.Success;
                }
                else
                {
                    count = 0;
                    return DecodeResult.InsufficientBuffer;
                }
            }
            else if (code == BinaryCode.Array32)
            {
                tokenSize = 5;
                if (TryReadBigEndian(source.Slice(1), out uint uintValue))
                {
                    count = uintValue;
                    return DecodeResult.Success;
                }
                else
                {
                    count = 0;
                    return DecodeResult.InsufficientBuffer;
                }
            }

            count = 0;
            return DecodeResult.TokenMismatch;
        }

        /// <summary>
        /// 尝试读取映射（字典）头并获取键值对数量。
        /// </summary>
        /// <param name="source">输入字节序列（起始于映射头）。</param>
        /// <param name="count">输出：键值对数量。</param>
        /// <param name="tokenSize">输出：映射头占用的字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadMapHeader(ReadOnlySpan<byte> source, out uint count, out int tokenSize)
        {
            tokenSize = 1;
            if (source.Length == 0)
            {
                count = 0;
                return DecodeResult.EmptyBuffer;
            }

            var code = source[0];

            if (code >= BinaryCode.MinFixMap && code <= BinaryCode.MaxFixMap)
            {
                count = (byte)(source[0] & 0xF);
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.Map16)
            {
                tokenSize = 3;
                if (TryReadBigEndian(source.Slice(1), out ushort ushortValue))
                {
                    count = ushortValue;
                    return DecodeResult.Success;
                }
                else
                {
                    count = 0;
                    return DecodeResult.InsufficientBuffer;
                }
            }
            else if (code == BinaryCode.Map32)
            {
                tokenSize = 5;
                if (TryReadBigEndian(source.Slice(1), out uint uintValue))
                {
                    count = uintValue;
                    return DecodeResult.Success;
                }
                else
                {
                    count = 0;
                    return DecodeResult.InsufficientBuffer;
                }
            }
            count = 0;
            return DecodeResult.TokenMismatch;
        }

        /// <summary>
        /// 尝试读取布尔值。
        /// </summary>
        /// <param name="source">输入字节序列。</param>
        /// <param name="value">输出：布尔值。</param>
        /// <param name="tokenSize">输出：已消费字节数（true/false 令牌均为 1）。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadBool(ReadOnlySpan<byte> source, out bool value, out int tokenSize)
        {
            tokenSize = 1;
            if (source.Length == 0)
            {
                value = default;
                return DecodeResult.EmptyBuffer;
            }

            var code = source[0];

            if (code == BinaryCode.True)
            {
                value = true;
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.False)
            {
                value = false;
                return DecodeResult.Success;
            }
            value = false;
            return DecodeResult.TokenMismatch;
        }

        /// <summary>
        /// 尝试读取 UTF-16 码位并转换为 <see cref="char"/>。
        /// </summary>
        /// <param name="source">输入字节序列（起始于数值令牌）。</param>
        /// <param name="value">输出：字符。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadChar(ReadOnlySpan<byte> source, out char value, out int tokenSize)
        {
            DecodeResult result = TryReadUInt16(source, out ushort ordinal, out tokenSize);
            if (result == DecodeResult.Success)
            {
                value = (char)ordinal;
            }
            else
            {
                value = default;
            }

            return result;
        }

        /// <summary>
        /// 尝试读取 MessagePack 时间戳扩展并解析为 <see cref="DateTime"/>。
        /// </summary>
        /// <param name="source">输入字节序列（起始于扩展头）。</param>
        /// <param name="value">输出：解析后的时间（基于 Unix Epoch）。</param>
        /// <param name="tokenSize">输出：已消费总字节数（扩展头 + 时间戳负载）。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadDateTime(ReadOnlySpan<byte> source, out DateTime value, out int tokenSize)
        {
            DecodeResult result = TryReadDateTime(source, out value, out tokenSize);
            if (result != DecodeResult.Success)
            {
                value = default;
                return result;
            }

            result = TryReadDateTime(source.Slice(tokenSize), out value, out int extensionSize);
            tokenSize += extensionSize;
            return result;
        }

        /// <summary>
        /// 尝试读取二进制（bin）头并获取负载长度。
        /// </summary>
        /// <param name="source">输入字节序列（起始于 bin 头）。</param>
        /// <param name="length">输出：二进制负载长度。</param>
        /// <param name="tokenSize">输出：bin 头占用的字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadBinHeader(ReadOnlySpan<byte> source, out uint length, out int tokenSize)
        {
            tokenSize = 1;
            if (source.Length < tokenSize)
            {
                length = 0;
                return DecodeResult.EmptyBuffer;
            }

            byte code = source[0];

            if (code == BinaryCode.Bin8)
            {
                tokenSize = 2;
                if (source.Length < tokenSize)
                {
                    length = 0;
                    return DecodeResult.InsufficientBuffer;
                }

                length = source[1];
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.Bin16)
            {
                tokenSize = 3;
                if (source.Length < tokenSize)
                {
                    length = 0;
                    return DecodeResult.InsufficientBuffer;
                }

                AssumesTrue(TryReadBigEndian(source.Slice(1), out ushort ushortValue));
                length = ushortValue;
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.Bin32)
            {
                tokenSize = 5;
                if (source.Length < tokenSize)
                {
                    length = 0;
                    return DecodeResult.InsufficientBuffer;
                }

                AssumesTrue(TryReadBigEndian(source.Slice(1), out uint uintValue));
                length = uintValue;
                return DecodeResult.Success;
            }

            length = 0;
            return DecodeResult.TokenMismatch;
        }

        /// <summary>
        /// 尝试读取字符串（str）头并获取 UTF-8 负载长度。
        /// </summary>
        /// <param name="source">输入字节序列（起始于 str 头）。</param>
        /// <param name="length">输出：字符串字节长度（UTF-8）。</param>
        /// <param name="tokenSize">输出：str 头占用的字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadStringHeader(ReadOnlySpan<byte> source, out uint length, out int tokenSize)
        {
            tokenSize = 1;
            if (source.Length < tokenSize)
            {
                length = 0;
                return DecodeResult.EmptyBuffer;
            }

            byte code = source[0];

            if (code == BinaryCode.Str8)
            {
                tokenSize = 2;
                if (source.Length < tokenSize)
                {
                    length = 0;
                    return DecodeResult.InsufficientBuffer;
                }

                length = source[1];
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.Str16)
            {
                tokenSize = 3;
                if (source.Length < tokenSize)
                {
                    length = 0;
                    return DecodeResult.InsufficientBuffer;
                }

                AssumesTrue(TryReadBigEndian(source.Slice(1), out ushort ushortValue));
                length = ushortValue;
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.Str32)
            {
                tokenSize = 5;
                if (source.Length < tokenSize)
                {
                    length = 0;
                    return DecodeResult.InsufficientBuffer;
                }

                AssumesTrue(TryReadBigEndian(source.Slice(1), out uint uintValue));
                length = uintValue;
                return DecodeResult.Success;
            }
            else if (code >= BinaryCode.MinFixStr && code <= BinaryCode.MaxFixStr)
            {
                length = (byte)(source[0] & 0x1F);
                return DecodeResult.Success;
            }

            length = 0;
            return DecodeResult.TokenMismatch;
        }

        /// <summary>
        /// 以大端序读取 <see cref="ushort"/>。
        /// </summary>
        /// <param name="source">输入字节序列（至少 2 字节）。</param>
        /// <param name="value">输出：读取的值。</param>
        /// <returns>读取是否成功。</returns>
        private bool TryReadBigEndian(ReadOnlySpan<byte> source, out ushort value)
        {
            if (source.Length < sizeof(short))
            {
                value = default;
                return false;
            }

            value = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(source));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            return true;
        }

        /// <summary>
        /// 以大端序读取 <see cref="short"/>。
        /// </summary>
        /// <param name="source">输入字节序列（至少 2 字节）。</param>
        /// <param name="value">输出：读取的值。</param>
        /// <returns>读取是否成功。</returns>
        private bool TryReadBigEndian(ReadOnlySpan<byte> source, out short value)
        {
            if (TryReadBigEndian(source, out ushort ushortValue))
            {
                value = unchecked((short)ushortValue);
                return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// 以大端序读取 <see cref="uint"/>。
        /// </summary>
        /// <param name="source">输入字节序列（至少 4 字节）。</param>
        /// <param name="value">输出：读取的值。</param>
        /// <returns>读取是否成功。</returns>
        private bool TryReadBigEndian(ReadOnlySpan<byte> source, out uint value)
        {
            if (source.Length < sizeof(uint))
            {
                value = default;
                return false;
            }

            value = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(source));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            return true;
        }

        /// <summary>
        /// 以大端序读取 <see cref="int"/>。
        /// </summary>
        /// <param name="source">输入字节序列（至少 4 字节）。</param>
        /// <param name="value">输出：读取的值。</param>
        /// <returns>读取是否成功。</returns>
        private bool TryReadBigEndian(ReadOnlySpan<byte> source, out int value)
        {
            if (TryReadBigEndian(source, out uint uintValue))
            {
                value = unchecked((int)uintValue);
                return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// 以大端序读取 <see cref="ulong"/>。
        /// </summary>
        /// <param name="source">输入字节序列（至少 8 字节）。</param>
        /// <param name="value">输出：读取的值。</param>
        /// <returns>读取是否成功。</returns>
        private bool TryReadBigEndian(ReadOnlySpan<byte> source, out ulong value)
        {
            if (source.Length < sizeof(ulong))
            {
                value = default;
                return false;
            }

            value = Unsafe.ReadUnaligned<ulong>(ref MemoryMarshal.GetReference(source));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            return true;
        }

        /// <summary>
        /// 以大端序读取 <see cref="long"/>。
        /// </summary>
        /// <param name="source">输入字节序列（至少 8 字节）。</param>
        /// <param name="value">输出：读取的值。</param>
        /// <returns>读取是否成功。</returns>
        private bool TryReadBigEndian(ReadOnlySpan<byte> source, out long value)
        {
            if (TryReadBigEndian(source, out ulong ulongValue))
            {
                value = unchecked((long)ulongValue);
                return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// 断言条件为 true，否则抛出内部错误异常。
        /// </summary>
        /// <param name="condition">条件表达式。</param>
        /// <exception cref="Exception">当条件为 false 时抛出。</exception>
        private void AssumesTrue([DoesNotReturnIf(false)] bool condition)
        {
            if (!condition)
            {
                throw new Exception("Internal error.");
            }
        }

        #region Struct

        /// <summary>
        /// 尝试读取无符号 8 位整数。
        /// </summary>
        /// <param name="source">输入字节序列（起始于数值令牌）。</param>
        /// <param name="value">输出：数值。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadByte(ReadOnlySpan<byte> source, out Byte value, out int tokenSize)
        {
            if (source.Length > 0)
            {
                DecodeResult result = Decoders.UInt64JumpTable[source[0]].Read(source, out ulong longValue, out tokenSize);
                value = checked((Byte)longValue);
                return result;
            }
            else
            {
                tokenSize = 1;
                value = 0;
                return DecodeResult.EmptyBuffer;
            }
        }

        /// <summary>
        /// 尝试读取无符号 16 位整数。
        /// </summary>
        /// <param name="source">输入字节序列（起始于数值令牌）。</param>
        /// <param name="value">输出：数值。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadUInt16(ReadOnlySpan<byte> source, out UInt16 value, out int tokenSize)
        {
            if (source.Length > 0)
            {
                DecodeResult result = Decoders.UInt64JumpTable[source[0]].Read(source, out ulong longValue, out tokenSize);
                value = checked((UInt16)longValue);
                return result;
            }
            else
            {
                tokenSize = 1;
                value = 0;
                return DecodeResult.EmptyBuffer;
            }
        }

        /// <summary>
        /// 尝试读取无符号 32 位整数。
        /// </summary>
        /// <param name="source">输入字节序列（起始于数值令牌）。</param>
        /// <param name="value">输出：数值。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadUInt32(ReadOnlySpan<byte> source, out UInt32 value, out int tokenSize)
        {
            if (source.Length > 0)
            {
                DecodeResult result = Decoders.UInt64JumpTable[source[0]].Read(source, out ulong longValue, out tokenSize);
                value = checked((UInt32)longValue);
                return result;
            }
            else
            {
                tokenSize = 1;
                value = 0;
                return DecodeResult.EmptyBuffer;
            }
        }

        /// <summary>
        /// 尝试读取无符号 64 位整数。
        /// </summary>
        /// <param name="source">输入字节序列（起始于数值令牌）。</param>
        /// <param name="value">输出：数值。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadUInt64(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
        {
            if (source.Length > 0)
            {
                DecodeResult result = Decoders.UInt64JumpTable[source[0]].Read(source, out ulong longValue, out tokenSize);
                value = checked((UInt64)longValue);
                return result;
            }
            else
            {
                tokenSize = 1;
                value = 0;
                return DecodeResult.EmptyBuffer;
            }
        }

        /// <summary>
        /// 尝试读取有符号 8 位整数。
        /// </summary>
        /// <param name="source">输入字节序列（起始于数值令牌）。</param>
        /// <param name="value">输出：数值。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadSByte(ReadOnlySpan<byte> source, out SByte value, out int tokenSize)
        {
            if (source.Length > 0)
            {
                DecodeResult result = Decoders.Int64JumpTable[source[0]].Read(source, out long longValue, out tokenSize);
                value = checked((SByte)longValue);
                return result;
            }
            else
            {
                tokenSize = 1;
                value = 0;
                return DecodeResult.EmptyBuffer;
            }
        }

        /// <summary>
        /// 尝试读取有符号 16 位整数。
        /// </summary>
        /// <param name="source">输入字节序列（起始于数值令牌）。</param>
        /// <param name="value">输出：数值。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadInt16(ReadOnlySpan<byte> source, out Int16 value, out int tokenSize)
        {
            if (source.Length > 0)
            {
                DecodeResult result = Decoders.Int64JumpTable[source[0]].Read(source, out long longValue, out tokenSize);
                value = checked((Int16)longValue);
                return result;
            }
            else
            {
                tokenSize = 1;
                value = 0;
                return DecodeResult.EmptyBuffer;
            }
        }

        /// <summary>
        /// 尝试读取有符号 32 位整数。
        /// </summary>
        /// <param name="source">输入字节序列（起始于数值令牌）。</param>
        /// <param name="value">输出：数值。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadInt32(ReadOnlySpan<byte> source, out Int32 value, out int tokenSize)
        {
            if (source.Length > 0)
            {
                DecodeResult result = Decoders.Int64JumpTable[source[0]].Read(source, out long longValue, out tokenSize);
                value = checked((Int32)longValue);
                return result;
            }
            else
            {
                tokenSize = 1;
                value = 0;
                return DecodeResult.EmptyBuffer;
            }
        }

        /// <summary>
        /// 尝试读取有符号 64 位整数。
        /// </summary>
        /// <param name="source">输入字节序列（起始于数值令牌）。</param>
        /// <param name="value">输出：数值。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        public DecodeResult TryReadInt64(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
        {
            if (source.Length > 0)
            {
                DecodeResult result = Decoders.Int64JumpTable[source[0]].Read(source, out long longValue, out tokenSize);
                value = checked((Int64)longValue);
                return result;
            }
            else
            {
                tokenSize = 1;
                value = 0;
                return DecodeResult.EmptyBuffer;
            }
        }

        /// <summary>
        /// 尝试读取 32 位浮点数。
        /// </summary>
        /// <param name="source">输入字节序列（起始于浮点或数值令牌）。</param>
        /// <param name="value">输出：浮点值。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        /// <remarks>
        /// 同时支持从整数令牌提升为 <see cref="Single"/>。
        /// </remarks>
        public unsafe DecodeResult TryReadSingle(ReadOnlySpan<byte> source, out Single value, out int tokenSize)
        {
            tokenSize = 1;
            if (source.Length < 1)
            {
                value = default;
                return DecodeResult.EmptyBuffer;
            }

            byte code = source[0];

            if (code == BinaryCode.Float32)
            {
                tokenSize = 5;
                if (source.Length < tokenSize)
                {
                    value = default;
                    return DecodeResult.InsufficientBuffer;
                }

                AssumesTrue(TryReadBigEndian(source.Slice(1), out uint uintValue));
                value = *(float*)&uintValue;
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.Float64)
            {
                tokenSize = 9;
                if (source.Length < tokenSize)
                {
                    value = default;
                    return DecodeResult.InsufficientBuffer;
                }

                AssumesTrue(TryReadBigEndian(source.Slice(1), out ulong ulongValue));
                value = (Single)(*(double*)&ulongValue);
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.Int8 || code == BinaryCode.Int16 ||
                code == BinaryCode.Int32 || code == BinaryCode.Int64 ||
                (code >= BinaryCode.MinNegativeFixInt && code <= BinaryCode.MaxNegativeFixInt))
            {
                DecodeResult result = TryReadInt64(source, out long longValue, out tokenSize);
                value = longValue;
                return result;
            }
            else if (code == BinaryCode.UInt8 || code == BinaryCode.UInt16 ||
                code == BinaryCode.UInt32 || code == BinaryCode.UInt64 ||
                (code >= BinaryCode.MinFixInt && code <= BinaryCode.MaxFixInt))
            {
                DecodeResult result = TryReadUInt64(source, out var ulongValue, out tokenSize);
                value = ulongValue;
                return result;
            }

            value = default;
            return DecodeResult.TokenMismatch;
        }

        /// <summary>
        /// 尝试读取 64 位浮点数。
        /// </summary>
        /// <param name="source">输入字节序列（起始于浮点或数值令牌）。</param>
        /// <param name="value">输出：浮点值。</param>
        /// <param name="tokenSize">输出：已消费字节数。</param>
        /// <returns>解码结果。</returns>
        /// <remarks>
        /// 同时支持从整数令牌提升为 <see cref="Double"/>。
        /// </remarks>
        public unsafe DecodeResult TryReadDouble(ReadOnlySpan<byte> source, out Double value, out int tokenSize)
        {
            tokenSize = 1;
            if (source.Length < 1)
            {
                value = default;
                return DecodeResult.EmptyBuffer;
            }

            byte code = source[0];

            if (code == BinaryCode.Float32)
            {
                tokenSize = 5;
                if (source.Length < tokenSize)
                {
                    value = default;
                    return DecodeResult.InsufficientBuffer;
                }

                AssumesTrue(TryReadBigEndian(source.Slice(1), out uint uintValue));
                value = *(float*)&uintValue;
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.Float64)
            {
                tokenSize = 9;
                if (source.Length < tokenSize)
                {
                    value = default;
                    return DecodeResult.InsufficientBuffer;
                }

                AssumesTrue(TryReadBigEndian(source.Slice(1), out ulong ulongValue));
                value = (Double)(*(double*)&ulongValue);
                return DecodeResult.Success;
            }
            else if (code == BinaryCode.Int8 || code == BinaryCode.Int16 ||
                code == BinaryCode.Int32 || code == BinaryCode.Int64 ||
                (code >= BinaryCode.MinNegativeFixInt && code <= BinaryCode.MaxNegativeFixInt))
            {
                DecodeResult result = TryReadInt64(source, out long longValue, out tokenSize);
                value = longValue;
                return result;
            }
            else if (code == BinaryCode.UInt8 || code == BinaryCode.UInt16 ||
                code == BinaryCode.UInt32 || code == BinaryCode.UInt64 ||
                (code >= BinaryCode.MinFixInt && code <= BinaryCode.MaxFixInt))
            {
                DecodeResult result = TryReadUInt64(source, out var ulongValue, out tokenSize);
                value = ulongValue;
                return result;
            }

            value = default;
            return DecodeResult.TokenMismatch;
        }

        #endregion

        #region String

        /// <summary>
        /// 从字节序列中获取UTF-8编码的字符串。
        /// </summary>
        /// <param name="bytes">包含UTF-8编码字节的只读字节序列。</param>
        /// <returns>返回UTF-8编码的字符串。</returns>
        public unsafe string Utf8ToString(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            fixed (byte* pBytes = bytes)
            {
                return BinaryEncoding.GetString(pBytes, bytes.Length);
            }
        }

        /// <summary>
        /// 将UTF-8编码的字节数组转换为字符数组。
        /// </summary>
        /// <param name="bytes">指向UTF-8编码字节数组的指针。</param>
        /// <param name="byteCount">字节数组的长度。</param>
        /// <param name="chars">指向字符数组的指针。</param>
        /// <param name="charCount">字符数组的长度。</param>
        /// <returns>转换后的字符数量。</returns>
        public unsafe int UTF8ToChars(byte* bytes, int byteCount, char* chars, int charCount)
        {
            return BinaryEncoding.GetChars(bytes, byteCount, chars, charCount);
        }

        #endregion
    }
}
