using System.Diagnostics.CodeAnalysis;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary
{
    /// <summary>
    /// 表示一个二进制原始类。
    /// </summary>
    public class BinaryConvert
    {
        /// <summary>
        /// 获取或设置二进制选项。
        /// </summary>
        public BinaryOptions BinaryOptions { get; set; }

        public BinaryConvert() : this(new BinaryOptions())
        {

        }

        public BinaryConvert(BinaryOptions options)
        {
            BinaryOptions = options;
        }

        public bool TryWriteNil(Span<byte> destination, out int bytesWritten)
        {
            bytesWritten = 1;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.Nil;
            return true;
        }

        public bool TryWriteArrayHeader(Span<byte> destination, uint count, out int bytesWritten)
        {
            if (count <= BinaryOptions.BinaryRang.MaxFixArrayCount)
            {
                bytesWritten = 1;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = (byte)(BinaryOptions.BinaryCode.MinFixArray | count);
                return true;
            }
            else if (count <= ushort.MaxValue)
            {
                bytesWritten = 3;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = BinaryOptions.BinaryCode.Array16;
                WriteBigEndian(destination.Slice(1), (ushort)count);
                return true;
            }

            bytesWritten = 5;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.Array32;
            WriteBigEndian(destination.Slice(1), count);
            return true;
        }

        public bool TryWriteMapHeader(Span<byte> destination, uint count, out int bytesWritten)
        {
            if (count <= BinaryOptions.BinaryRang.MaxFixMapCount)
            {
                bytesWritten = 1;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = (byte)(BinaryOptions.BinaryCode.MinFixMap | count);
                return true;
            }
            else if (count <= ushort.MaxValue)
            {
                bytesWritten = 3;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = BinaryOptions.BinaryCode.Map16;
                WriteBigEndian(destination.Slice(1), (ushort)count);
                return true;
            }

            bytesWritten = 5;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.Map32;
            WriteBigEndian(destination.Slice(1), count);
            return true;
        }

        private bool TryWriteFixIntUnsafe(Span<byte> destination, byte value, out int bytesWritten)
        {
            bytesWritten = 1;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = unchecked(value);
            return true;
        }

        public bool TryWriteBinHeader(Span<byte> destination, uint length, out int bytesWritten)
        {
            switch (length)
            {
                case <= byte.MaxValue:
                    bytesWritten = 2;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryOptions.BinaryCode.Bin8;
                    destination[1] = (byte)length;
                    return true;
                case <= UInt16.MaxValue:
                    bytesWritten = 3;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryOptions.BinaryCode.Bin16;
                    WriteBigEndian(destination.Slice(1), (ushort)length);
                    return true;
                default:
                    bytesWritten = 5;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryOptions.BinaryCode.Bin32;
                    WriteBigEndian(destination.Slice(1), length);
                    return true;
            }
        }

        public bool TryWriteStringHeader(Span<byte> destination, uint byteCount, out int bytesWritten)
        {
            if (byteCount <= BinaryOptions.BinaryRang.MaxFixStringLength)
            {
                bytesWritten = 1;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = (byte)(BinaryOptions.BinaryCode.MinFixStr | byteCount);
                return true;
            }
            else if (byteCount <= byte.MaxValue)
            {
                bytesWritten = 2;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = BinaryOptions.BinaryCode.Str8;
                destination[1] = unchecked((byte)byteCount);
                return true;
            }
            else if (byteCount <= ushort.MaxValue)
            {
                bytesWritten = 3;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = BinaryOptions.BinaryCode.Str16;
                WriteBigEndian(destination.Slice(1), (ushort)byteCount);
                return true;
            }

            bytesWritten = 5;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.Str32;
            WriteBigEndian(destination.Slice(1), byteCount);
            return true;
        }

        public bool TryWriteExtensionFormatHeader(Span<byte> destination, ExtensionHeader extensionHeader, out int bytesWritten)
        {
            int dataLength = (int)extensionHeader.Length;
            byte typeCode = unchecked((byte)extensionHeader.TypeCode);
            switch (dataLength)
            {
                case 1 or 2 or 4 or 8 or 16:
                    bytesWritten = 2;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = dataLength switch
                    {
                        1 => BinaryOptions.BinaryCode.FixExt1,
                        2 => BinaryOptions.BinaryCode.FixExt2,
                        4 => BinaryOptions.BinaryCode.FixExt4,
                        8 => BinaryOptions.BinaryCode.FixExt8,
                        16 => BinaryOptions.BinaryCode.FixExt16,
                        _ => throw ThrowUnreachable(),
                    };
                    destination[1] = unchecked(typeCode);
                    return true;
                case <= byte.MaxValue:
                    bytesWritten = 3;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryOptions.BinaryCode.Ext8;
                    destination[1] = unchecked((byte)dataLength);
                    destination[2] = unchecked(typeCode);
                    return true;
                case <= ushort.MaxValue:
                    bytesWritten = 4;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryOptions.BinaryCode.Ext16;
                    WriteBigEndian(destination.Slice(1), (ushort)dataLength);
                    destination[3] = unchecked(typeCode);
                    return true;
                default:
                    bytesWritten = 6;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryOptions.BinaryCode.Ext32;
                    WriteBigEndian(destination.Slice(1), dataLength);
                    destination[5] = unchecked(typeCode);
                    return true;
            }
        }

        private bool TryWriteNegativeFixIntUnsafe(Span<byte> destination, sbyte value, out int bytesWritten)
        {
            bytesWritten = 1;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = unchecked((byte)value);
            return true;
        }

        #region TryWrite

        public bool TryWrite(Span<byte> destination, sbyte value, out int bytesWritten)
        {
            if (value >= 0)
            {
                return TryWrite(destination, unchecked((byte)value), out bytesWritten);
            }

            return value >= BinaryOptions.BinaryRang.MinFixNegativeInt ?
                TryWriteNegativeFixIntUnsafe(destination, value, out bytesWritten) :
                TryWriteInt8(destination, value, out bytesWritten);
        }

        public bool TryWrite(Span<byte> destination, byte value, out int bytesWritten)
        {
            if (value < BinaryOptions.BinaryRang.MaxFixPositiveInt)
            {
                return TryWriteFixIntUnsafe(destination, unchecked((byte)value), out bytesWritten);
            }
            return TryWriteUInt8(destination, value, out bytesWritten);
        }

        public bool TryWrite(Span<byte> destination, short value, out int bytesWritten)
        {
            if (value >= 0)
            {
                return TryWrite(destination, unchecked((ushort)value), out bytesWritten);
            }
            else if (value >= BinaryOptions.BinaryRang.MinFixNegativeInt)
            {
                return TryWriteNegativeFixIntUnsafe(destination, unchecked((sbyte)value), out bytesWritten);
            }
            else if (value >= sbyte.MinValue)
            {
                return TryWriteInt8(destination, unchecked((sbyte)value), out bytesWritten);
            }
            return TryWriteInt16(destination, value, out bytesWritten);
        }

        public bool TryWrite(Span<byte> destination, int value, out int bytesWritten)
        {
            if (value >= 0)
            {
                return TryWrite(destination, unchecked((uint)value), out bytesWritten);
            }
            else if (value >= BinaryOptions.BinaryRang.MinFixNegativeInt)
            {
                return TryWriteNegativeFixIntUnsafe(destination, unchecked((sbyte)value), out bytesWritten);
            }
            else if (value >= sbyte.MinValue)
            {
                return TryWriteInt8(destination, unchecked((sbyte)value), out bytesWritten);
            }
            else if (value >= short.MinValue)
            {
                return TryWriteInt16(destination, unchecked((short)value), out bytesWritten);
            }
            return TryWriteInt32(destination, value, out bytesWritten);
        }

        public bool TryWrite(Span<byte> destination, long value, out int bytesWritten)
        {
            if (value >= 0)
            {
                return TryWrite(destination, unchecked((ulong)value), out bytesWritten);
            }

            if (value >= BinaryOptions.BinaryRang.MinFixNegativeInt)
            {
                return TryWriteNegativeFixIntUnsafe(destination, unchecked((sbyte)value), out bytesWritten);
            }

            return SlowPath(destination, value, out bytesWritten);
            bool SlowPath(Span<byte> destination, long value, out int bytesWritten)
            {
                switch (value)
                {
                    case >= 0: return TryWrite(destination, (ulong)value, out bytesWritten);
                    case >= sbyte.MinValue: return TryWriteInt8(destination, (sbyte)value, out bytesWritten);
                    case >= short.MinValue: return TryWriteInt16(destination, (short)value, out bytesWritten);
                    case >= int.MinValue: return TryWriteInt32(destination, (int)value, out bytesWritten);
                    default: return TryWriteInt64(destination, value, out bytesWritten);
                }
            }
        }

        public bool TryWrite(Span<byte> destination, ushort value, out int bytesWritten)
        {

            if (value < BinaryOptions.BinaryRang.MaxFixPositiveInt)
            {
                return TryWriteFixIntUnsafe(destination, unchecked((byte)value), out bytesWritten);
            }

            switch (value)
            {
                case <= byte.MaxValue:
                    return TryWriteUInt8(destination, unchecked((byte)value), out bytesWritten);
                default:
                    return TryWriteUInt16(destination, value, out bytesWritten);
            }
        }

        public bool TryWrite(Span<byte> destination, uint value, out int bytesWritten)
        {
            if (value <= BinaryOptions.BinaryRang.MaxFixPositiveInt)
            {
                return TryWriteFixIntUnsafe(destination, unchecked((byte)value), out bytesWritten);
            }

            switch (value)
            {
                case <= byte.MaxValue:
                    return TryWriteUInt8(destination, unchecked((byte)value), out bytesWritten);
                case <= ushort.MaxValue:
                    return TryWriteUInt16(destination, unchecked((ushort)value), out bytesWritten);
                default:
                    return TryWriteUInt32(destination, value, out bytesWritten);
            }
        }

        public bool TryWrite(Span<byte> destination, ulong value, out int bytesWritten)
        {
            if (value <= (ulong)BinaryOptions.BinaryRang.MaxFixPositiveInt)
            {
                return TryWriteFixIntUnsafe(destination, unchecked((byte)value), out bytesWritten);
            }

            return SlowPath(destination, value, out bytesWritten);

            bool SlowPath(Span<byte> destination, ulong value, out int bytesWritten)
            {
                switch (value)
                {
                    case <= byte.MaxValue:
                        return TryWriteUInt8(destination, unchecked((byte)value), out bytesWritten);
                    case <= ushort.MaxValue:
                        return TryWriteUInt16(destination, unchecked((ushort)value), out bytesWritten);
                    case <= uint.MaxValue:
                        return TryWriteUInt32(destination, unchecked((uint)value), out bytesWritten);
                    default:
                        return TryWriteUInt64(destination, value, out bytesWritten);
                }
            }
        }

        public unsafe bool TryWrite(Span<byte> destination, float value, out int bytesWritten)
        {
            bytesWritten = 5;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.Float32;
            WriteBigEndian(destination.Slice(1), *(int*)&value);
            return true;
        }

        public unsafe bool TryWrite(Span<byte> destination, double value, out int bytesWritten)
        {
            bytesWritten = 9;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.Float64;
            WriteBigEndian(destination.Slice(1), *(long*)&value);
            return true;
        }

        public bool TryWrite(Span<byte> destination, bool value, out int bytesWritten)
        {
            bytesWritten = 1;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = value ? BinaryOptions.BinaryCode.True : BinaryOptions.BinaryCode.False;
            return true;
        }

        public bool TryWrite(Span<byte> destination, char value, out int bytesWritten)
            => TryWrite(destination, (ushort)value, out bytesWritten);

        public bool TryWrite(Span<byte> destination, DateTime value, out int bytesWritten)
        {
            // FixExt4(-1) => seconds |  [1970-01-01 00:00:00 UTC, 2106-02-07 06:28:16 UTC) range
            // FixExt8(-1) => nanoseconds + seconds | [1970-01-01 00:00:00.000000000 UTC, 2514-05-30 01:53:04.000000000 UTC) range
            // Ext8(12,-1) => nanoseconds + seconds | [-584554047284-02-23 16:59:44 UTC, 584554051223-11-09 07:00:16.000000000 UTC) range

            //规范需要UTC。如果我们确定值表示为本地时间，则转换为UTC。
            //如果它是未指定的，我们想不管它，因为。NET将在我们转换时更改值
            //我们根本不知道，所以我们应该保持原样。
            if (value.Kind == DateTimeKind.Local)
            {
                value = value.ToUniversalTime();
            }

            var secondsSinceBclEpoch = value.Ticks / TimeSpan.TicksPerSecond;
            var seconds = secondsSinceBclEpoch - BinaryOptions.DateTimeConstants.BclSecondsAtUnixEpoch;
            var nanoseconds = (value.Ticks % TimeSpan.TicksPerSecond) * BinaryOptions.DateTimeConstants.NanosecondsPerTick;


            if ((seconds >> 34) == 0)
            {
                var data64 = unchecked((ulong)((nanoseconds << 34) | seconds));
                if ((data64 & 0xffffffff00000000L) == 0)
                {
                    bytesWritten = 6;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    var data32 = (UInt32)data64;
                    destination[0] = BinaryOptions.BinaryCode.FixExt4;
                    destination[1] = unchecked((byte)BinaryOptions.DateTimeConstants.DateTime);
                    WriteBigEndian(destination.Slice(2), data32);
                }
                else
                {
                    bytesWritten = 10;
                    if (destination.Length < bytesWritten)
                    {
                        return false;
                    }

                    destination[0] = BinaryOptions.BinaryCode.FixExt8;
                    destination[1] = unchecked((byte)BinaryOptions.DateTimeConstants.DateTime);
                    WriteBigEndian(destination.Slice(2), data64);
                }
            }
            else
            {
                bytesWritten = 15;
                if (destination.Length < bytesWritten)
                {
                    return false;
                }

                destination[0] = BinaryOptions.BinaryCode.Ext8;
                destination[1] = 12;
                destination[2] = unchecked((byte)BinaryOptions.DateTimeConstants.DateTime);
                WriteBigEndian(destination.Slice(3), (uint)nanoseconds);
                WriteBigEndian(destination.Slice(7), seconds);
            }

            return true;
        }

        #endregion

        #region int

        public bool TryWriteInt8(Span<byte> destination, sbyte value, out int bytesWritten)
        {
            bytesWritten = 2;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.Int8;
            destination[1] = unchecked((byte)value);
            return true;
        }

        public bool TryWriteInt16(Span<byte> destination, short value, out int bytesWritten)
        {
            bytesWritten = 3;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.Int16;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        public bool TryWriteInt32(Span<byte> destination, int value, out int bytesWritten)
        {
            bytesWritten = 5;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.Int32;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        public bool TryWriteInt64(Span<byte> destination, long value, out int bytesWritten)
        {
            bytesWritten = 9;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.Int64;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        #endregion

        #region uint

        public bool TryWriteUInt8(Span<byte> destination, byte value, out int bytesWritten)
        {
            bytesWritten = 2;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.UInt8;
            destination[1] = value;
            return true;
        }

        public bool TryWriteUInt16(Span<byte> destination, ushort value, out int bytesWritten)
        {
            bytesWritten = 3;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.UInt16;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        public bool TryWriteUInt32(Span<byte> destination, uint value, out int bytesWritten)
        {
            bytesWritten = 5;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.UInt32;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        public bool TryWriteUInt64(Span<byte> destination, ulong value, out int bytesWritten)
        {
            bytesWritten = 9;
            if (destination.Length < bytesWritten)
            {
                return false;
            }

            destination[0] = BinaryOptions.BinaryCode.UInt64;
            WriteBigEndian(destination.Slice(1), value);
            return true;
        }

        #endregion

        private void WriteBigEndian(Span<byte> destination, ushort value)
        {
            unchecked
            {
                destination[1] = (byte)value;
                destination[0] = (byte)(value >> 8);
            }
        }

        private void WriteBigEndian(Span<byte> destination, uint value)
        {
            unchecked
            {
                destination[3] = (byte)value;
                destination[2] = (byte)(value >> 8);
                destination[1] = (byte)(value >> 16);
                destination[0] = (byte)(value >> 24);
            }
        }

        private void WriteBigEndian(Span<byte> destination, ulong value)
        {
            unchecked
            {
                destination[7] = (byte)value;
                destination[6] = (byte)(value >> 8);
                destination[5] = (byte)(value >> 16);
                destination[4] = (byte)(value >> 24);
                destination[3] = (byte)(value >> 32);
                destination[2] = (byte)(value >> 40);
                destination[1] = (byte)(value >> 48);
                destination[0] = (byte)(value >> 56);
            }
        }

        private void WriteBigEndian(Span<byte> destination, short value)
            => WriteBigEndian(destination, unchecked((ushort)value));

        private void WriteBigEndian(Span<byte> destination, int value)
            => WriteBigEndian(destination, unchecked((uint)value));

        private void WriteBigEndian(Span<byte> destination, long value)
            => WriteBigEndian(destination, unchecked((ulong)value));

        [DoesNotReturn]
        private Exception ThrowUnreachable() => throw new Exception("转换二进制出现问题");
    }
}
