using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary
{
    public partial class ByteBlockConvert
    {
        #region Skip

        /// <summary>
        /// 跳过 ref ByteBlock 中的数据，如果不成功则抛出异常。
        /// </summary>
        /// <param name="block">要读取的 ref ByteBlock 对象。</param>
        /// <exception cref="InsufficientBufferException">如果缓冲区不足，则抛出此异常。</exception>
        public void Skip(ref ByteBlock block)
            => ThrowInsufficientBufferUnless(TrySkip(ref block));

        /// <summary>
        /// 尝试跳过 ref ByteBlock 中的数据。
        /// </summary>
        /// <param name="block">要读取的 ref ByteBlock 对象。</param>
        /// <returns>如果成功跳过数据，则返回 true；否则返回 false。</returns>
        internal bool TrySkip(ref ByteBlock block)
        {
            if (block.Remaining == 0)
            {
                return false;
            }

            byte code = block.NextCode;

            if (_binaryConvert.IsPositiveFixInt(code) ||
                _binaryConvert.IsNegativeFixInt(code) ||
                code == BinaryCode.Nil ||
                code == BinaryCode.True ||
                code == BinaryCode.False)
            {
                return block.TryReadAdvance(1);
            }
            else if (code == BinaryCode.Int8 ||
                code == BinaryCode.UInt8)
            {
                return block.TryReadAdvance(2);
            }
            else if (code == BinaryCode.Int16 ||
                code == BinaryCode.UInt16)
            {
                return block.TryReadAdvance(3);
            }
            else if (code == BinaryCode.Int32 ||
                code == BinaryCode.UInt32 ||
                code == BinaryCode.Float32)
            {
                return block.TryReadAdvance(5);
            }
            else if (code == BinaryCode.Int64 ||
                code == BinaryCode.UInt64 ||
                code == BinaryCode.Float64)
            {
                return block.TryReadAdvance(9);
            }
            else if (_binaryConvert.IsFixMap(code) ||
                code == BinaryCode.Map16 ||
                code == BinaryCode.Map32)
            {
                return TrySkipNextMap(ref block);
            }
            else if (_binaryConvert.IsFixArray(code) ||
                code == BinaryCode.Array16 ||
                code == BinaryCode.Array32)
            {
                return TrySkipNextArray(ref block);
            }
            else if (_binaryConvert.IsFixStr(code) ||
                code == BinaryCode.Str8 ||
                code == BinaryCode.Str16 ||
                code == BinaryCode.Str32)
            {
                return TryStringLength(ref block);
            }
            else if (code == BinaryCode.Bin8 ||
                code == BinaryCode.Bin16 ||
                code == BinaryCode.Bin32)
            {
                return TryBytesLength(ref block);
            }
            else if (code == BinaryCode.FixExt1 ||
                code == BinaryCode.FixExt2 ||
                code == BinaryCode.FixExt4 ||
                code == BinaryCode.FixExt8 ||
                code == BinaryCode.FixExt16 ||
                code == BinaryCode.Ext8 ||
                code == BinaryCode.Ext16 ||
                code == BinaryCode.Ext32)
            {
                return TryExtensionHeader(ref block);
            }

            throw new FileNotFoundException(string.Format("没有找到这个二进制所代表的意思：{0}", code));
        }

        private bool TrySkip(ref ByteBlock block, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!TrySkip(ref block))
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
        /// <param name="block">ByteBlock对象，通过ref传递以修改其状态</param>
        /// <returns>如果成功跳过数组，则返回true；否则返回false</returns>
        private bool TrySkipNextArray(ref ByteBlock block)
            => TryArrayHeader(ref block, out int count) && TrySkip(ref block, count);

        /// <summary>
        /// 尝试读取数组头部信息
        /// </summary>
        /// <param name="block">ByteBlock对象，通过ref传递以读取其数据</param>
        /// <param name="count">输出参数，存储数组元素的数量</param>
        /// <returns>如果成功读取数组头部，则返回true；否则返回false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryArrayHeader(ref ByteBlock block, out int count)
        => TryReadArrayHeader(ref block, out count);

        /// <summary>
        /// 读取字节序列
        /// </summary>
        /// <param name="block">ByteBlock对象，通过ref传递以读取其数据</param>
        /// <returns>如果读取成功，则返回包含读取字节的ReadOnlySequence<byte>对象；如果读取到nil，则返回null</returns>
        public ReadOnlySequence<byte> ReadBytes(ref ByteBlock block)
        {
            if (TryReadNil(ref block))
            {
                return ReadOnlySequence<byte>.Empty;
            }

            TryBytesLength(ref block, out uint length);
            ThrowInsufficientBufferUnless(block.Remaining >= length);
            ReadOnlySequence<byte> result = block.Sequence.Slice(block.Position, length);
            block.ReadAdvance(length);
            return result;
        }

        #endregion

        #region Map

        private bool TrySkipNextMap(ref ByteBlock block)
            => TryMapHeader(ref block, out int count) && TrySkip(ref block, count * 2);

        public bool TryMapHeader(ref ByteBlock block, out int count)
        {
            DecodeResult readResult = _binaryConvert.TryReadMapHeader(block.UnreadSpan, out uint uintCount, out int tokenSize);
            count = checked((int)uintCount);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref block, readResult, ref count, ref tokenSize);

            bool SlowPath(ref ByteBlock self, DecodeResult readResult, ref int count, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        self.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(self.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (self.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadMapHeader(buffer, out uint uintCount, out tokenSize);
                            count = checked((int)uintCount);
                            return SlowPath(ref self, readResult, ref count, ref tokenSize);
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

        private bool TryStringLength(ref ByteBlock block)
            => TryStringLength(ref block, out uint length) && block.TryReadAdvance(length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryStringLength(ref ByteBlock block, out uint length)
        {
            DecodeResult readResult = _binaryConvert.TryReadStringHeader(block.UnreadSpan, out length, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref block, readResult, ref length, ref tokenSize);

            bool SlowPath(ref ByteBlock block, DecodeResult readResult, ref uint length, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadStringHeader(buffer, out length, out tokenSize);
                            return SlowPath(ref block, readResult, ref length, ref tokenSize);
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

        private bool TryBytesLength(ref ByteBlock block)
            => TryBytesLength(ref block, out uint length) && block.TryReadAdvance(length);

        private bool TryBytesLength(ref ByteBlock block, out uint length)
        {
            bool usingBinaryHeader = true;
            DecodeResult readResult = _binaryConvert.TryReadBinHeader(block.UnreadSpan, out length, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref block, readResult, usingBinaryHeader, ref length, ref tokenSize);

            bool SlowPath(ref ByteBlock block, DecodeResult readResult, bool usingBinaryHeader, ref uint length, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        if (usingBinaryHeader)
                        {
                            usingBinaryHeader = false;
                            readResult = _binaryConvert.TryReadStringHeader(block.UnreadSpan, out length, out tokenSize);
                            return SlowPath(ref block, readResult, usingBinaryHeader, ref length, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowInvalidCode(block.UnreadSpan[0]);
                        }

                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = usingBinaryHeader
                                ? _binaryConvert.TryReadBinHeader(buffer, out length, out tokenSize)
                                : _binaryConvert.TryReadStringHeader(buffer, out length, out tokenSize);
                            return SlowPath(ref block, readResult, usingBinaryHeader, ref length, ref tokenSize);
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

        #region ExtensionHeader

        /// <summary>
        /// 尝试读取扩展头，并尝试前进读取器到扩展头的长度位置
        /// </summary>
        /// <param name="block">二进制读取器</param>
        /// <returns>如果成功读取扩展头并成功前进读取器到扩展头的长度位置，则返回true；否则返回false</returns>
        private bool TryExtensionHeader(ref ByteBlock block)
                    => TryExtensionHeader(ref block, out ExtensionHeader header) && block.TryReadAdvance(header.Length);

        /// <summary>
        /// 尝试从给定的二进制读取器中读取扩展头。
        /// </summary>
        /// <param name="block">二进制读取器。</param>
        /// <param name="extensionHeader">读取的扩展头。</param>
        /// <returns>如果成功读取扩展头，则返回true；否则返回false。</returns>
        /// <remarks>
        /// 该方法首先尝试使用快速路径读取扩展头。如果快速路径失败，则调用慢速路径。
        /// 快速路径和慢速路径都会处理不同的解码结果，并根据结果执行相应的操作。
        /// </remarks>
        public bool TryExtensionHeader(ref ByteBlock block, out ExtensionHeader extensionHeader)
        {
            DecodeResult readResult = _binaryConvert.TryReadExtensionHeader(block.UnreadSpan, out extensionHeader, out int tokenSize);

            return SlowPath(ref block, readResult, ref extensionHeader, ref tokenSize);

            bool SlowPath(ref ByteBlock block, DecodeResult readResult, ref ExtensionHeader extensionHeader, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        //throw ThrowInvalidCode(block.UnreadSpan[0]);
                        return false;
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadExtensionHeader(buffer, out extensionHeader, out tokenSize);
                            return SlowPath(ref block, readResult, ref extensionHeader, ref tokenSize);
                        }
                        else
                        {
                            extensionHeader = default;
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
        /// <param name="block">二进制读取器。</param>
        /// <returns>如果读取到Nil值，则返回true；否则返回false。</returns>
        public bool ReadNil(ref ByteBlock block)
        {
            ThrowInsufficientBufferUnless(block.TryRead(out var code));

            return code == BinaryCode.Nil;
        }

        /// <summary>
        /// 尝试读取Nil值。
        /// </summary>
        /// <param name="block">二进制读取器。</param>
        /// <returns>如果成功读取到Nil值，则返回true；否则返回false。</returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadNil(ref ByteBlock block)
        {
            if (block.NextCode == BinaryCode.Nil)
            {
                ReadNil(ref block);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 读取指定长度的原始数据。
        /// </summary>
        /// <param name="block">二进制读取器。</param>
        /// <param name="length">要读取的长度。</param>
        /// <returns>读取到的原始数据。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取的数据不足指定长度时抛出。</exception>
        public ReadOnlySequence<byte> ReadRaw(ref ByteBlock block, long length)
        {
            try
            {
                ReadOnlySequence<byte> result = block.Sequence.Slice(block.Position, length);
                block.TryReadAdvance(length);
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
        /// <param name="block">二进制读取器。</param>
        /// <returns>读取到的原始数据。</returns>
        public ReadOnlySequence<byte> ReadRaw(ref ByteBlock block)
        {
            SequencePosition initialPosition = block.Position;
            Skip(ref block);
            return block.Sequence.Slice(initialPosition, block.Position);
        }

        /// <summary>
        /// 读取数组头部信息。
        /// </summary>
        /// <param name="block">二进制读取器。</param>
        /// <returns>数组的长度。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当剩余数据不足以表示数组长度或数组元素时抛出。</exception>
        public int ReadArrayHeader(ref ByteBlock block)
        {
            ThrowInsufficientBufferUnless(TryReadArrayHeader(ref block, out int count));

            //防止损坏或恶意的数据，这些数据可能会导致分配过多的内存。
            //我们允许每个基元的大小最小为1个字节。
            //知道每个元素较大的格式化程序可以选择添加更强的检查。
            ThrowInsufficientBufferUnless(block.Remaining >= count);

            return count;
        }

        /// <summary>
        /// 尝试读取数组头部信息。
        /// </summary>
        /// <param name="block">二进制读取器。</param>
        /// <param name="count">数组的长度。</param>
        /// <returns>如果成功读取到数组头部信息，则返回true；否则返回false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadArrayHeader(ref ByteBlock block, out int count)
        {
            DecodeResult readResult = _binaryConvert.TryReadArrayHeader(block.UnreadSpan, out uint uintCount, out int tokenSize);
            count = checked((int)uintCount);
            if (readResult == DecodeResult.Success)
            {
                block.TryReadAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref block, readResult, ref count, ref tokenSize);

            bool SlowPath(ref ByteBlock block, DecodeResult readResult, ref int count, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadArrayHeader(buffer, out uint uintCount, out tokenSize);
                            count = checked((int)uintCount);
                            return SlowPath(ref block, readResult, ref count, ref tokenSize);
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
        /// <param name="block">二进制读取器。</param>
        /// <returns>映射中的键值对数量。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当剩余数据不足以表示映射长度或映射元素时抛出。</exception>
        public int ReadMapHeader(ref ByteBlock block)
        {
            ThrowInsufficientBufferUnless(TryReadMapHeader(ref block, out int count));

            // 防止因损坏或恶意数据导致分配过多内存。
            // 我们允许每个基本数据类型至少占用1个字节的大小，并且我们有一个key=value映射，因此总共是2个字节。
            // 知道每个元素更大的格式化程序可以选择添加更强的检查。
            ThrowInsufficientBufferUnless(block.Remaining >= count * 2);

            return count;
        }

        /// <summary>
        /// 尝试读取映射头部信息。
        /// </summary>
        /// <param name="block">二进制读取器。</param>
        /// <param name="count">映射中的键值对数量。</param>
        /// <returns>如果成功读取到映射头部信息，则返回true；否则返回false。</returns>
        public bool TryReadMapHeader(ref ByteBlock block, out int count)
        {
            DecodeResult readResult = _binaryConvert.TryReadMapHeader(block.UnreadSpan, out uint uintCount, out int tokenSize);
            count = checked((int)uintCount);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref block, readResult, ref count, ref tokenSize);

            bool SlowPath(ref ByteBlock block, DecodeResult readResult, ref int count, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadMapHeader(buffer, out uint uintCount, out tokenSize);
                            count = checked((int)uintCount);
                            return SlowPath(ref block, readResult, ref count, ref tokenSize);
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
        /// <param name="block">二进制读取器。</param>
        /// <returns>读取到的布尔值。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的代码不是True或False时抛出。</exception>
        public bool ReadBoolean(ref ByteBlock block)
        {
            ThrowInsufficientBufferUnless(block.TryRead(out byte code));

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
        /// <param name="block">二进制读取器。</param>
        /// <returns>读取到的字符。</returns>
        public char ReadChar(ref ByteBlock block) => (char)ReadUInt16(ref block);

        /// <summary>
        /// 读取单精度浮点数。
        /// </summary>
        /// <param name="block">二进制读取器。</param>
        /// <returns>读取到的单精度浮点数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示单精度浮点数时抛出。</exception>
        public unsafe float ReadSingle(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadSingle(block.UnreadSpan, out float value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            float SlowPath(ref ByteBlock block, DecodeResult readResult, float value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadSingle(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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
        /// <param name="block">二进制读取器。</param>
        /// <returns>读取到的双精度浮点数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示双精度浮点数时抛出。</exception>
        public unsafe double ReadDouble(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadDouble(block.UnreadSpan, out double value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            double SlowPath(ref ByteBlock block, DecodeResult readResult, double value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadDouble(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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
        /// <param name="block">二进制读取器。</param>
        /// <returns>读取到的日期时间。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示日期时间时抛出。</exception>
        public DateTime ReadDateTime(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadDateTime(block.UnreadSpan, out DateTime value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            DateTime SlowPath(ref ByteBlock block, DecodeResult readResult, DateTime value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadDateTime(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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
        /// 读取带有扩展头的日期时间。
        /// </summary>
        /// <param name="block">二进制读取器。</param>
        /// <param name="header">扩展头。</param>
        /// <returns>读取到的日期时间。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示日期时间时抛出。</exception>
        public DateTime ReadDateTime(ref ByteBlock block, ExtensionHeader header)
        {
            DecodeResult readResult = _binaryConvert.TryReadDateTime(block.UnreadSpan, header, out DateTime value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, header, readResult, value, ref tokenSize);

            DateTime SlowPath(ref ByteBlock block, ExtensionHeader header, DecodeResult readResult, DateTime value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadDateTime(buffer, header, out value, out tokenSize);
                            return SlowPath(ref block, header, readResult, value, ref tokenSize);
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

        public Byte ReadByte(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadByte(block.UnreadSpan, out Byte value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            Byte SlowPath(ref ByteBlock block, DecodeResult readResult, Byte value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadByte(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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

        public UInt16 ReadUInt16(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadUInt16(block.UnreadSpan, out UInt16 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            UInt16 SlowPath(ref ByteBlock block, DecodeResult readResult, UInt16 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadUInt16(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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

        public UInt32 ReadUInt32(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadUInt32(block.UnreadSpan, out UInt32 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            UInt32 SlowPath(ref ByteBlock block, DecodeResult readResult, UInt32 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadUInt32(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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

        public UInt64 ReadUInt64(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadUInt64(block.UnreadSpan, out UInt64 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            UInt64 SlowPath(ref ByteBlock block, DecodeResult readResult, UInt64 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadUInt64(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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

        public SByte ReadSByte(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadSByte(block.UnreadSpan, out SByte value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            SByte SlowPath(ref ByteBlock block, DecodeResult readResult, SByte value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadSByte(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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

        public Int16 ReadInt16(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadInt16(block.UnreadSpan, out Int16 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            Int16 SlowPath(ref ByteBlock block, DecodeResult readResult, Int16 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadInt16(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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

        public Int32 ReadInt32(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadInt32(block.UnreadSpan, out Int32 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            Int32 SlowPath(ref ByteBlock block, DecodeResult readResult, Int32 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadInt32(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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

        public Int64 ReadInt64(ref ByteBlock block)
        {
            DecodeResult readResult = _binaryConvert.TryReadInt64(block.UnreadSpan, out Int64 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                block.ReadAdvance(tokenSize);
                return value;
            }

            return SlowPath(ref block, readResult, value, ref tokenSize);

            Int64 SlowPath(ref ByteBlock block, DecodeResult readResult, Int64 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        block.ReadAdvance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(block.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadInt64(buffer, out value, out tokenSize);
                            return SlowPath(ref block, readResult, value, ref tokenSize);
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
        /// 尝试从<see cref="ByteBlock"/>中读取字符串范围。
        /// </summary>
        /// <param name="block">包含二进制数据的<see cref="ByteBlock"/>。</param>
        /// <param name="span">读取到的字符串范围。</param>
        /// <returns>如果成功读取到字符串范围，则返回true；否则返回false。</returns>
        public bool TryReadStringSpan(ref ByteBlock block, out ReadOnlySpan<byte> span)
        {
            if (TryReadNil(ref block))
            {
                span = default;
                return false;
            }

            long oldPosition = block.Consumed;
            int length = checked((int)GetStringLengthInBytes(ref block));
            ThrowInsufficientBufferUnless(block.Remaining >= length);

            if (block.CurrentSpanIndex + length <= block.CurrentSpan.Length)
            {
                span = block.CurrentSpan.Slice(block.CurrentSpanIndex, length);
                block.ReadAdvance(length);
                return true;
            }
            else
            {
                block.Rewind(block.Consumed - oldPosition);
                span = default;
                return false;
            }
        }

        /// <summary>
        /// 获取字符串的长度（以字节为单位）。
        /// </summary>
        /// <param name="block">ByteBlock对象，用于读取数据。</param>
        /// <returns>返回字符串的长度（以字节为单位）。</returns>
        /// <exception cref="Exception">如果缓冲区不足，则抛出异常。</exception>
        private uint GetStringLengthInBytes(ref ByteBlock block)
        {
            // 如果缓冲区不足，则抛出异常
            ThrowInsufficientBufferUnless(TryGetStringLengthInBytes(ref block, out uint length));
            return length;
        }

        /// <summary>
        /// 尝试获取字符串的长度（以字节为单位）。
        /// </summary>
        /// <param name="block">ByteBlock对象，用于读取数据。</param>
        /// <param name="length">输出参数，存储字符串的长度（以字节为单位）。</param>
        /// <returns>如果成功获取长度，则返回true；否则返回false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetStringLengthInBytes(ref ByteBlock block, out uint length)
        {
            // 尝试读取字符串头部并获取长度
            DecodeResult readResult = _binaryConvert.TryReadStringHeader(block.UnreadSpan, out length, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                // 成功读取，移动读取器指针
                block.ReadAdvance(tokenSize);
                return true;
            }

            // 调用慢路径处理
            return SlowPath(ref block, readResult, ref length, ref tokenSize);

            bool SlowPath(ref ByteBlock block, DecodeResult readResult, ref uint length, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        // 成功读取，移动读取器指针
                        block.ReadAdvance(tokenSize);
                        return true;

                    case DecodeResult.TokenMismatch:
                        // 标记不匹配，抛出异常
                        throw ThrowInvalidCode(block.UnreadSpan[0]);

                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        // 缓冲区不足，尝试复制缓冲区并重新读取
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (block.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadStringHeader(buffer, out length, out tokenSize);
                            return SlowPath(ref block, readResult, ref length, ref tokenSize);
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
