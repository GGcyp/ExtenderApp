using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary
{
    /// <summary>
    /// 二进制写入器转换类。
    /// </summary>
    public class ExtenderBinaryWriterConvert
    {
        private readonly BinaryConvert _binaryConvert;

        public ExtenderBinaryWriterConvert(BinaryConvert binaryConvert)
        {
            _binaryConvert = binaryConvert;
        }

        /// <summary>
        /// 写入一个空值（Nil）。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        public void WriteNil(ref ExtenderBinaryWriter writer)
        {
            Span<byte> span = writer.GetSpan(1);
            AssumesTrue(_binaryConvert.TryWriteNil(span, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个原始的二进制块。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="span">原始的二进制数据。</param>
        public void WriteRaw(ref ExtenderBinaryWriter writer, ReadOnlySpan<byte> span)
            => writer.Write(span);

        /// <summary>
        /// 写入一个原始的二进制序列。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="sequence">原始的二进制序列。</param>
        public void WriteRaw(ref ExtenderBinaryWriter writer, in ReadOnlySequence<byte> sequence)
        {
            foreach (ReadOnlyMemory<byte> segment in sequence)
            {
                writer.Write(segment.Span);
            }
        }

        /// <summary>
        /// 写入一个数组头，数组长度由整数指定。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="count">数组长度。</param>
        public void WriteArrayHeader(ref ExtenderBinaryWriter writer, int count)
            => WriteArrayHeader(ref writer, (uint)count);

        /// <summary>
        /// 写入一个数组头，数组长度由无符号整数指定。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="count">数组长度。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteArrayHeader(ref ExtenderBinaryWriter writer, uint count)
        {
            Span<byte> span = writer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteArrayHeader(span, count, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个映射头，映射项数由整数指定。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="count">映射项数。</param>
        public void WriteMapHeader(ref ExtenderBinaryWriter writer, int count)
            => WriteMapHeader(ref writer, (uint)count);

        /// <summary>
        /// 写入一个映射头，映射项数由无符号整数指定。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="count">映射项数。</param>
        public void WriteMapHeader(ref ExtenderBinaryWriter writer, uint count)
        {
            Span<byte> span = writer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteMapHeader(span, count, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个字节值。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, byte value)
        {
            Span<byte> span = writer.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个无符号8位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt8(ref ExtenderBinaryWriter writer, byte value)
        {
            Span<byte> span = writer.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWriteUInt8(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个有符号8位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, sbyte value)
        {
            Span<byte> span = writer.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个有符号8位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt8(ref ExtenderBinaryWriter writer, sbyte value)
        {
            Span<byte> span = writer.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWriteInt8(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个无符号16位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, ushort value)
        {
            Span<byte> span = writer.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个无符号16位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt16(ref ExtenderBinaryWriter writer, ushort value)
        {
            Span<byte> span = writer.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWriteUInt16(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个有符号16位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, short value)
        {
            Span<byte> span = writer.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个有符号16位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt16(ref ExtenderBinaryWriter writer, short value)
        {
            Span<byte> span = writer.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWriteInt16(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个无符号32位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ref ExtenderBinaryWriter writer, uint value)
        {
            Span<byte> span = writer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个无符号32位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt32(ref ExtenderBinaryWriter writer, uint value)
        {
            Span<byte> span = writer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteUInt32(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个有符号32位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ref ExtenderBinaryWriter writer, int value)
        {
            Span<byte> span = writer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个有符号32位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt32(ref ExtenderBinaryWriter writer, int value)
        {
            Span<byte> span = writer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteInt32(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个无符号64位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, ulong value)
        {
            Span<byte> span = writer.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个无符号64位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt64(ref ExtenderBinaryWriter writer, ulong value)
        {
            Span<byte> span = writer.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWriteUInt64(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个有符号64位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, long value)
        {
            Span<byte> span = writer.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个有符号64位整数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt64(ref ExtenderBinaryWriter writer, long value)
        {
            Span<byte> span = writer.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWriteInt64(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个布尔值。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, bool value)
        {
            Span<byte> span = writer.GetSpan(1);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个字符。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, char value)
        {
            Span<byte> span = writer.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个浮点数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, float value)
        {
            Span<byte> span = writer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个双精度浮点数。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, double value)
        {
            Span<byte> span = writer.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个日期时间值。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="dateTime">要写入的值。</param>
        public void Write(ref ExtenderBinaryWriter writer, DateTime dateTime)
        {
            Span<byte> span = writer.GetSpan(15);
            AssumesTrue(_binaryConvert.TryWrite(span, dateTime, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个字节数组。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="src">要写入的字节数组。</param>
        public void Write(ref ExtenderBinaryWriter writer, byte[]? src)
        {
            if (src == null)
            {
                WriteNil(ref writer);
            }
            else
            {
                writer.Write(src.AsSpan());
            }
        }

        /// <summary>
        /// 写入一个字节序列。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="src">要写入的字节序列。</param>
        public void Write(ref ExtenderBinaryWriter writer, ReadOnlySpan<byte> src)
        {
            int length = (int)src.Length;
            WriteBinHeader(ref writer, length);
            var span = writer.GetSpan(length);
            src.CopyTo(span);
            writer.Advance(length);
        }

        /// <summary>
        /// 写入一个字节序列。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="src">要写入的字节序列。</param>
        public void Write(ref ExtenderBinaryWriter writer, in ReadOnlySequence<byte> src)
        {
            int length = (int)src.Length;
            WriteBinHeader(ref writer, length);
            var span = writer.GetSpan(length);
            src.CopyTo(span);
            writer.Advance(length);
        }

        /// <summary>
        /// 将字符串写入到给定的 <see cref="ExtenderBinaryWriter"/> 中。
        /// </summary>
        /// <param name="writer"><see cref="ExtenderBinaryWriter"/> 实例，用于写入数据。</param>
        /// <param name="value">要写入的字符串。如果为 null，则写入 nil 值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ref ExtenderBinaryWriter writer, string? value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            ref byte buffer = ref WriteString_PrepareSpan(ref writer, value.Length, out int bufferSize, out int useOffset);
            fixed (char* pValue = value)
            fixed (byte* pBuffer = &buffer)
            {
                int byteCount = _binaryConvert.UTF8.GetBytes(pValue, value.Length, pBuffer + useOffset, bufferSize);
                WriteString_PostEncoding(ref writer, pBuffer, useOffset, byteCount);
            }
        }

        /// <summary>
        /// 写入一个二进制块头。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="length">二进制块长度。</param>
        public void WriteBinHeader(ref ExtenderBinaryWriter writer, int length)
        {
            Span<byte> span = writer.GetSpan(length + 5);
            AssumesTrue(_binaryConvert.TryWriteBinHeader(span, (uint)length, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个UTF-8字符串。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="utf8stringBytes">UTF-8编码的字符串字节序列。</param>
        public void WriteString(ref ExtenderBinaryWriter writer, in ReadOnlySequence<byte> utf8stringBytes)
        {
            var length = (int)utf8stringBytes.Length;
            WriteStringHeader(ref writer, length);
            Span<byte> span = writer.GetSpan(length);
            utf8stringBytes.CopyTo(span);
            writer.Advance(length);
        }

        /// <summary>
        /// 将UTF-8编码的字符串写入到BinaryWriter中。
        /// </summary>
        /// <param name="writer">BinaryWriter实例。</param>
        /// <param name="utf8stringBytes">UTF-8编码的字符串的字节序列。</param>
        public void WriteString(ref ExtenderBinaryWriter writer, ReadOnlySpan<byte> utf8stringBytes)
        {
            var length = utf8stringBytes.Length;
            WriteStringHeader(ref writer, length);
            Span<byte> span = writer.GetSpan(length);
            utf8stringBytes.CopyTo(span);
            writer.Advance(length);
        }

        public void WriteStringHeader(ref ExtenderBinaryWriter writer, int byteCount)
        {
            Span<byte> span = writer.GetSpan(byteCount + 5);
            AssumesTrue(_binaryConvert.TryWriteStringHeader(span, (uint)byteCount, out int written));
            writer.Advance(written);
        }

        #region String

        /// <summary>
        /// 准备字符串写入的缓冲区，并返回缓冲区指针。
        /// </summary>
        /// <param name="writer">扩展的二进制写入器。</param>
        /// <param name="characterLength">字符串的字符长度。</param>
        /// <param name="bufferSize">缓冲区的大小。</param>
        /// <param name="encodedBytesOffset">编码后的字节偏移量。</param>
        /// <returns>缓冲区指针。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref byte WriteString_PrepareSpan(ref ExtenderBinaryWriter writer, int characterLength, out int bufferSize, out int encodedBytesOffset)
        {
            // 计算缓冲区大小
            bufferSize = _binaryConvert.UTF8.GetMaxByteCount(characterLength) + 5;
            // 获取缓冲区指针
            ref byte buffer = ref writer.GetPointer(bufferSize);

            int useOffset;
            // 根据字符长度确定偏移量
            if (characterLength <= _binaryConvert.BinaryOptions.BinaryRang.MaxFixStringLength)
            {
                useOffset = 1;
            }
            else if (characterLength <= byte.MaxValue)
            {
                useOffset = 2;
            }
            else if (characterLength <= ushort.MaxValue)
            {
                useOffset = 3;
            }
            else
            {
                useOffset = 5;
            }

            // 设置编码后的字节偏移量
            encodedBytesOffset = useOffset;
            return ref buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void WriteString_PostEncoding(ref ExtenderBinaryWriter writer, byte* pBuffer, int estimatedOffset, int byteCount)
        {
            // move body and write prefix
            if (byteCount <= _binaryConvert.BinaryOptions.BinaryRang.MaxFixStringLength)
            {
                if (estimatedOffset != 1)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 1, byteCount, byteCount);
                }

                pBuffer[0] = (byte)(_binaryConvert.BinaryOptions.BinaryCode.MinFixStr | byteCount);
                writer.Advance(byteCount + 1);
            }
            else if (byteCount <= byte.MaxValue)
            {
                if (estimatedOffset != 2)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 2, byteCount, byteCount);
                }

                pBuffer[0] = _binaryConvert.BinaryOptions.BinaryCode.Str8;
                pBuffer[1] = unchecked((byte)byteCount);
                writer.Advance(byteCount + 2);
            }
            else if (byteCount <= ushort.MaxValue)
            {
                if (estimatedOffset != 3)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 3, byteCount, byteCount);
                }

                pBuffer[0] = _binaryConvert.BinaryOptions.BinaryCode.Str16;
                WriteBigEndian((ushort)byteCount, pBuffer + 1);
                writer.Advance(byteCount + 3);
            }
            else
            {
                if (estimatedOffset != 5)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 5, byteCount, byteCount);
                }

                pBuffer[0] = _binaryConvert.BinaryOptions.BinaryCode.Str32;
                WriteBigEndian((uint)byteCount, pBuffer + 1);
                writer.Advance(byteCount + 5);
            }
        }

        private unsafe void WriteBigEndian(ushort value, byte* span)
        {
            unchecked
            {
                span[0] = (byte)(value >> 8);
                span[1] = (byte)value;
            }
        }

        private unsafe void WriteBigEndian(uint value, byte* span)
        {
            unchecked
            {
                span[0] = (byte)(value >> 24);
                span[1] = (byte)(value >> 16);
                span[2] = (byte)(value >> 8);
                span[3] = (byte)value;
            }
        }

        #endregion

        /// <summary>
        /// 假设条件为真，否则抛出异常。
        /// </summary>
        /// <param name="condition">条件表达式。</param>
        private void AssumesTrue([DoesNotReturnIf(false)] bool condition)
        {
            if (!condition)
            {
                throw new Exception("Internal error.");
            }
        }
    }
}
