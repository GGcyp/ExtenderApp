using ExtenderApp.Data;
using ExtenderApp.Data.File;

namespace ExtenderApp.Common.IO.Binaries
{
    public partial class BinaryConvert
    {
        /// <summary>
        /// 定义了一个接口，用于从字节序列中读取一个 64 位整数。
        /// </summary>
        public interface IReadInt64
        {
            /// <summary>
            /// 从字节序列中读取一个 64 位整数。
            /// </summary>
            /// <param name="source">包含要读取的字节序列的只读字节跨度。</param>
            /// <param name="value">读取到的 64 位整数值。</param>
            /// <param name="tokenSize">读取到的整数值所占用的字节数。</param>
            /// <returns>返回一个包含解码结果的 <see cref="DecodeResult"/> 枚举值。</returns>
            DecodeResult Read(ReadOnlySpan<byte> source, out long value, out int tokenSize);
        }

        /// <summary>
        /// 定义一个用于读取无符号64位整数的接口。
        /// </summary>
        public interface IReadUInt64
        {
            /// <summary>
            /// 从指定的字节序列中读取无符号64位整数。
            /// </summary>
            /// <param name="source">包含要读取数据的字节序列。</param>
            /// <param name="value">输出参数，读取到的无符号64位整数值。</param>
            /// <param name="tokenSize">输出参数，读取到的令牌的大小（以字节为单位）。</param>
            /// <returns>包含读取结果的 <see cref="DecodeResult"/> 对象。</returns>
            DecodeResult Read(ReadOnlySpan<byte> source, out ulong value, out int tokenSize);
        }

        private class BinaryConvertDecoders
        {
            /// <summary>
            /// 获取一个包含用于读取 Int64 类型的跳转表的数组。
            /// </summary>
            public IReadInt64[] Int64JumpTable { get; }

            /// <summary>
            /// 获取一个包含用于读取 UInt64 类型的跳转表的数组。
            /// </summary>
            public IReadUInt64[] UInt64JumpTable { get; }

            /// <summary>
            /// 初始化 Decoders 类的新实例。
            /// </summary>
            /// <param name="binaryConvert">二进制转换对象。</param>
            /// <param name="options">二进制选项。</param>
            public BinaryConvertDecoders(BinaryConvert binaryConvert, BinaryOptions options)
            {
                Int64JumpTable = new IReadInt64[256];
                InitReadInt64(binaryConvert, options.BinaryCode);

                UInt64JumpTable = new IReadUInt64[256];
                InitReadUInt64(binaryConvert, options.BinaryCode);
            }

            /// <summary>
            /// 初始化用于读取 Int64 类型的跳转表。
            /// </summary>
            /// <param name="binaryConvert">二进制转换对象。</param>
            /// <param name="binaryCode">二进制代码。</param>
            private void InitReadInt64(BinaryConvert binaryConvert, BinaryCode binaryCode)
            {
                var invalid = new ReadInt64Invalid(binaryConvert);
                Int64JumpTable.AsSpan().Fill(invalid);
                Int64JumpTable[binaryCode.UInt8] = new ReadInt64UInt8(binaryConvert);
                Int64JumpTable[binaryCode.UInt16] = new ReadInt64UInt16(binaryConvert);
                Int64JumpTable[binaryCode.UInt32] = new ReadInt64UInt32(binaryConvert);
                Int64JumpTable[binaryCode.UInt64] = new ReadInt64UInt64(binaryConvert);
                Int64JumpTable[binaryCode.Int8] = new ReadInt64Int8(binaryConvert);
                Int64JumpTable[binaryCode.Int16] = new ReadInt64Int16(binaryConvert);
                Int64JumpTable[binaryCode.Int32] = new ReadInt64Int32(binaryConvert);
                Int64JumpTable[binaryCode.Int64] = new ReadInt64Int64(binaryConvert);

                var negativeFixInt = new ReadInt64NegativeFixInt(binaryConvert);
                for (int i = binaryCode.MinNegativeFixInt; i <= binaryCode.MaxNegativeFixInt; i++)
                {
                    Int64JumpTable[i] = negativeFixInt;
                }

                var fixInt = new ReadInt64FixInt(binaryConvert);
                for (int i = binaryCode.MinFixInt; i <= binaryCode.MaxFixInt; i++)
                {
                    Int64JumpTable[i] = fixInt;
                }
            }

