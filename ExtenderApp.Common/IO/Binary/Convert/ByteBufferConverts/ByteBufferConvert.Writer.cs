using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary
{
    public partial class ByteBufferConvert
    {
        /// <summary>
        /// 写入一个空值（Nil）。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        public void WriteNil(ref ByteBuffer buffer)
        {
            Span<byte> span = buffer.GetSpan(1);
            AssumesTrue(_binaryConvert.TryWriteNil(span, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个原始的二进制块。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="span">原始的二进制数据。</param>
        public void bufferaw(ref ByteBuffer buffer, ReadOnlySpan<byte> span)
            => buffer.Write(span);

        /// <summary>
        /// 写入一个原始的二进制序列。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="sequence">原始的二进制序列。</param>
        public void bufferaw(ref ByteBuffer buffer, in ReadOnlySequence<byte> sequence)
        {
            foreach (ReadOnlyMemory<byte> segment in sequence)
            {
                buffer.Write(segment.Span);
            }
        }

        /// <summary>
        /// 写入一个数组头，数组长度由整数指定。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="count">数组长度。</param>
        public void WriteArrayHeader(ref ByteBuffer buffer, int count)
            => WriteArrayHeader(ref buffer, (uint)count);

        /// <summary>
        /// 写入一个数组头，数组长度由无符号整数指定。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="count">数组长度。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteArrayHeader(ref ByteBuffer buffer, uint count)
        {
            Span<byte> span = buffer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteArrayHeader(span, count, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个映射头，映射项数由整数指定。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="count">映射项数。</param>
        public void WriteMapHeader(ref ByteBuffer buffer, int count)
            => WriteMapHeader(ref buffer, (uint)count);

        /// <summary>
        /// 写入一个映射头，映射项数由无符号整数指定。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="count">映射项数。</param>
        public void WriteMapHeader(ref ByteBuffer buffer, uint count)
        {
            Span<byte> span = buffer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteMapHeader(span, count, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个字节值。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, byte value)
        {
            Span<byte> span = buffer.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号8位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt8(ref ByteBuffer buffer, byte value)
        {
            Span<byte> span = buffer.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWriteUInt8(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号8位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, sbyte value)
        {
            Span<byte> span = buffer.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号8位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt8(ref ByteBuffer buffer, sbyte value)
        {
            Span<byte> span = buffer.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWriteInt8(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号16位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, ushort value)
        {
            Span<byte> span = buffer.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号16位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt16(ref ByteBuffer buffer, ushort value)
        {
            Span<byte> span = buffer.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWriteUInt16(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号16位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, short value)
        {
            Span<byte> span = buffer.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号16位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt16(ref ByteBuffer buffer, short value)
        {
            Span<byte> span = buffer.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWriteInt16(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号32位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ref ByteBuffer buffer, uint value)
        {
            Span<byte> span = buffer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号32位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt32(ref ByteBuffer buffer, uint value)
        {
            Span<byte> span = buffer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteUInt32(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号32位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ref ByteBuffer buffer, int value)
        {
            Span<byte> span = buffer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号32位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt32(ref ByteBuffer buffer, int value)
        {
            Span<byte> span = buffer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteInt32(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号64位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, ulong value)
        {
            Span<byte> span = buffer.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号64位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt64(ref ByteBuffer buffer, ulong value)
        {
            Span<byte> span = buffer.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWriteUInt64(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号64位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, long value)
        {
            Span<byte> span = buffer.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号64位整数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt64(ref ByteBuffer buffer, long value)
        {
            Span<byte> span = buffer.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWriteInt64(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个布尔值。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, bool value)
        {
            Span<byte> span = buffer.GetSpan(1);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个字符。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, char value)
        {
            Span<byte> span = buffer.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个浮点数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, float value)
        {
            Span<byte> span = buffer.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个双精度浮点数。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, double value)
        {
            Span<byte> span = buffer.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个日期时间值。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="dateTime">要写入的值。</param>
        public void Write(ref ByteBuffer buffer, DateTime dateTime)
        {
            Span<byte> span = buffer.GetSpan(15);
            AssumesTrue(_binaryConvert.TryWrite(span, dateTime, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个字节数组。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="src">要写入的字节数组。</param>
        public void Write(ref ByteBuffer buffer, byte[]? src)
        {
            if (src == null)
            {
                WriteNil(ref buffer);
            }
            else
            {
                buffer.Write(src.AsSpan());
            }
        }

        /// <summary>
        /// 写入一个字节序列。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="src">要写入的字节序列。</param>
        public void Write(ref ByteBuffer buffer, ReadOnlySpan<byte> src)
        {
            int length = (int)src.Length;
            WriteBinHeader(ref buffer, length);
            var span = buffer.GetSpan(length);
            src.CopyTo(span);
            buffer.WriteAdvance(length);
        }

        /// <summary>
        /// 写入一个字节序列。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="src">要写入的字节序列。</param>
        public void Write(ref ByteBuffer buffer, in ReadOnlySequence<byte> src)
        {
            int length = (int)src.Length;
            WriteBinHeader(ref buffer, length);
            var span = buffer.GetSpan(length);
            src.CopyTo(span);
            buffer.WriteAdvance(length);
        }

        /// <summary>
        /// 将字符串写入到给定的 <see cref="ByteBuffer"/> 中。
        /// </summary>
        /// <param name="buffer">
        /// <see cref="ByteBuffer"/> 实例，用于写入数据。
        /// </param>
        /// <param name="value">
        /// 要写入的字符串。如果为 null，则写入 nil 值。
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ref ByteBuffer buffer, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteNil(ref buffer);
                return;
            }

            ref byte byteBuffer = ref WriteString_PrepareSpan(ref buffer, value.Length, out int bufferSize, out int useOffset);
            fixed (char* pValue = value)
            fixed (byte* pBuffer = &byteBuffer)
            {
                int byteCount = _binaryConvert.BinaryEncoding.GetBytes(pValue, value.Length, pBuffer + useOffset, bufferSize);
                WriteString_PostEncoding(ref buffer, pBuffer, useOffset, byteCount);
            }
        }

        /// <summary>
        /// 写入一个二进制块头。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="length">二进制块长度。</param>
        public void WriteBinHeader(ref ByteBuffer buffer, int length)
        {
            Span<byte> span = buffer.GetSpan(length + 5);
            AssumesTrue(_binaryConvert.TryWriteBinHeader(span, (uint)length, out int written));
            buffer.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个UTF-8字符串。
        /// </summary>
        /// <param name="buffer">
        /// Binarybuffer 实例。
        /// </param>
        /// <param name="utf8stringBytes">UTF-8编码的字符串字节序列。</param>
        public void WriteString(ref ByteBuffer buffer, in ReadOnlySequence<byte> utf8stringBytes)
        {
            var length = (int)utf8stringBytes.Length;
            WriteStringHeader(ref buffer, length);
            Span<byte> span = buffer.GetSpan(length);
            utf8stringBytes.CopyTo(span);
            buffer.WriteAdvance(length);
        }

        /// <summary>
        /// 将UTF-8编码的字符串写入到Binarybuffer中。
        /// </summary>
        /// <param name="buffer">Binarybuffer实例。</param>
        /// <param name="utf8stringBytes">UTF-8编码的字符串的字节序列。</param>
        public void WriteString(ref ByteBuffer buffer, in ReadOnlySpan<byte> utf8stringBytes)
        {
            var length = utf8stringBytes.Length;
            WriteStringHeader(ref buffer, length);
            Span<byte> span = buffer.GetSpan(length);
            utf8stringBytes.CopyTo(span);
            buffer.WriteAdvance(length);
        }

        /// <summary>
        /// 向二进制写入器中写入字符串头部。
        /// </summary>
        /// <param name="buffer">二进制写入器。</param>
        /// <param name="byteCount">字符串的字节数。</param>
        /// <remarks>
        /// 此方法首先通过调用 <see
        /// cref="ByteBuffer.GetSpan(int)"/>
        /// 方法获取一个足够大的字节跨度， 然后调用 <see
        /// cref="_binaryConvert.TryWriteStringHeader(Span{byte},
        /// uint, out int)"/> 方法尝试将字符串头部写入该跨度中。
        /// 如果写入成功，则通过调用 <see
        /// cref="ByteBuffer.WriteAdvance(int)"/> 方法更新写入器的位置。
        /// </remarks>
        public void WriteStringHeader(ref ByteBuffer buffer, int byteCount)
        {
            Span<byte> span = buffer.GetSpan(byteCount + 5);
            AssumesTrue(_binaryConvert.TryWriteStringHeader(span, (uint)byteCount, out int written));
            buffer.WriteAdvance(written);
        }

        #region String

        /// <summary>
        /// 准备字符串写入的缓冲区，并返回缓冲区指针。
        /// </summary>
        /// <param name="buffer">扩展的二进制写入器。</param>
        /// <param name="characterLength">字符串的字符长度。</param>
        /// <param name="bufferSize">缓冲区的大小。</param>
        /// <param name="encodedBytesOffset">
        /// 编码后的字节偏移量。
        /// </param>
        /// <returns>缓冲区指针。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref byte WriteString_PrepareSpan(ref ByteBuffer buffer, int characterLength, out int bufferSize, out int encodedBytesOffset)
        {
            // 计算缓冲区大小
            bufferSize = _binaryConvert.BinaryEncoding.GetMaxByteCount(characterLength) + 5;
            // 获取缓冲区指针
            ref byte bytes = ref buffer.GetPointer(bufferSize);

            int useOffset;
            // 根据字符长度确定偏移量
            if (characterLength <= _binaryConvert.BinaryRang.MaxFixStringLength)
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
            return ref bytes;
        }

        /// <summary>
        /// 在字符串编码完成后，按实际字节数写入前缀并调整主体位置，然后推进写入游标。
        /// </summary>
        /// <param name="buffer">目标写入器。</param>
        /// <param name="pBuffer">指向缓冲区起始位置的指针（可能包含为前缀预留的偏移）。</param>
        /// <param name="estimatedOffset">预估的前缀字节数（编码前预留的偏移）。</param>
        /// <param name="byteCount">
        /// 实际编码得到的 UTF-8 字节数。
        /// </param>
        /// <remarks>
        /// - 若实际前缀大小与预估不一致，将通过内存移动使主体紧跟在正确大小的前缀之后。
        /// - 前缀规则： • FixStr: 1 字节类型码（携带长度低位） + N
        /// 字节数据（N ≤ MaxFixStringLength）； • Str8:
        /// 1 字节类型码 + 1 字节长度； • Str16: 1 字节类型码 + 2
        /// 字节长度（大端）； • Str32: 1 字节类型码 + 4 字节长度（大端）。
        /// - 成功写入后会调用 <see
        ///   cref="ByteBuffer.WriteAdvance(int)"/> 推进相应总字节数。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void WriteString_PostEncoding(ref ByteBuffer buffer, byte* pBuffer, int estimatedOffset, int byteCount)
        {
            // move body and write prefix
            if (byteCount <= _binaryConvert.BinaryOptions.BinaryRang.MaxFixStringLength)
            {
                if (estimatedOffset != 1)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 1, byteCount, byteCount);
                }

                pBuffer[0] = (byte)(_binaryConvert.BinaryOptions.BinaryCode.MinFixStr | byteCount);
                buffer.WriteAdvance(byteCount + 1);
            }
            else if (byteCount <= byte.MaxValue)
            {
                if (estimatedOffset != 2)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 2, byteCount, byteCount);
                }

                pBuffer[0] = _binaryConvert.BinaryOptions.BinaryCode.Str8;
                pBuffer[1] = unchecked((byte)byteCount);
                buffer.WriteAdvance(byteCount + 2);
            }
            else if (byteCount <= ushort.MaxValue)
            {
                if (estimatedOffset != 3)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 3, byteCount, byteCount);
                }

                pBuffer[0] = _binaryConvert.BinaryOptions.BinaryCode.Str16;
                WriteBigEndian((ushort)byteCount, pBuffer + 1);
                buffer.WriteAdvance(byteCount + 3);
            }
            else
            {
                if (estimatedOffset != 5)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 5, byteCount, byteCount);
                }

                pBuffer[0] = _binaryConvert.BinaryOptions.BinaryCode.Str32;
                WriteBigEndian((uint)byteCount, pBuffer + 1);
                buffer.WriteAdvance(byteCount + 5);
            }
        }

        /// <summary>
        /// 以大端序写入 16 位无符号整数。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="span">目标缓冲区指针（至少 2 字节）。</param>
        private unsafe void WriteBigEndian(ushort value, byte* span)
        {
            unchecked
            {
                span[0] = (byte)(value >> 8);
                span[1] = (byte)value;
            }
        }

        /// <summary>
        /// 以大端序写入 32 位无符号整数。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="span">目标缓冲区指针（至少 4 字节）。</param>
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

        #endregion String

        /// <summary>
        /// 写入扩展格式头部信息。
        /// </summary>
        /// <param name="buffer">Bytebuffer对象，用于写入数据。</param>
        /// <param name="extensionHeader">要写入的扩展头部信息。</param>
        /// <remarks>
        /// 在编写数据头部信息时，同时请求足够空间来存储后续有效载荷数据的策略。 这样做的目的是为了提高程序的效率，通过减少内存分配的次数来避免潜在的性能问题。
        /// </remarks>
        public void WriteExtensionFormatHeader(ref ByteBuffer buffer, ExtensionHeader extensionHeader)
        {
            //在编写数据头部信息时，同时请求足够空间来存储后续有效载荷数据的策略。
            //这样做的目的是为了提高程序的效率，通过减少内存分配的次数来避免潜在的性能问题。
            Span<byte> span = buffer.GetSpan((int)(extensionHeader.Length + 6));
            AssumesTrue(_binaryConvert.TryWriteExtensionFormatHeader(span, extensionHeader, out int written));
            buffer.WriteAdvance(written);
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