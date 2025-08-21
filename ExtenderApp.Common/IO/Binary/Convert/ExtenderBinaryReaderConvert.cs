using ExtenderApp.Data;

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Common.IO.Binaries
{
    public class ExtenderBinaryReaderConvert
    {
        private readonly BinaryConvert _binaryConvert;
        private readonly BinaryOptions _options;
        private BinaryCode binaryCode => _options.BinaryCode;

        public ExtenderBinaryReaderConvert(BinaryConvert binaryConvert, BinaryOptions options)
        {
            _binaryConvert = binaryConvert;
            _options = options;
        }

        #region Skip

        /// <summary>
        /// 跳过 ref ExtenderBinaryReader 中的数据，如果不成功则抛出异常。
        /// </summary>
        /// <param name="reader">要读取的 ref ExtenderBinaryReader 对象。</param>
        /// <exception cref="InsufficientBufferException">如果缓冲区不足，则抛出此异常。</exception>
        public void Skip(ref ExtenderBinaryReader reader)
            => ThrowInsufficientBufferUnless(TrySkip(ref reader));

        /// <summary>
        /// 尝试跳过 ref ExtenderBinaryReader 中的数据。
        /// </summary>
        /// <param name="reader">要读取的 ref ExtenderBinaryReader 对象。</param>
        /// <returns>如果成功跳过数据，则返回 true；否则返回 false。</returns>
        internal bool TrySkip(ref ExtenderBinaryReader reader)
        {
            if (reader.Remaining == 0)
            {
                return false;
            }

            byte code = reader.NextCode;


            if (_binaryConvert.IsPositiveFixInt(code) ||
                _binaryConvert.IsNegativeFixInt(code) ||
                code == binaryCode.Nil ||
                code == binaryCode.True ||
                code == binaryCode.False)
            {
                return reader.TryAdvance(1);
            }
            else if (code == binaryCode.Int8 ||
                code == binaryCode.UInt8)
            {
                return reader.TryAdvance(2);
            }
            else if (code == binaryCode.Int16 ||
                code == binaryCode.UInt16)
            {
                return reader.TryAdvance(3);
            }
            else if (code == binaryCode.Int32 ||
                code == binaryCode.UInt32 ||
                code == binaryCode.Float32)
            {
                return reader.TryAdvance(5);
            }
            else if (code == binaryCode.Int64 ||
                code == binaryCode.UInt64 ||
                code == binaryCode.Float64)
            {
                return reader.TryAdvance(9);
            }
            else if (_binaryConvert.IsFixMap(code) ||
                code == binaryCode.Map16 ||
                code == binaryCode.Map32)
            {
                return TrySkipNextMap(ref reader);
            }
            else if (_binaryConvert.IsFixArray(code) ||
                code == binaryCode.Array16 ||
                code == binaryCode.Array32)
            {
                return TrySkipNextArray(ref reader);
            }
            else if (_binaryConvert.IsFixStr(code) ||
                code == binaryCode.Str8 ||
                code == binaryCode.Str16 ||
                code == binaryCode.Str32)
            {
                return TryStringLength(ref reader);
            }
            else if (code == binaryCode.Bin8 ||
                code == binaryCode.Bin16 ||
                code == binaryCode.Bin32)
            {
                return TryBytesLength(ref reader);
            }
            else if (code == binaryCode.FixExt1 ||
                code == binaryCode.FixExt2 ||
                code == binaryCode.FixExt4 ||
                code == binaryCode.FixExt8 ||
                code == binaryCode.FixExt16 ||
                code == binaryCode.Ext8 ||
                code == binaryCode.Ext16 ||
                code == binaryCode.Ext32)
            {
                return TryExtensionHeader(ref reader);
            }

            throw new FileNotFoundException(string.Format("没有找到这个二进制所代表的意思：{0}", code));
        }

        private bool TrySkip(ref ExtenderBinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!TrySkip(ref reader))
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
        /// <param name="reader">ExtenderBinaryReader对象，通过ref传递以修改其状态</param>
        /// <returns>如果成功跳过数组，则返回true；否则返回false</returns>
        private bool TrySkipNextArray(ref ExtenderBinaryReader reader)
            => TryArrayHeader(ref reader, out int count) && TrySkip(ref reader, count);

        /// <summary>
        /// 尝试读取数组头部信息
        /// </summary>
        /// <param name="reader">ExtenderBinaryReader对象，通过ref传递以读取其数据</param>
        /// <param name="count">输出参数，存储数组元素的数量</param>
        /// <returns>如果成功读取数组头部，则返回true；否则返回false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryArrayHeader(ref ExtenderBinaryReader reader, out int count)
        => TryReadArrayHeader(ref reader, out count);

        /// <summary>
        /// 读取字节序列
        /// </summary>
        /// <param name="reader">ExtenderBinaryReader对象，通过ref传递以读取其数据</param>
        /// <returns>如果读取成功，则返回包含读取字节的ReadOnlySequence<byte>对象；如果读取到nil，则返回null</returns>
        public ReadOnlySequence<byte> ReadBytes(ref ExtenderBinaryReader reader)
        {
            if (TryReadNil(ref reader))
            {
                return ReadOnlySequence<byte>.Empty;
            }

            TryBytesLength(ref reader, out uint length);
            ThrowInsufficientBufferUnless(reader.Remaining >= length);
            ReadOnlySequence<byte> result = reader.Sequence.Slice(reader.Position, length);
            reader.Advance(length);
            return result;
        }

        #endregion

        #region Map

        private bool TrySkipNextMap(ref ExtenderBinaryReader reader)
            => TryMapHeader(ref reader, out int count) && TrySkip(ref reader, count * 2);

        public bool TryMapHeader(ref ExtenderBinaryReader reader, out int count)
        {
            //DecodeResult readResult = TryReadMapHeader(reader.UnreadSpan, out uint uintCount, out int tokenSize);
            //count = checked((int)uintCount);
            //if (readResult == DecodeResult.Success)
            //{
            //    reader.Advance(tokenSize);
            //    return true;
            //}

            //return SlowPath(ref this, readResult, ref count, ref tokenSize);

            //static bool SlowPath(ref MessagePackReader self, DecodeResult readResult, ref int count, ref int tokenSize)
            //{
            //    switch (readResult)
            //    {
            //        case DecodeResult.Success:
            //            reader.Advance(tokenSize);
            //            return true;
            //        case DecodeResult.TokenMismatch:
            //            throw ThrowInvalidCode(reader.UnreadSpan[0]);
            //        case DecodeResult.EmptyBuffer:
            //        case DecodeResult.InsufficientBuffer:
            //            Span<byte> buffer = stackalloc byte[tokenSize];
            //            if (reader.TryCopyTo(buffer))
            //            {
            //                readResult = TryReadMapHeader(buffer, out uint uintCount, out tokenSize);
            //                count = checked((int)uintCount);
            //                return SlowPath(ref self, readResult, ref count, ref tokenSize);
            //            }
            //            else
            //            {
            //                count = 0;
            //                return false;
            //            }

            //        default:
            //            throw ThrowUnreachable();
            //    }
            //}

            count = 0;
            return true;
        }

        #endregion

        #region String

        private bool TryStringLength(ref ExtenderBinaryReader reader)
            => TryStringLength(ref reader, out uint length) && reader.TryAdvance(length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryStringLength(ref ExtenderBinaryReader reader, out uint length)
        {
            //DecodeResult readResult = TryReadStringHeader(reader.UnreadSpan, out length, out int tokenSize);
            //if (readResult == DecodeResult.Success)
            //{
            //    reader.Advance(tokenSize);
            //    return true;
            //}

            //return SlowPath(ref this, readResult, ref length, ref tokenSize);

            //static bool SlowPath(ref MessagePackReader self, DecodeResult readResult, ref uint length, ref int tokenSize)
            //{
            //    switch (readResult)
            //    {
            //        case DecodeResult.Success:
            //            reader.Advance(tokenSize);
            //            return true;
            //        case DecodeResult.TokenMismatch:
            //            throw ThrowInvalidCode(reader.UnreadSpan[0]);
            //        case DecodeResult.EmptyBuffer:
            //        case DecodeResult.InsufficientBuffer:
            //            Span<byte> buffer = stackalloc byte[tokenSize];
            //            if (reader.TryCopyTo(buffer))
            //            {
            //                readResult = TryReadStringHeader(buffer, out length, out tokenSize);
            //                return SlowPath(ref self, readResult, ref length, ref tokenSize);
            //            }
            //            else
            //            {
            //                length = default;
            //                return false;
            //            }

            //        default:
            //            throw ThrowUnreachable();
            //    }
            //}
            length = 0;
            return true;
        }

        #endregion

        #region Bin

        private bool TryBytesLength(ref ExtenderBinaryReader reader)
            => TryBytesLength(ref reader, out uint length) && reader.TryAdvance(length);

        private bool TryBytesLength(ref ExtenderBinaryReader reader, out uint length)
        {
            bool usingBinaryHeader = true;
            DecodeResult readResult = _binaryConvert.TryReadBinHeader(reader.UnreadSpan, out length, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return true;
            }

            return SlowPath(ref reader, readResult, usingBinaryHeader, ref length, ref tokenSize);

            bool SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, bool usingBinaryHeader, ref uint length, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        if (usingBinaryHeader)
                        {
                            usingBinaryHeader = false;
                            readResult = _binaryConvert.TryReadStringHeader(reader.UnreadSpan, out length, out tokenSize);
                            return SlowPath(ref reader, readResult, usingBinaryHeader, ref length, ref tokenSize);
                        }
                        else
                        {
                            throw ThrowInvalidCode(reader.UnreadSpan[0]);
                        }

                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = usingBinaryHeader
                                ? _binaryConvert.TryReadBinHeader(buffer, out length, out tokenSize)
                                : _binaryConvert.TryReadStringHeader(buffer, out length, out tokenSize);
                            return SlowPath(ref reader, readResult, usingBinaryHeader, ref length, ref tokenSize);
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
            length = 0;
            return true;
        }

        #endregion

        #region ExtensionHeader

        /// <summary>
        /// 尝试读取扩展头，并尝试前进读取器到扩展头的长度位置
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <returns>如果成功读取扩展头并成功前进读取器到扩展头的长度位置，则返回true；否则返回false</returns>
        private bool TryExtensionHeader(ref ExtenderBinaryReader reader)
                    => TryExtensionHeader(ref reader, out ExtensionHeader header) && reader.TryAdvance(header.Length);

        /// <summary>
        /// 尝试从给定的二进制读取器中读取扩展头。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <param name="extensionHeader">读取的扩展头。</param>
        /// <returns>如果成功读取扩展头，则返回true；否则返回false。</returns>
        /// <remarks>
        /// 该方法首先尝试使用快速路径读取扩展头。如果快速路径失败，则调用慢速路径。
        /// 快速路径和慢速路径都会处理不同的解码结果，并根据结果执行相应的操作。
        /// </remarks>
        public bool TryExtensionHeader(ref ExtenderBinaryReader reader, out ExtensionHeader extensionHeader)
        {
            DecodeResult readResult = _binaryConvert.TryReadExtensionHeader(reader.UnreadSpan, out extensionHeader, out int tokenSize);

            return SlowPath(ref reader, readResult, ref extensionHeader, ref tokenSize);

            bool SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, ref ExtensionHeader extensionHeader, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        //throw ThrowInvalidCode(reader.UnreadSpan[0]);
                        return false;
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadExtensionHeader(buffer, out extensionHeader, out tokenSize);
                            return SlowPath(ref reader, readResult, ref extensionHeader, ref tokenSize);
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
            extensionHeader = default;
            return true;
        }

        #endregion

        #region Read

        /// <summary>
        /// 读取Nil值。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <returns>如果读取到Nil值，则返回true；否则返回false。</returns>
        public bool ReadNil(ref ExtenderBinaryReader reader)
        {
            ThrowInsufficientBufferUnless(reader.TryRead(out var code));

            return code == binaryCode.Nil;
        }

        /// <summary>
        /// 尝试读取Nil值。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <returns>如果成功读取到Nil值，则返回true；否则返回false。</returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadNil(ref ExtenderBinaryReader reader)
        {
            if (reader.NextCode == binaryCode.Nil)
            {
                ReadNil(ref reader);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 读取指定长度的原始数据。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <param name="length">要读取的长度。</param>
        /// <returns>读取到的原始数据。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取的数据不足指定长度时抛出。</exception>
        public ReadOnlySequence<byte> ReadRaw(ref ExtenderBinaryReader reader, long length)
        {
            try
            {
                ReadOnlySequence<byte> result = reader.Sequence.Slice(reader.Position, length);
                reader.TryAdvance(length);
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
        /// <param name="reader">二进制读取器。</param>
        /// <returns>读取到的原始数据。</returns>
        public ReadOnlySequence<byte> ReadRaw(ref ExtenderBinaryReader reader)
        {
            SequencePosition initialPosition = reader.Position;
            Skip(ref reader);
            return reader.Sequence.Slice(initialPosition, reader.Position);
        }

        /// <summary>
        /// 读取数组头部信息。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <returns>数组的长度。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当剩余数据不足以表示数组长度或数组元素时抛出。</exception>
        public int ReadArrayHeader(ref ExtenderBinaryReader reader)
        {
            ThrowInsufficientBufferUnless(TryReadArrayHeader(ref reader, out int count));

            //防止损坏或恶意的数据，这些数据可能会导致分配过多的内存。
            //我们允许每个基元的大小最小为1个字节。
            //知道每个元素较大的格式化程序可以选择添加更强的检查。
            ThrowInsufficientBufferUnless(reader.Remaining >= count);

            return count;
        }

        /// <summary>
        /// 尝试读取数组头部信息。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <param name="count">数组的长度。</param>
        /// <returns>如果成功读取到数组头部信息，则返回true；否则返回false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadArrayHeader(ref ExtenderBinaryReader reader, out int count)
        {
            DecodeResult readResult = _binaryConvert.TryReadArrayHeader(reader.UnreadSpan, out uint uintCount, out int tokenSize);
            count = checked((int)uintCount);
            if (readResult == DecodeResult.Success)
            {
                reader.TryAdvance(tokenSize);
                return true;
            }

            return SlowPath(ref reader, readResult, ref count, ref tokenSize);

            bool SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, ref int count, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadArrayHeader(buffer, out uint uintCount, out tokenSize);
                            count = checked((int)uintCount);
                            return SlowPath(ref reader, readResult, ref count, ref tokenSize);
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
        /// <param name="reader">二进制读取器。</param>
        /// <returns>映射中的键值对数量。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当剩余数据不足以表示映射长度或映射元素时抛出。</exception>
        public int ReadMapHeader(ref ExtenderBinaryReader reader)
        {
            ThrowInsufficientBufferUnless(TryReadMapHeader(ref reader, out int count));

            // 防止因损坏或恶意数据导致分配过多内存。
            // 我们允许每个基本数据类型至少占用1个字节的大小，并且我们有一个key=value映射，因此总共是2个字节。
            // 知道每个元素更大的格式化程序可以选择添加更强的检查。
            ThrowInsufficientBufferUnless(reader.Remaining >= count * 2);

            return count;
        }

        /// <summary>
        /// 尝试读取映射头部信息。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <param name="count">映射中的键值对数量。</param>
        /// <returns>如果成功读取到映射头部信息，则返回true；否则返回false。</returns>
        public bool TryReadMapHeader(ref ExtenderBinaryReader reader, out int count)
        {
            DecodeResult readResult = _binaryConvert.TryReadMapHeader(reader.UnreadSpan, out uint uintCount, out int tokenSize);
            count = checked((int)uintCount);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return true;
            }

            return SlowPath(ref reader, readResult, ref count, ref tokenSize);

            bool SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, ref int count, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return true;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadMapHeader(buffer, out uint uintCount, out tokenSize);
                            count = checked((int)uintCount);
                            return SlowPath(ref reader, readResult, ref count, ref tokenSize);
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
        /// <param name="reader">二进制读取器。</param>
        /// <returns>读取到的布尔值。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的代码不是True或False时抛出。</exception>
        public bool ReadBoolean(ref ExtenderBinaryReader reader)
        {
            ThrowInsufficientBufferUnless(reader.TryRead(out byte code));

            if (code == binaryCode.True)
            {
                return true;
            }
            else if (code == binaryCode.False)
            {
                return false;
            }

            throw ThrowInvalidCode(code);
        }

        /// <summary>
        /// 读取字符。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <returns>读取到的字符。</returns>
        public char ReadChar(ref ExtenderBinaryReader reader) => (char)ReadUInt16(ref reader);

        /// <summary>
        /// 读取单精度浮点数。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <returns>读取到的单精度浮点数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示单精度浮点数时抛出。</exception>
        public unsafe float ReadSingle(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadSingle(reader.UnreadSpan, out float value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            float SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, float value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadSingle(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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
        /// <param name="reader">二进制读取器。</param>
        /// <returns>读取到的双精度浮点数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示双精度浮点数时抛出。</exception>
        public unsafe double ReadDouble(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadDouble(reader.UnreadSpan, out double value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            double SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, double value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadDouble(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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
        /// <param name="reader">二进制读取器。</param>
        /// <returns>读取到的日期时间。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示日期时间时抛出。</exception>
        public DateTime ReadDateTime(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadDateTime(reader.UnreadSpan, out DateTime value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            DateTime SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, DateTime value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadDateTime(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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
        /// <param name="reader">二进制读取器。</param>
        /// <param name="header">扩展头。</param>
        /// <returns>读取到的日期时间。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当读取到的数据不足以表示日期时间时抛出。</exception>
        public DateTime ReadDateTime(ref ExtenderBinaryReader reader, ExtensionHeader header)
        {
            DecodeResult readResult = _binaryConvert.TryReadDateTime(reader.UnreadSpan, header, out DateTime value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, header, readResult, value, ref tokenSize);

            DateTime SlowPath(ref ExtenderBinaryReader reader, ExtensionHeader header, DecodeResult readResult, DateTime value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadDateTime(buffer, header, out value, out tokenSize);
                            return SlowPath(ref reader, header, readResult, value, ref tokenSize);
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

        public Byte ReadByte(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadByte(reader.UnreadSpan, out Byte value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            Byte SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, Byte value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadByte(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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

        public UInt16 ReadUInt16(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadUInt16(reader.UnreadSpan, out UInt16 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            UInt16 SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, UInt16 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadUInt16(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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

        public UInt32 ReadUInt32(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadUInt32(reader.UnreadSpan, out UInt32 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            UInt32 SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, UInt32 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadUInt32(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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

        public UInt64 ReadUInt64(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadUInt64(reader.UnreadSpan, out UInt64 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            UInt64 SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, UInt64 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadUInt64(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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

        public SByte ReadSByte(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadSByte(reader.UnreadSpan, out SByte value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            SByte SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, SByte value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadSByte(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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

        public Int16 ReadInt16(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadInt16(reader.UnreadSpan, out Int16 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            Int16 SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, Int16 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadInt16(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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

        public Int32 ReadInt32(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadInt32(reader.UnreadSpan, out Int32 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            Int32 SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, Int32 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadInt32(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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

        public Int64 ReadInt64(ref ExtenderBinaryReader reader)
        {
            DecodeResult readResult = _binaryConvert.TryReadInt64(reader.UnreadSpan, out Int64 value, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                reader.Advance(tokenSize);
                return value;
            }

            return SlowPath(ref reader, readResult, value, ref tokenSize);

            Int64 SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, Int64 value, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        reader.Advance(tokenSize);
                        return value;
                    case DecodeResult.TokenMismatch:
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);
                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadInt64(buffer, out value, out tokenSize);
                            return SlowPath(ref reader, readResult, value, ref tokenSize);
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
        /// 尝试从<see cref="ExtenderBinaryReader"/>中读取字符串范围。
        /// </summary>
        /// <param name="reader">包含二进制数据的<see cref="ExtenderBinaryReader"/>。</param>
        /// <param name="span">读取到的字符串范围。</param>
        /// <returns>如果成功读取到字符串范围，则返回true；否则返回false。</returns>
        public bool TryReadStringSpan(ref ExtenderBinaryReader reader, out ReadOnlySpan<byte> span)
        {
            if (TryReadNil(ref reader))
            {
                span = default;
                return false;
            }

            long oldPosition = reader.Consumed;
            int length = checked((int)GetStringLengthInBytes(ref reader));
            ThrowInsufficientBufferUnless(reader.Remaining >= length);

            if (reader.CurrentSpanIndex + length <= reader.CurrentSpan.Length)
            {
                span = reader.CurrentSpan.Slice(reader.CurrentSpanIndex, length);
                reader.Advance(length);
                return true;
            }
            else
            {
                reader.Rewind(reader.Consumed - oldPosition);
                span = default;
                return false;
            }
        }

        /// <summary>
        /// 获取字符串的长度（以字节为单位）。
        /// </summary>
        /// <param name="reader">ExtenderBinaryReader对象，用于读取数据。</param>
        /// <returns>返回字符串的长度（以字节为单位）。</returns>
        /// <exception cref="Exception">如果缓冲区不足，则抛出异常。</exception>
        private uint GetStringLengthInBytes(ref ExtenderBinaryReader reader)
        {
            // 如果缓冲区不足，则抛出异常
            ThrowInsufficientBufferUnless(TryGetStringLengthInBytes(ref reader, out uint length));
            return length;
        }

        /// <summary>
        /// 尝试获取字符串的长度（以字节为单位）。
        /// </summary>
        /// <param name="reader">ExtenderBinaryReader对象，用于读取数据。</param>
        /// <param name="length">输出参数，存储字符串的长度（以字节为单位）。</param>
        /// <returns>如果成功获取长度，则返回true；否则返回false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetStringLengthInBytes(ref ExtenderBinaryReader reader, out uint length)
        {
            // 尝试读取字符串头部并获取长度
            DecodeResult readResult = _binaryConvert.TryReadStringHeader(reader.UnreadSpan, out length, out int tokenSize);
            if (readResult == DecodeResult.Success)
            {
                // 成功读取，移动读取器指针
                reader.Advance(tokenSize);
                return true;
            }

            // 调用慢路径处理
            return SlowPath(ref reader, readResult, ref length, ref tokenSize);

            bool SlowPath(ref ExtenderBinaryReader reader, DecodeResult readResult, ref uint length, ref int tokenSize)
            {
                switch (readResult)
                {
                    case DecodeResult.Success:
                        // 成功读取，移动读取器指针
                        reader.Advance(tokenSize);
                        return true;

                    case DecodeResult.TokenMismatch:
                        // 标记不匹配，抛出异常
                        throw ThrowInvalidCode(reader.UnreadSpan[0]);

                    case DecodeResult.EmptyBuffer:
                    case DecodeResult.InsufficientBuffer:
                        // 缓冲区不足，尝试复制缓冲区并重新读取
                        Span<byte> buffer = stackalloc byte[tokenSize];
                        if (reader.TryCopyTo(buffer))
                        {
                            readResult = _binaryConvert.TryReadStringHeader(buffer, out length, out tokenSize);
                            return SlowPath(ref reader, readResult, ref length, ref tokenSize);
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