            /// <summary>
            /// 初始化用于读取 UInt64 类型的跳转表。
            /// </summary>
            /// <param name="binaryConvert">二进制转换对象。</param>
            /// <param name="binaryCode">二进制代码。</param>
            private void InitReadUInt64(BinaryConvert binaryConvert, BinaryCode binaryCode)
            {
                var invalid = new ReadUInt64Invalid(binaryConvert);
                UInt64JumpTable.AsSpan().Fill(invalid);
                UInt64JumpTable[binaryCode.UInt8] = new ReadUInt64UInt8(binaryConvert);
                UInt64JumpTable[binaryCode.UInt16] = new ReadUInt64UInt16(binaryConvert);
                UInt64JumpTable[binaryCode.UInt32] = new ReadUInt64UInt32(binaryConvert);
                UInt64JumpTable[binaryCode.UInt64] = new ReadUInt64UInt64(binaryConvert);
                UInt64JumpTable[binaryCode.Int8] = new ReadUInt64Int8(binaryConvert);
                UInt64JumpTable[binaryCode.Int16] = new ReadUInt64Int16(binaryConvert);
                UInt64JumpTable[binaryCode.Int32] = new ReadUInt64Int32(binaryConvert);
                UInt64JumpTable[binaryCode.Int64] = new ReadUInt64Int64(binaryConvert);

                var negativeFixInt = new ReadUInt64NegativeFixInt(binaryConvert);
                for (int i = binaryCode.MinNegativeFixInt; i <= binaryCode.MaxNegativeFixInt; i++)
                {
                    UInt64JumpTable[i] = negativeFixInt;
                }

                var fixInt = new ReadUInt64FixInt(binaryConvert);
                for (int i = binaryCode.MinFixInt; i <= binaryCode.MaxFixInt; i++)
                {
                    UInt64JumpTable[i] = fixInt;
                }
            }

            #region ReadInt64

            private abstract class ReadInt64 : IReadInt64
            {
                protected BinaryConvert BinaryConvert { get; }

                public ReadInt64(BinaryConvert binaryConvert)
                {
                    BinaryConvert = binaryConvert;
                }

                public abstract DecodeResult Read(ReadOnlySpan<byte> source, out long value, out int tokenSize);
            }

            private class ReadInt64Invalid : ReadInt64
            {
                public ReadInt64Invalid(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    value = 0;
                    tokenSize = 1;
                    return DecodeResult.TokenMismatch;
                }
            }

            private class ReadInt64FixInt : ReadInt64
            {

                public ReadInt64FixInt(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    tokenSize = 1;
                    value = source[0];
                    return DecodeResult.Success;
                }
            }

            private class ReadInt64NegativeFixInt : ReadInt64
            {
                public ReadInt64NegativeFixInt(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    tokenSize = 1;
                    value = checked((Int64)unchecked((sbyte)source[0]));
                    return DecodeResult.Success;
                }
            }

            private class ReadInt64UInt8 : ReadInt64
            {
                public ReadInt64UInt8(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    tokenSize = 2;
                    if (source.Length < tokenSize)
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = source[1];
                    return DecodeResult.Success;
                }
            }

            private class ReadInt64UInt16 : ReadInt64
            {
                public ReadInt64UInt16(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    tokenSize = 3;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out ushort ushortResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = ushortResult;
                    return DecodeResult.Success;
                }
            }

            private class ReadInt64UInt32 : ReadInt64
            {
                public ReadInt64UInt32(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    tokenSize = 5;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out uint uintResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = uintResult;
                    return DecodeResult.Success;
                }
            }

            private class ReadInt64UInt64 : ReadInt64
            {
                public ReadInt64UInt64(BinaryConvert binaryConvert) : base(binaryConvert)
                {

                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    tokenSize = 9;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out ulong ulongResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = checked((Int64)ulongResult);
                    return DecodeResult.Success;
                }
            }

            private class ReadInt64Int8 : ReadInt64
            {
                public ReadInt64Int8(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    tokenSize = 2;
                    if (source.Length < tokenSize)
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = checked((Int64)unchecked((sbyte)source[1]));
                    return DecodeResult.Success;
                }
            }

