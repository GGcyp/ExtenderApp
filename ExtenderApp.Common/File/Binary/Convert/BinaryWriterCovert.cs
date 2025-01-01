using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BinaryWriter = ExtenderApp.Data.BinaryWriter;

namespace ExtenderApp.Common.File.Binary
{
    /// <summary>
    /// BinaryWriter 的扩展方法类。
    /// </summary>
    public class BinaryWriterCovert
    {
        private readonly BinaryConvert _binaryConvert;

        public BinaryWriterCovert() : this(new BinaryConvert())
        {

        }

        public BinaryWriterCovert(BinaryConvert binaryConvert)
        {
            _binaryConvert = binaryConvert;
        }

        /// <summary>
        /// 写入一个空值（Nil）。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        public void WriteNil(BinaryWriter writer)
        {
            Span<byte> span = writer.GetSpan(1);
            AssumesTrue(_binaryConvert.TryWriteNil(span, out int written));
            writer.Advance(written);
        }

        /// <summary>
        /// 写入一个原始的二进制块。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="rawMessagePackBlock">原始的二进制数据。</param>
        public void WriteRaw(BinaryWriter writer, ReadOnlySpan<byte> rawMessagePackBlock)
            => writer.Write(rawMessagePackBlock);

        /// <summary>
        /// 写入一个原始的二进制序列。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="rawMessagePackBlock">原始的二进制序列。</param>
        public void WriteRaw(BinaryWriter writer, in ReadOnlySequence<byte> rawMessagePackBlock)
        {
            foreach (ReadOnlyMemory<byte> segment in rawMessagePackBlock)
            {
                writer.Write(segment.Span);
            }
        }

        /// <summary>
        /// 写入一个数组头，数组长度由整数指定。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="count">数组长度。</param>
        public void WriteArrayHeader(BinaryWriter writer, int count)
            => WriteArrayHeader(writer, (uint)count);

        /// <summary>
        /// 写入一个数组头，数组长度由无符号整数指定。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="count">数组长度。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteArrayHeader(BinaryWriter writer, uint count)
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
        public void WriteMapHeader(BinaryWriter writer, int count)
            => WriteMapHeader(writer, (uint)count);

        /// <summary>
        /// 写入一个映射头，映射项数由无符号整数指定。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="count">映射项数。</param>
        public void WriteMapHeader(BinaryWriter writer, uint count)
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
        public void Write(BinaryWriter writer, byte value)
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
        public void WriteUInt8(BinaryWriter writer, byte value)
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
        public void Write(BinaryWriter writer, sbyte value)
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
        public void WriteInt8(BinaryWriter writer, sbyte value)
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
        public void Write(BinaryWriter writer, ushort value)
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
        public void WriteUInt16(BinaryWriter writer, ushort value)
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
        public void Write(BinaryWriter writer, short value)
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
        public void WriteInt16(BinaryWriter writer, short value)
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
        public void Write(BinaryWriter writer, uint value)
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
        public void WriteUInt32(BinaryWriter writer, uint value)
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
        public void Write(BinaryWriter writer, int value)
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
        public void WriteInt32(BinaryWriter writer, int value)
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
        public void Write(BinaryWriter writer, ulong value)
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
        public void WriteUInt64(BinaryWriter writer, ulong value)
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
        public void Write(BinaryWriter writer, long value)
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
        public void WriteInt64(BinaryWriter writer, long value)
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
        public void Write(BinaryWriter writer, bool value)
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
        public void Write(BinaryWriter writer, char value)
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
        public void Write(BinaryWriter writer, float value)
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
        public void Write(BinaryWriter writer, double value)
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
        public void Write(BinaryWriter writer, DateTime dateTime)
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
        public void Write(BinaryWriter writer, byte[]? src)
        {
            if (src == null)
            {
                WriteNil(writer);
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
        public void Write(BinaryWriter writer, ReadOnlySpan<byte> src)
        {
            int length = (int)src.Length;
            WriteBinHeader(writer, length);
            var span = writer.GetSpan(length);
            src.CopyTo(span);
            writer.Advance(length);
        }

        /// <summary>
        /// 写入一个字节序列。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="src">要写入的字节序列。</param>
        public void Write(BinaryWriter writer, in ReadOnlySequence<byte> src)
        {
            int length = (int)src.Length;
            WriteBinHeader(writer, length);
            var span = writer.GetSpan(length);
            src.CopyTo(span);
            writer.Advance(length);
        }

        /// <summary>
        /// 写入一个二进制块头。
        /// </summary>
        /// <param name="writer">BinaryWriter 实例。</param>
        /// <param name="length">二进制块长度。</param>
        public void WriteBinHeader(BinaryWriter writer, int length)
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
        public void WriteString(BinaryWriter writer, in ReadOnlySequence<byte> utf8stringBytes)
        {
            var length = (int)utf8stringBytes.Length;
            WriteStringHeader(writer, length);
            Span<byte> span = writer.GetSpan(length);
            utf8stringBytes.CopyTo(span);
            writer.Advance(length);
        }

        /// <summary>
        /// 将UTF-8编码的字符串写入到BinaryWriter中。
        /// </summary>
        /// <param name="writer">BinaryWriter实例。</param>
        /// <param name="utf8stringBytes">UTF-8编码的字符串的字节序列。</param>
        public void WriteString(BinaryWriter writer, ReadOnlySpan<byte> utf8stringBytes)
        {
            var length = utf8stringBytes.Length;
            WriteStringHeader(writer, length);
            Span<byte> span = writer.GetSpan(length);
            utf8stringBytes.CopyTo(span);
            writer.Advance(length);
        }

        /// <summary>
        /// 写入字符串头信息到BinaryWriter中。
        /// </summary>
        /// <param name="writer">BinaryWriter实例。</param>
        /// <param name="byteCount">字符串的字节数。</param>
        public void WriteStringHeader(BinaryWriter writer, int byteCount)
        {
            Span<byte> span = writer.GetSpan(byteCount + 5);
            AssumesTrue(_binaryConvert.TryWriteStringHeader(span, (uint)byteCount, out int written));
            writer.Advance(written);
        }

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
