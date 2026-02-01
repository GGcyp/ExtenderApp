using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary
{
    public partial class ByteBufferConvert
    {
        #region Skip

        /// <summary>
        /// 跳过 ref Bytebuffer 中的数据，如果不成功则抛出异常。
        /// </summary>
        /// <param name="buffer">要读取的 ref Bytebuffer 对象。</param>
        /// <exception cref="InsufficientBufferException">如果缓冲区不足，则抛出此异常。</exception>
        public void Skip(ref ByteBuffer buffer)
            => ThrowInsufficientBufferUnless(TrySkip(ref buffer));

        /// <summary>
        /// 尝试跳过 ref Bytebuffer 中的数据。
        /// </summary>
        /// <param name="buffer">要读取的 ref Bytebuffer 对象。</param>
        /// <returns>如果成功跳过数据，则返回 true；否则返回 false。</returns>
        internal bool TrySkip(ref ByteBuffer buffer)
        {
            if (buffer.Remaining == 0)
            {
                return false;
            }

            byte code = buffer.NextCode;

            if (_binaryConvert.IsPositiveFixInt(code) ||
                _binaryConvert.IsNegativeFixInt(code) ||
                code == BinaryCode.Nil ||
                code == BinaryCode.True ||
                code == BinaryCode.False)
            {
                return buffer.TryReadAdvance(1);
            }
            else if (code == BinaryCode.Int8 ||
                code == BinaryCode.UInt8)
            {
                return buffer.TryReadAdvance(2);
            }
            else if (code == BinaryCode.Int16 ||
                code == BinaryCode.UInt16)
            {
                return buffer.TryReadAdvance(3);
            }
            else if (code == BinaryCode.Int32 ||
                code == BinaryCode.UInt32 ||
                code == BinaryCode.Float32)
            {
                return buffer.TryReadAdvance(5);
            }
            else if (code == BinaryCode.Int64 ||
                code == BinaryCode.UInt64 ||
                code == BinaryCode.Float64)
            {
                return buffer.TryReadAdvance(9);
            }
            else if (_binaryConvert.IsFixMap(code) ||
                code == BinaryCode.Map16 ||
                code == BinaryCode.Map32)
            {
                return TrySkipNextMap(ref buffer);
            }
            else if (_binaryConvert.IsFixArray(code) ||
                code == BinaryCode.Array16 ||
                code == BinaryCode.Array32)
            {
                return TrySkipNextArray(ref buffer);
            }
            else if (_binaryConvert.IsFixStr(code) ||
                code == BinaryCode.Str8 ||
                code == BinaryCode.Str16 ||
                code == BinaryCode.Str32)
            {
                return TryStringLength(ref buffer);
            }
            else if (code == BinaryCode.Bin8 ||
                code == BinaryCode.Bin16 ||
                code == BinaryCode.Bin32)
            {
                return TryBytesLength(ref buffer);
            }

            throw new FileNotFoundException(string.Format("没有找到这个二进制所代表的意思：{0}", code));
        }

        private bool TrySkip(ref ByteBuffer buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!TrySkip(ref buffer))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Array

        /// <summary>
        /// 尝试跳过下一个数组
        /// </summary>
        /// <param name="buffer">Bytebuffer对象，通过ref传递以修改其状态</param>
        /// <returns>如果成功跳过数组，则返回true；否则返回false</returns>
        private bool TrySkipNextArray(ref ByteBuffer buffer)
            => TryArrayHeader(ref buffer, out int count) && TrySkip(ref buffer, count);

        /// <summary>
        /// 尝试读取数组头部信息
        /// </summary>
        /// <param name="buffer">Bytebuffer对象，通过ref传递以读取其数据</param>
        /// <param name="count">输出参数，存储数组元素的数量</param>
        /// <returns>如果成功读取数组头部，则返回true；否则返回false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryArrayHeader(ref ByteBuffer buffer, out int count)
        => TryReadArrayHeader(ref buffer, out count);

        /// <summary>
        /// 读取字节序列
        /// </summary>
        /// <param name="buffer">Bytebuffer对象，通过ref传递以读取其数据</param>
        /// <returns>如果读取成功，则返回包含读取字节的ReadOnlySequence<byte>对象；如果读取到nil，则返回null</returns>
        public ReadOnlySequence<byte> ReadBytes(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return ReadOnlySequence<byte>.Empty;
            }

            TryBytesLength(ref buffer, out uint length);
            ThrowInsufficientBufferUnless(buffer.Remaining >= length);
            ReadOnlySequence<byte> result = buffer.Sequence.Slice(buffer.Position, length);
            buffer.ReadAdvance(length);
            return result;
        }

        #endregion

        #region Map

        private bool TrySkipNextMap(ref ByteBuffer buffer)
            => TryMapHeader(ref buffer, out int count) && TrySkip(ref buffer, count * 2);

        public bool TryMapHeader(ref ByteBuffer buffer, out int count)
        {
            DecodeResult readResult = _binaryConvert.TryReadMapHeader(buffer.UnreadSpan, out uint uintCount, out int tokenSize);
            count = checked((int)uintCount);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref buffer, readResult, ref count, ref tokenSize);

            bool SlowPath(ref ByteBuffer buffer, DecodeResult readResult, ref int count, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadMapHeader(span, out uint uintCount, out tokenSize);
                            count = checked((int)uintCount);
                            return SlowPath(ref buffer, readResult, ref count, ref tokenSize);
                        }
                        else
                        {
                            count = 0;
                            return false;
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        #endregion

        #region String

        private bool TryStringLength(ref ByteBuffer buffer)
            => TryStringLength(ref buffer, out uint length) && buffer.TryReadAdvance(length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryStringLength(ref ByteBuffer buffer, out uint length)
        {
            DecodeResult readResult = _binaryConvert.TryReadStringHeader(buffer.UnreadSpan, out length, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref buffer, readResult, ref length, ref tokenSize);

            bool SlowPath(ref ByteBuffer buffer, DecodeResult readResult, ref uint length, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadStringHeader(span, out length, out tokenSize);
                            return SlowPath(ref buffer, readResult, ref length, ref tokenSize);
                        }
                        else
                        {
                            length = default;
                            return false;
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        #endregion

        #region Bin

        private bool TryBytesLength(ref ByteBuffer buffer)
            => TryBytesLength(ref buffer, out uint length) && buffer.TryReadAdvance(length);

        private bool TryBytesLength(ref ByteBuffer buffer, out uint length)
        {
            bool usingBinaryHeader = true;
            DecodeResult readResult = _binaryConvert.TryReadBinHeader(buffer.UnreadSpan, out length, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref buffer, readResult, usingBinaryHeader, ref length, ref tokenSize);

            bool SlowPath(ref ByteBuffer buffer, DecodeResult readResult, bool usingBinaryHeader, ref uint length, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        if (usingBinaryHeader)
                        {
                            usingBinaryHeader = false;
                            readResult = _binaryConvert.TryReadStringHeader(buffer.UnreadSpan, out length, out tokenSize);
                            return SlowPath(ref buffer, readResult, usingBinaryHeader, ref length, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                        }

                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = usingBinaryHeader
                                ? _binaryConvert.TryReadBinHeader(span, out length, out tokenSize)
                                : _binaryConvert.TryReadStringHeader(span, out length, out tokenSize);
                            return SlowPath(ref buffer, readResult, usingBinaryHeader, ref length, ref tokenSize);
                        }
                        else
                        {
                            length = default;
                            return false;
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// 读取Nil值。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>如果读取到Nil值，则返回true；否则返回false。</returns>
        public bool ReadNil(ref ByteBuffer buffer)
        {
            ThrowInsufficientBufferUnless(buffer.TryRead(out var code));

            return code == BinaryCode.Nil;
        }

        /// <summary>
        /// 尝试读取Nil值。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>如果成功读取到Nil值，则返回true；否则返回false。</returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadNil(ref ByteBuffer buffer)
        {
            if (buffer.NextCode == BinaryCode.Nil)
            {
                ReadNil(ref buffer);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 读取指定长度的原始数据。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <param name="length">要读取的长度。</param>
        /// <returns>读取到的原始数据。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取的数据不足指定长度时抛出。</exception>
        public ReadOnlySequence<byte> ReadRaw(ref ByteBuffer buffer, long length)
        {
            try
            {
                ReadOnlySequence<byte> result = buffer.Sequence.Slice(buffer.Position, length);
                buffer.TryReadAdvance(length);
                return result;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw ThrowNotEnoughBytesException(ex);
            }
        }

        /// <summary>
        /// 读取剩余的原始数据。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>读取到的原始数据。</returns>
        public ReadOnlySequence<byte> ReadRaw(ref ByteBuffer buffer)
        {
            SequencePosition initialPosition = buffer.Position;
            Skip(ref buffer);
            return buffer.Sequence.Slice(initialPosition, buffer.Position);
        }

        /// <summary>
        /// 读取数组头部信息。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>数组的长度。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当剩余数据不足以表示数组长度或数组元素时抛出。</exception>
        public int ReadArrayHeader(ref ByteBuffer buffer)
        {
            ThrowInsufficientBufferUnless(TryReadArrayHeader(ref buffer, out int count));

            //防止损坏或恶意的数据，这些数据可能会导致分配过多的内存。
            //我们允许每个基元的大小最小为1个字节。
            //知道每个元素较大的格式化程序可以选择添加更强的检查。
            ThrowInsufficientBufferUnless(buffer.Remaining >= count);

            return count;
        }

        /// <summary>
        /// 尝试读取数组头部信息。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <param name="count">数组的长度。</param>
        /// <returns>如果成功读取到数组头部信息，则返回true；否则返回false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadArrayHeader(ref ByteBuffer buffer, out int count)
        {
            DecodeResult readResult = _binaryConvert.TryReadArrayHeader(buffer.UnreadSpan, out uint uintCount, out int tokenSize);
            count = checked((int)uintCount);
            if (readResult == DecodeResult.Success)
            {
                buffer.TryReadAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref buffer, readResult, ref count, ref tokenSize);

            bool SlowPath(ref ByteBuffer buffer, DecodeResult readResult, ref int count, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadArrayHeader(span, out uint uintCount, out tokenSize);
                            count = checked((int)uintCount);
                            return SlowPath(ref buffer, readResult, ref count, ref tokenSize);
                        }
                        else
                        {
                            count = 0;
                            return false;
                        }

                    default:
                        throw new Exception(string.Format("无法解析的指令:{0}", readResult));
                }
            }
        }

        /// <summary>
        /// 读取映射头部信息。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>映射中的键值对数量。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当剩余数据不足以表示映射长度或映射元素时抛出。</exception>
        public int ReadMapHeader(ref ByteBuffer buffer)
        {
            ThrowInsufficientBufferUnless(TryReadMapHeader(ref buffer, out int count));

            // 防止因损坏或恶意数据导致分配过多内存。
            // 我们允许每个基本数据类型至少占用1个字节的大小，并且我们有一个key=value映射，因此总共是2个字节。
            // 知道每个元素更大的格式化程序可以选择添加更强的检查。
            ThrowInsufficientBufferUnless(buffer.Remaining >= count * 2);

            return count;
        }

        /// <summary>
        /// 尝试读取映射头部信息。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <param name="count">映射中的键值对数量。</param>
        /// <returns>如果成功读取到映射头部信息，则返回true；否则返回false。</returns>
        public bool TryReadMapHeader(ref ByteBuffer buffer, out int count)
        {
            DecodeResult readResult = _binaryConvert.TryReadMapHeader(buffer.UnreadSpan, out uint uintCount, out int tokenSize);
            count = checked((int)uintCount);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref buffer, readResult, ref count, ref tokenSize);

            bool SlowPath(ref ByteBuffer buffer, DecodeResult readResult, ref int count, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadMapHeader(span, out uint uintCount, out tokenSize);
                            count = checked((int)uintCount);
                            return SlowPath(ref buffer, readResult, ref count, ref tokenSize);
                        }
                        else
                        {
                            count = 0;
                            return false;
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        /// <summary>
        /// 读取布尔值。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>读取到的布尔值。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的代码不是True或False时抛出。</exception>
        public bool ReadBoolean(ref ByteBuffer buffer)
        {
            ThrowInsufficientBufferUnless(buffer.TryRead(out byte code));

            if (code == BinaryCode.True)
            {
                return true;
            }
            else if (code == BinaryCode.False)
            {
                return false;
            }

            throw ThrowInvalidCode(code);
        }

        /// <summary>
        /// 读取字符。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>读取到的字符。</returns>
        public char ReadChar(ref ByteBuffer buffer) => (char)ReadUInt16(ref buffer);

        /// <summary>
        /// 读取单精度浮点数。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>读取到的单精度浮点数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示单精度浮点数时抛出。</exception>
        public unsafe float ReadSingle(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadSingle(buffer.UnreadSpan, out float value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            float SlowPath(ref ByteBuffer buffer, DecodeResult readResult, float value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadSingle(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        /// <summary>
        /// 读取双精度浮点数。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>读取到的双精度浮点数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示双精度浮点数时抛出。</exception>
        public unsafe double ReadDouble(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadDouble(buffer.UnreadSpan, out double value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            double SlowPath(ref ByteBuffer buffer, DecodeResult readResult, double value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadDouble(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        /// <summary>
        /// 读取日期时间。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>读取到的日期时间。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示日期时间时抛出。</exception>
        public DateTime ReadDateTime(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadDateTime(buffer.UnreadSpan, out DateTime value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            DateTime SlowPath(ref ByteBuffer buffer, DecodeResult readResult, DateTime value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadDateTime(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        #endregion

        #region Int

        public Byte ReadByte(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadByte(buffer.UnreadSpan, out Byte value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            Byte SlowPath(ref ByteBuffer buffer, DecodeResult readResult, Byte value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadByte(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        public UInt16 ReadUInt16(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadUInt16(buffer.UnreadSpan, out UInt16 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            UInt16 SlowPath(ref ByteBuffer buffer, DecodeResult readResult, UInt16 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadUInt16(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        public UInt32 ReadUInt32(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadUInt32(buffer.UnreadSpan, out UInt32 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            UInt32 SlowPath(ref ByteBuffer buffer, DecodeResult readResult, UInt32 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadUInt32(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        public UInt64 ReadUInt64(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadUInt64(buffer.UnreadSpan, out UInt64 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            UInt64 SlowPath(ref ByteBuffer buffer, DecodeResult readResult, UInt64 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadUInt64(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        public SByte ReadSByte(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadSByte(buffer.UnreadSpan, out SByte value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            SByte SlowPath(ref ByteBuffer buffer, DecodeResult readResult, SByte value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadSByte(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        public Int16 ReadInt16(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadInt16(buffer.UnreadSpan, out Int16 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            Int16 SlowPath(ref ByteBuffer buffer, DecodeResult readResult, Int16 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadInt16(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        public Int32 ReadInt32(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadInt32(buffer.UnreadSpan, out Int32 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            Int32 SlowPath(ref ByteBuffer buffer, DecodeResult readResult, Int32 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadInt32(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        public Int64 ReadInt64(ref ByteBuffer buffer)
        {
            DecodeResult readResult = _binaryConvert.TryReadInt64(buffer.UnreadSpan, out Int64 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                buffer.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref buffer, readResult, value, ref tokenSize);

            Int64 SlowPath(ref ByteBuffer buffer, DecodeResult readResult, Int64 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        buffer.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadInt64(span, out value, out tokenSize);
                            return SlowPath(ref buffer, readResult, value, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowNotEnoughBytesException();
                        }

                    default:
                        throw ThrowUnreachable();
                }
            }
        }

        #endregion

        #region String

        /// <summary>
        /// 将 UTF-8 编码的字节数组转换为字符串。
        /// </summary>
        /// <param name="bytes">包含 UTF-8 编码字节的 ReadOnlySpan<byte>。</param>
        /// <returns>返回转换后的字符串。</returns>
        public unsafe string UTF8ToString(ReadOnlySpan<byte> bytes)
        {
            return _binaryConvert.Utf8ToString(bytes);
        }

        /// <summary>
        /// 将UTF-8编码的字节数组转换为字符数组。
        /// </summary>
        /// <param name="bytes">指向UTF-8编码字节数组的指针。</param>
        /// <param name="byteCount">字节数组的长度。</param>
        /// <param name="chars">指向字符数组的指针。</param>
        /// <param name="charCount">字符数组的长度。</param>
        /// <returns>转换后的字符数。</returns>
        public unsafe int UTF8ToChars(byte* bytes, int byteCount, char* chars, int charCount)
        {
            return _binaryConvert.UTF8ToChars(bytes, byteCount, chars, charCount);
        }

        /// <summary>
        /// 尝试从<see cref="ByteBuffer"/>中读取字符串范围。
        /// </summary>
        /// <param name="buffer">包含二进制数据的<see cref="ByteBuffer"/>。</param>
        /// <param name="span">读取到的字符串范围。</param>
        /// <returns>如果成功读取到字符串范围，则返回true；否则返回false。</returns>
        public bool TryReadStringSpan(ref ByteBuffer buffer, out ReadOnlySpan<byte> span)
        {
            if (TryReadNil(ref buffer))
            {
                span = default;
                return false;
            }

            long oldPosition = buffer.Consumed;
            int length = checked((int)GetStringLengthInBuffer(ref buffer));
            ThrowInsufficientBufferUnless(buffer.Remaining >= length);

            if (buffer.CurrentSpanIndex + length <= buffer.CurrentSpan.Length)
            {
                span = buffer.CurrentSpan.Slice(buffer.CurrentSpanIndex, length);
                buffer.ReadAdvance(length);
                return true;
            }
            else
            {
                buffer.Rewind(buffer.Consumed - oldPosition);
                span = default;
                return false;
            }
        }

        /// <summary>
        /// 获取字符串的长度（以字节为单位）。
        /// </summary>
        /// <param name="buffer">Bytebuffer对象，用于读取数据。</param>
        /// <returns>返回字符串的长度（以字节为单位）。</returns>
        /// <exception cref="Exception">如果缓冲区不足，则抛出异常。</exception>
        private uint GetStringLengthInBuffer(ref ByteBuffer buffer)
        {
            // 如果缓冲区不足，则抛出异常
            ThrowInsufficientBufferUnless(TryGetStringLengthInBytes(ref buffer, out uint length));
            return length;
        }

        /// <summary>
        /// 尝试获取字符串的长度（以字节为单位）。
        /// </summary>
        /// <param name="buffer">Bytebuffer对象，用于读取数据。</param>
        /// <param name="length">输出参数，存储字符串的长度（以字节为单位）。</param>
        /// <returns>如果成功获取长度，则返回true；否则返回false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetStringLengthInBytes(ref ByteBuffer buffer, out uint length)
        {
            // 尝试读取字符串头部并获取长度
            DecodeResult readResult = _binaryConvert.TryReadStringHeader(buffer.UnreadSpan, out length, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                // 成功读取，移动读取器指针
                buffer.ReadAdvance(tokenSize);
                return true;
            }

            // 调用慢路径处理
            return SlowPath(ref buffer, readResult, ref length, ref tokenSize);

            bool SlowPath(ref ByteBuffer buffer, DecodeResult readResult, ref uint length, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        // 成功读取，移动读取器指针
                        buffer.ReadAdvance(tokenSize);
                        return true;

                    case DecodeResult.TokenMismatch:
                        // 标记不匹配，抛出异常
                        throw ThrowInvalidCode(buffer.UnreadSpan[0]);

                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        // 缓冲区不足，尝试复制缓冲区并重新读取
                        Span<byte> span = stackalloc byte[tokenSize];
                        if (buffer.TryCopyTo(span))
                        {
                            readResult = _binaryConvert.TryReadStringHeader(span, out length, out tokenSize);
                            return SlowPath(ref buffer, readResult, ref length, ref tokenSize);
                        }
                        else
                        {
                            // 无法复制缓冲区，返回false
                            length = default;
                            return false;
                        }

                    default:
                        // 不可达路径，抛出异常
                        throw ThrowUnreachable();
                }
            }
        }

        #endregion

        #region Exception

        /// <summary>
        /// 除非满足指定条件，否则抛出 <see cref="EndOfStreamException"/> 异常。
        /// </summary>
        /// <param name="condition">要检查的条件。</param>
        /// <exception cref="EndOfStreamException">如果 <paramref name="condition"/> 为 <c>false</c>，则抛出此异常。</exception>
        private void ThrowInsufficientBufferUnless(bool condition)
        {
            if (!condition)
            {
                throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// 抛出无效代码异常。
        /// </summary>
        /// <param name="code">无效的代码值。</param>
        /// <returns>不会返回任何值，因为该方法总是抛出异常。</returns>
        /// <exception cref="KeyNotFoundException">当传入无效代码时抛出，包含错误信息的异常。</exception>
        [DoesNotReturn]
        private Exception ThrowInvalidCode(byte code)
        {
            throw new KeyNotFoundException(string.Format("未注册的二进制代码值：{0}", code));
        }

        /// <summary>
        /// 抛出字节不足异常。
        /// </summary>
        /// <param name="innerException">内部异常对象。</param>
        /// <returns>返回一个 <see cref="EndOfStreamException"/> 异常。</returns>
        /// <exception cref="EndOfStreamException">当字节不足时抛出。</exception>
        private EndOfStreamException ThrowNotEnoughBytesException(Exception innerException)
            => throw new EndOfStreamException(new EndOfStreamException().Message, innerException);

        /// <summary>
        /// 抛出字节不足异常。
        /// </summary>
        /// <returns>返回一个 <see cref="EndOfStreamException"/> 异常。</returns>
        /// <exception cref="EndOfStreamException">当字节不足时抛出。</exception>
        private EndOfStreamException ThrowNotEnoughBytesException()
            => throw new EndOfStreamException();

        /// <summary>
        /// 抛出一个无法到达的异常。
        /// </summary>
        /// <returns>不会返回任何值，因为会直接抛出异常。</returns>
        /// <exception cref="Exception">抛出包含消息“无法解析的指令”的异常。</exception>
        [DoesNotReturn]
        private static Exception ThrowUnreachable()
            => throw new Exception("无法解析的指令");

        #endregion
    }
}