            private class ReadInt64Int16 : ReadInt64
            {
                public ReadInt64Int16(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    tokenSize = 3;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out short shortResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = checked((Int64)shortResult);
                    return DecodeResult.Success;
                }
            }

            private class ReadInt64Int32 : ReadInt64
            {
                public ReadInt64Int32(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    tokenSize = 5;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out int intResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = checked((Int64)intResult);
                    return DecodeResult.Success;
                }
            }

            private class ReadInt64Int64 : ReadInt64
            {
                public ReadInt64Int64(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out Int64 value, out int tokenSize)
                {
                    tokenSize = 9;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out long longResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = checked((Int64)longResult);
                    return DecodeResult.Success;
                }
            }

            #endregion

            #region ReadUInt64

            private abstract class ReadUInt64 : IReadUInt64
            {
                protected BinaryConvert BinaryConvert { get; }

                protected ReadUInt64(BinaryConvert binaryConvert)
                {
                    BinaryConvert = binaryConvert;
                }

                public abstract DecodeResult Read(ReadOnlySpan<byte> source, out ulong value, out int tokenSize);
            }

            private class ReadUInt64Invalid : ReadUInt64
            {
                public ReadUInt64Invalid(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    value = 0;
                    tokenSize = 1;
                    return DecodeResult.TokenMismatch;
                }
            }

            private class ReadUInt64FixInt : ReadUInt64
            {
                public ReadUInt64FixInt(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    tokenSize = 1;
                    value = source[0];
                    return DecodeResult.Success;
                }
            }

            private class ReadUInt64NegativeFixInt : ReadUInt64
            {
                public ReadUInt64NegativeFixInt(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    tokenSize = 1;
                    value = checked((UInt64)unchecked((sbyte)source[0]));
                    return DecodeResult.Success;
                }
            }

            private class ReadUInt64UInt8 : ReadUInt64
            {
                public ReadUInt64UInt8(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    tokenSize = 2;
                    if (source.Length < tokenSize)
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = source[1];
                    return DecodeResult.Success;
                }
            }

            private class ReadUInt64UInt16 : ReadUInt64
            {
                public ReadUInt64UInt16(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    tokenSize = 3;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out ushort ushortResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = ushortResult;
                    return DecodeResult.Success;
                }
            }

            private class ReadUInt64UInt32 : ReadUInt64
            {
                public ReadUInt64UInt32(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    tokenSize = 5;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out uint uintResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = uintResult;
                    return DecodeResult.Success;
                }
            }

            private class ReadUInt64UInt64 : ReadUInt64
            {
                public ReadUInt64UInt64(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    tokenSize = 9;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out ulong ulongResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = checked((UInt64)ulongResult);
                    return DecodeResult.Success;
                }
            }

            private class ReadUInt64Int8 : ReadUInt64
            {
                public ReadUInt64Int8(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    tokenSize = 2;
                    if (source.Length < tokenSize)
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = checked((UInt64)unchecked((sbyte)source[1]));
                    return DecodeResult.Success;
                }
            }

            private class ReadUInt64Int16 : ReadUInt64
            {
                public ReadUInt64Int16(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    tokenSize = 3;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out short shortResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = checked((UInt64)shortResult);
                    return DecodeResult.Success;
                }
            }

            private class ReadUInt64Int32 : ReadUInt64
            {
                public ReadUInt64Int32(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    tokenSize = 5;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out int intResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = checked((UInt64)intResult);
                    return DecodeResult.Success;
                }
            }

            private class ReadUInt64Int64 : ReadUInt64
            {
                public ReadUInt64Int64(BinaryConvert binaryConvert) : base(binaryConvert)
                {
                }

                public override DecodeResult Read(ReadOnlySpan<byte> source, out UInt64 value, out int tokenSize)
                {
                    tokenSize = 9;
                    if (!BinaryConvert.TryReadBigEndian(source.Slice(1), out long longResult))
                    {
                        value = 0;
                        return DecodeResult.InsufficientBuffer;
                    }

                    value = checked((UInt64)longResult);
                    return DecodeResult.Success;
                }
            }

            #endregion
        }
    }
}
