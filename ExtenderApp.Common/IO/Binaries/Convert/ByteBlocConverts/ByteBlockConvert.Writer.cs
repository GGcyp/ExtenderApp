using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary
{
    public partial class ByteBlockConvert
    {
        /// <summary>
        /// 写入一个空值（Nil）。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        public void WriteNil(ref ByteBlock block)
        {
            Span<byte> span = block.GetSpan(1);
            AssumesTrue(_binaryConvert.TryWriteNil(span, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个原始的二进制块。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="span">原始的二进制数据。</param>
        public void WriteRaw(ref ByteBlock block, ReadOnlySpan<byte> span)
            => block.Write(span);

        /// <summary>
        /// 写入一个原始的二进制序列。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="sequence">原始的二进制序列。</param>
        public void WriteRaw(ref ByteBlock block, in ReadOnlySequence<byte> sequence)
        {
            foreach (ReadOnlyMemory<byte> segment in sequence)
            {
                block.Write(segment.Span);
            }
        }

        /// <summary>
        /// 写入一个数组头，数组长度由整数指定。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="count">数组长度。</param>
        public void WriteArrayHeader(ref ByteBlock block, int count)
            => WriteArrayHeader(ref block, (uint)count);

        /// <summary>
        /// 写入一个数组头，数组长度由无符号整数指定。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="count">数组长度。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteArrayHeader(ref ByteBlock block, uint count)
        {
            Span<byte> span = block.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteArrayHeader(span, count, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个映射头，映射项数由整数指定。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="count">映射项数。</param>
        public void WriteMapHeader(ref ByteBlock block, int count)
            => WriteMapHeader(ref block, (uint)count);

        /// <summary>
        /// 写入一个映射头，映射项数由无符号整数指定。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="count">映射项数。</param>
        public void WriteMapHeader(ref ByteBlock block, uint count)
        {
            Span<byte> span = block.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteMapHeader(span, count, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个字节值。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBlock block, byte value)
        {
            Span<byte> span = block.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号8位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt8(ref ByteBlock block, byte value)
        {
            Span<byte> span = block.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWriteUInt8(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号8位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBlock block, sbyte value)
        {
            Span<byte> span = block.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号8位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt8(ref ByteBlock block, sbyte value)
        {
            Span<byte> span = block.GetSpan(2);
            AssumesTrue(_binaryConvert.TryWriteInt8(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号16位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBlock block, ushort value)
        {
            Span<byte> span = block.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号16位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt16(ref ByteBlock block, ushort value)
        {
            Span<byte> span = block.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWriteUInt16(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号16位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBlock block, short value)
        {
            Span<byte> span = block.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号16位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt16(ref ByteBlock block, short value)
        {
            Span<byte> span = block.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWriteInt16(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号32位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ref ByteBlock block, uint value)
        {
            Span<byte> span = block.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号32位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt32(ref ByteBlock block, uint value)
        {
            Span<byte> span = block.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteUInt32(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号32位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ref ByteBlock block, int value)
        {
            Span<byte> span = block.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号32位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt32(ref ByteBlock block, int value)
        {
            Span<byte> span = block.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWriteInt32(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号64位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBlock block, ulong value)
        {
            Span<byte> span = block.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个无符号64位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteUInt64(ref ByteBlock block, ulong value)
        {
            Span<byte> span = block.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWriteUInt64(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号64位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBlock block, long value)
        {
            Span<byte> span = block.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个有符号64位整数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void WriteInt64(ref ByteBlock block, long value)
        {
            Span<byte> span = block.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWriteInt64(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个布尔值。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBlock block, bool value)
        {
            Span<byte> span = block.GetSpan(1);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个字符。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBlock block, char value)
        {
            Span<byte> span = block.GetSpan(3);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个浮点数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBlock block, float value)
        {
            Span<byte> span = block.GetSpan(5);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个双精度浮点数。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="value">要写入的值。</param>
        public void Write(ref ByteBlock block, double value)
        {
            Span<byte> span = block.GetSpan(9);
            AssumesTrue(_binaryConvert.TryWrite(span, value, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个日期时间值。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="dateTime">要写入的值。</param>
        public void Write(ref ByteBlock block, DateTime dateTime)
        {
            Span<byte> span = block.GetSpan(15);
            AssumesTrue(_binaryConvert.TryWrite(span, dateTime, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个字节数组。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="src">要写入的字节数组。</param>
        public void Write(ref ByteBlock block, byte[]? src)
        {
            if (src == null)
            {
                WriteNil(ref block);
            }
            else
            {
                block.Write(src.AsSpan());
            }
        }

        /// <summary>
        /// 写入一个字节序列。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="src">要写入的字节序列。</param>
        public void Write(ref ByteBlock block, ReadOnlySpan<byte> src)
        {
            int length = (int)src.Length;
            WriteBinHeader(ref block, length);
            var span = block.GetSpan(length);
            src.CopyTo(span);
            block.WriteAdvance(length);
        }

        /// <summary>
        /// 写入一个字节序列。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="src">要写入的字节序列。</param>
        public void Write(ref ByteBlock block, in ReadOnlySequence<byte> src)
        {
            int length = (int)src.Length;
            WriteBinHeader(ref block, length);
            var span = block.GetSpan(length);
            src.CopyTo(span);
            block.WriteAdvance(length);
        }

        /// <summary>
        /// 将字符串写入到给定的 <see cref="ByteBlock"/> 中。
        /// </summary>
        /// <param name="block">
        /// <see cref="ByteBlock"/> 实例，用于写入数据。
        /// </param>
        /// <param name="value">
        /// 要写入的字符串。如果为 null，则写入 nil 值。
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ref ByteBlock block, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteNil(ref block);
                return;
            }

            ref byte buffer = ref WriteString_PrepareSpan(ref block, value.Length, out int bufferSize, out int useOffset);
            fixed (char* pValue = value)
            fixed (byte* pBuffer = &buffer)
            {
                int byteCount = _binaryConvert.BinaryEncoding.GetBytes(pValue, value.Length, pBuffer + useOffset, bufferSize);
                WriteString_PostEncoding(ref block, pBuffer, useOffset, byteCount);
            }
        }

        /// <summary>
        /// 写入一个二进制块头。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="length">二进制块长度。</param>
        public void WriteBinHeader(ref ByteBlock block, int length)
        {
            Span<byte> span = block.GetSpan(length + 5);
            AssumesTrue(_binaryConvert.TryWriteBinHeader(span, (uint)length, out int written));
            block.WriteAdvance(written);
        }

        /// <summary>
        /// 写入一个UTF-8字符串。
        /// </summary>
        /// <param name="block">
        /// BinaryWriter 实例。
        /// </param>
        /// <param name="utf8stringBytes">UTF-8编码的字符串字节序列。</param>
        public void WriteString(ref ByteBlock block, in ReadOnlySequence<byte> utf8stringBytes)
        {
            var length = (int)utf8stringBytes.Length;
            WriteStringHeader(ref block, length);
            Span<byte> span = block.GetSpan(length);
            utf8stringBytes.CopyTo(span);
            block.WriteAdvance(length);
        }

        /// <summary>
        /// 将UTF-8编码的字符串写入到BinaryWriter中。
        /// </summary>
        /// <param name="block">BinaryWriter实例。</param>
        /// <param name="utf8stringBytes">UTF-8编码的字符串的字节序列。</param>
        public void WriteString(ref ByteBlock block, ReadOnlySpan<byte> utf8stringBytes)
        {
            var length = utf8stringBytes.Length;
            WriteStringHeader(ref block, length);
            Span<byte> span = block.GetSpan(length);
            utf8stringBytes.CopyTo(span);
            block.WriteAdvance(length);
        }

        /// <summary>
        /// 向二进制写入器中写入字符串头部。
        /// </summary>
        /// <param name="block">二进制写入器。</param>
        /// <param name="byteCount">字符串的字节数。</param>
        /// <remarks>
        /// 此方法首先通过调用 <see
        /// cref="ByteBlock.GetSpan(int)"/>
        /// 方法获取一个足够大的字节跨度， 然后调用 <see
        /// cref="_binaryConvert.TryWriteStringHeader(Span{byte},
        /// uint, out int)"/> 方法尝试将字符串头部写入该跨度中。
        /// 如果写入成功，则通过调用 <see
        /// cref="ByteBlock.WriteAdvance(int)"/> 方法更新写入器的位置。
        /// </remarks>
        public void WriteStringHeader(ref ByteBlock block, int byteCount)
        {
            Span<byte> span = block.GetSpan(byteCount + 5);
            AssumesTrue(_binaryConvert.TryWriteStringHeader(span, (uint)byteCount, out int written));
            block.WriteAdvance(written);
        }

        #region String

        /// <summary>
        /// 准备字符串写入的缓冲区，并返回缓冲区指针。
        /// </summary>
        /// <param name="block">扩展的二进制写入器。</param>
        /// <param name="characterLength">字符串的字符长度。</param>
        /// <param name="bufferSize">缓冲区的大小。</param>
        /// <param name="encodedBytesOffset">
        /// 编码后的字节偏移量。
        /// </param>
        /// <returns>缓冲区指针。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref byte WriteString_PrepareSpan(ref ByteBlock block, int characterLength, out int bufferSize, out int encodedBytesOffset)
        {
            // 计算缓冲区大小
            bufferSize = _binaryConvert.BinaryEncoding.GetMaxByteCount(characterLength) + 5;
            // 获取缓冲区指针
            ref byte buffer = ref block.GetPointer(bufferSize);

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

        /// <summary>
        /// 在字符串编码完成后，按实际字节数写入前缀并调整主体位置，然后推进写入游标。
        /// </summary>
        /// <param name="block">目标写入器。</param>
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
        ///   cref="ByteBlock.WriteAdvance(int)"/> 推进相应总字节数。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void WriteString_PostEncoding(ref ByteBlock block, byte* pBuffer, int estimatedOffset, int byteCount)
        {
            // move body and write prefix
            if (byteCount <= _binaryConvert.BinaryOptions.BinaryRang.MaxFixStringLength)
            {
                if (estimatedOffset != 1)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 1, byteCount, byteCount);
                }

                pBuffer[0] = (byte)(_binaryConvert.BinaryOptions.BinaryCode.MinFixStr | byteCount);
                block.WriteAdvance(byteCount + 1);
            }
            else if (byteCount <= byte.MaxValue)
            {
                if (estimatedOffset != 2)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 2, byteCount, byteCount);
                }

                pBuffer[0] = _binaryConvert.BinaryOptions.BinaryCode.Str8;
                pBuffer[1] = unchecked((byte)byteCount);
                block.WriteAdvance(byteCount + 2);
            }
            else if (byteCount <= ushort.MaxValue)
            {
                if (estimatedOffset != 3)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 3, byteCount, byteCount);
                }

                pBuffer[0] = _binaryConvert.BinaryOptions.BinaryCode.Str16;
                WriteBigEndian((ushort)byteCount, pBuffer + 1);
                block.WriteAdvance(byteCount + 3);
            }
            else
            {
                if (estimatedOffset != 5)
                {
                    Buffer.MemoryCopy(pBuffer + estimatedOffset, pBuffer + 5, byteCount, byteCount);
                }

                pBuffer[0] = _binaryConvert.BinaryOptions.BinaryCode.Str32;
                WriteBigEndian((uint)byteCount, pBuffer + 1);
                block.WriteAdvance(byteCount + 5);
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
        /// <param name="block">ByteBlock对象，用于写入数据。</param>
        /// <param name="extensionHeader">要写入的扩展头部信息。</param>
        /// <remarks>
        /// 在编写数据头部信息时，同时请求足够空间来存储后续有效载荷数据的策略。 这样做的目的是为了提高程序的效率，通过减少内存分配的次数来避免潜在的性能问题。
        /// </remarks>
        public void WriteExtensionFormatHeader(ref ByteBlock block, ExtensionHeader extensionHeader)
        {
            //在编写数据头部信息时，同时请求足够空间来存储后续有效载荷数据的策略。
            //这样做的目的是为了提高程序的效率，通过减少内存分配的次数来避免潜在的性能问题。
            Span<byte> span = block.GetSpan((int)(extensionHeader.Length + 6));
            AssumesTrue(_binaryConvert.TryWriteExtensionFormatHeader(span, extensionHeader, out int written));
            block.WriteAdvance(written);
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