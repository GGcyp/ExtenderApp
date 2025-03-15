using ExtenderApp.Data;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace ExtenderApp.Common.IO.Binaries
{
    public partial class BinaryConvert
    {
        public DecodeResult TryReadNil(ReadOnlySpan<byte> source, out int tokenSize)
        {
            tokenSize = 1;
            if (source.Length == 0)
            {
                return DecodeResult.EmptyBuffer;
            }

            return source[0] == BinaryCode.Nil ? DecodeResult.Success : DecodeResult.TokenMismatch;
        }

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

        public DecodeResult TryReadDateTime(ReadOnlySpan<byte> source, out DateTime value, out int tokenSize)
        {
            DecodeResult result = TryReadExtensionHeader(source, out ExtensionHeader header, out tokenSize);
            if (result != DecodeResult.Success)
            {
                value = default;
                return result;
            }

            result = TryReadDateTime(source.Slice(tokenSize), header, out value, out int extensionSize);
            tokenSize += extensionSize;
            return result;
        }

        public DecodeResult TryReadDateTime(ReadOnlySpan<byte> source, ExtensionHeader header, out DateTime value, out int tokenSize)
        {
            tokenSize = checked((int)header.Length);
            if (header.TypeCode != DateTimeConstants.DateTime)
            {
                value = default;
                return DecodeResult.TokenMismatch;
            }

            if (source.Length < tokenSize)
            {
                value = default;
                return DecodeResult.InsufficientBuffer;
            }

            switch (header.Length)
            {
                case 4:
                    AssumesTrue(TryReadBigEndian(source, out uint uintValue));
                    value = DateTimeConstants.UnixEpoch.AddSeconds(uintValue);
                    return DecodeResult.Success;
                case 8:
                    AssumesTrue(TryReadBigEndian(source, out ulong ulongValue));
                    long nanoseconds = (long)(ulongValue >> 34);
                    ulong seconds = ulongValue & 0x00000003ffffffffL;
                    value = DateTimeConstants.UnixEpoch.AddSeconds(seconds).AddTicks(nanoseconds / DateTimeConstants.NanosecondsPerTick);
                    return DecodeResult.Success;
                case 12:
                    AssumesTrue(TryReadBigEndian(source, out uintValue));
                    nanoseconds = uintValue;
                    AssumesTrue(TryReadBigEndian(source.Slice(sizeof(uint)), out long longValue));
                    value = DateTimeConstants.UnixEpoch.AddSeconds(longValue).AddTicks(nanoseconds / DateTimeConstants.NanosecondsPerTick);
                    return DecodeResult.Success;
                default:
                    value = default;
                    return DecodeResult.TokenMismatch;
            }
        }

        public DecodeResult TryReadExtensionHeader(ReadOnlySpan<byte> source, out ExtensionHeader extensionHeader, out int tokenSize)
        {
            tokenSize = 2;
            if (source.Length < tokenSize)
            {
                extensionHeader = default;
                return source.Length == 0 ? DecodeResult.EmptyBuffer : DecodeResult.InsufficientBuffer;
            }

            uint length = 0;
            byte code = source[0];

            if (code == BinaryCode.FixExt1)
            {
                length = 1;
            }
            else if (code == BinaryCode.FixExt2)
            {
                length = 2;
            }
            else if (code == BinaryCode.FixExt4)
            {
                length = 4;
            }
            else if (code == BinaryCode.FixExt8)
            {
                length = 8;
            }
            else if (code == BinaryCode.FixExt16)
            {
                length = 16;
            }
            else if (code == BinaryCode.Ext8)
            {
                tokenSize = 3;
                if (source.Length < tokenSize)
                {
                    extensionHeader = default;
                    return DecodeResult.InsufficientBuffer;
                }

                length = source[1];
            }
            else if (code == BinaryCode.Ext16)
            {
                tokenSize = 4;
                if (source.Length < tokenSize)
                {
                    extensionHeader = default;
                    return DecodeResult.InsufficientBuffer;
                }

                AssumesTrue(TryReadBigEndian(source.Slice(1), out ushort ushortValue));
                length = ushortValue;
            }
            else if (code == BinaryCode.Ext32)
            {
                tokenSize = 6;
                if (source.Length < tokenSize)
                {
                    extensionHeader = default;
                    return DecodeResult.InsufficientBuffer;
                }

                AssumesTrue(TryReadBigEndian(source.Slice(1), out uint uintValue));
                length = uintValue;
            }
            else
            {
                extensionHeader = default;
                return DecodeResult.TokenMismatch;
            }

            sbyte typeCode = unchecked((sbyte)source[tokenSize - 1]);
            extensionHeader = new ExtensionHeader(typeCode, length);
            return DecodeResult.Success;
        }

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

        private void AssumesTrue([DoesNotReturnIf(false)] bool condition)
        {
            if (!condition)
            {
                throw new Exception("Internal error.");
            }
        }

        #region Struct

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
                return UTF8.GetString(pBytes, bytes.Length);
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
            return UTF8.GetChars(bytes, byteCount, chars, charCount);
        }

        #endregion
    }
}
