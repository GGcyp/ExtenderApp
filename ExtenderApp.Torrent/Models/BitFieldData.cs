using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 高效表示和操作 BitField 的类，用于跟踪可用的文件分片
    /// </summary>
    public class BitFieldData : IEnumerable<bool>
    {
        // 每个 ulong 存储 64 位
        private const int BitsPerULong = 64;
        private const int BitsPerByte = 8;
        private const int ShiftPerByte = 3; // 2^3 = 8

        // 底层存储
        private readonly ulong[] _data;
        private int _trueCount;

        /// <summary>
        /// BitField 的总位数（分片数）
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 已设置为 true 的位的数量
        /// </summary>
        public int TrueCount => _trueCount;

        /// <summary>
        /// 是否所有位都为 false
        /// </summary>
        public bool AllFalse => _trueCount == 0;

        /// <summary>
        /// 是否所有位都为 true
        /// </summary>
        public bool AllTrue => _trueCount == Length;

        /// <summary>
        /// BitField 所需的字节数
        /// </summary>
        public int LengthInBytes => (Length + BitsPerByte - 1) / BitsPerByte;

        /// <summary>
        /// 已完成的百分比
        /// </summary>
        public double PercentComplete => (double)_trueCount / Length * 100.0;

        /// <summary>
        /// 以 Span 形式访问内部数据
        /// </summary>
        public ReadOnlySpan<ulong> DataSpan => _data.AsSpan();

        /// <summary>
        /// 从字节数组创建 BitField
        /// </summary>
        public BitFieldData(ReadOnlySpan<byte> bytes, int length)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be positive", nameof(length));

            if (bytes.Length < LengthInBytes)
                throw new ArgumentException("Byte array is too small", nameof(bytes));

            Length = length;
            _data = new ulong[(length + BitsPerULong - 1) / BitsPerULong];

            // 从字节数组加载数据
            int byteIndex = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                ulong value = 0;
                int bitsToRead = Math.Min(BitsPerULong, length - i * BitsPerULong);
                int bytesToRead = (bitsToRead + BitsPerByte - 1) / BitsPerByte;

                for (int j = 0; j < bytesToRead; j++)
                {
                    value = (value << BitsPerByte) | bytes[byteIndex++];
                }

                // 处理最后一个 ulong 可能不足 64 位的情况
                if (bitsToRead < BitsPerULong)
                {
                    value <<= BitsPerULong - bitsToRead;
                }

                _data[i] = value;
                _trueCount += BitOperations.PopCount(value);
            }
        }

        /// <summary>
        /// 创建指定长度的 BitField，初始值全为 false
        /// </summary>
        public BitFieldData(int length)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be positive", nameof(length));

            Length = length;
            _data = new ulong[(length + BitsPerULong - 1) / BitsPerULong];
            _trueCount = 0;
        }

        /// <summary>
        /// 从现有 BitField 复制
        /// </summary>
        public BitFieldData(BitFieldData other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            Length = other.Length;
            _data = other._data.Clone() as ulong[];
            _trueCount = other._trueCount;
        }

        /// <summary>
        /// 从布尔数组创建 BitField
        /// </summary>
        public BitFieldData(bool[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Length = array.Length;
            _data = new ulong[(Length + BitsPerULong - 1) / BitsPerULong];

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i])
                {
                    Set(i);
                }
            }
        }

        /// <summary>
        /// 索引器：获取或设置指定位置的位
        /// </summary>
        public bool this[int index]
        {
            get
            {
                ValidateIndex(index);
                return Get(index);
            }
            set
            {
                ValidateIndex(index);

                if (value)
                    Set(index);
                else
                    Clear(index);
            }
        }

        /// <summary>
        /// 设置指定位置的位为 true
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index)
        {
            int arrayIndex = index / BitsPerULong;
            int bitIndex = index % BitsPerULong;
            ulong mask = 1UL << (BitsPerULong - 1 - bitIndex);

            if ((_data[arrayIndex] & mask) == 0)
            {
                _data[arrayIndex] |= mask;
                _trueCount++;
            }
        }

        /// <summary>
        /// 设置指定位置的位为 false
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(int index)
        {
            int arrayIndex = index / BitsPerULong;
            int bitIndex = index % BitsPerULong;
            ulong mask = 1UL << (BitsPerULong - 1 - bitIndex);

            if ((_data[arrayIndex] & mask) != 0)
            {
                _data[arrayIndex] &= ~mask;
                _trueCount--;
            }
        }

        /// <summary>
        /// 获取指定位置的位值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index)
        {
            int arrayIndex = index / BitsPerULong;
            int bitIndex = index % BitsPerULong;
            ulong mask = 1UL << (BitsPerULong - 1 - bitIndex);

            return (_data[arrayIndex] & mask) != 0;
        }

        /// <summary>
        /// 查找第一个值为 true 的位的索引
        /// </summary>
        public int FirstTrue() => FirstTrue(0, Length - 1);

        /// <summary>
        /// 在指定范围内查找第一个值为 true 的位的索引
        /// </summary>
        public int FirstTrue(int startIndex, int endIndex)
        {
            ValidateRange(startIndex, endIndex);

            if (AllFalse)
                return -1;

            int startArrayIndex = startIndex / BitsPerULong;
            int endArrayIndex = endIndex / BitsPerULong;

            for (int i = startArrayIndex; i <= endArrayIndex; i++)
            {
                if (_data[i] == 0)
                    continue;

                // 计算在当前 ulong 中的起始和结束位
                int startBit = i == startArrayIndex ? startIndex % BitsPerULong : 0;
                int endBit = i == endArrayIndex ? endIndex % BitsPerULong : BitsPerULong - 1;

                // 处理起始位偏移
                ulong masked = _data[i] & (ulong.MaxValue << (BitsPerULong - 1 - endBit));
                masked &= ulong.MaxValue >> startBit;

                if (masked != 0)
                {
                    int trailingZeros = BitOperations.TrailingZeroCount(masked);
                    return i * BitsPerULong + (BitsPerULong - 1 - trailingZeros);
                }
            }

            return -1;
        }

        /// <summary>
        /// 查找第一个值为 false 的位的索引
        /// </summary>
        public int FirstFalse() => FirstFalse(0, Length - 1);

        /// <summary>
        /// 在指定范围内查找第一个值为 false 的位的索引
        /// </summary>
        public int FirstFalse(int startIndex, int endIndex)
        {
            ValidateRange(startIndex, endIndex);

            if (AllTrue)
                return -1;

            int startArrayIndex = startIndex / BitsPerULong;
            int endArrayIndex = endIndex / BitsPerULong;

            for (int i = startArrayIndex; i <= endArrayIndex; i++)
            {
                ulong inverted = ~_data[i];
                if (inverted == 0)
                    continue;

                // 计算在当前 ulong 中的起始和结束位
                int startBit = i == startArrayIndex ? startIndex % BitsPerULong : 0;
                int endBit = i == endArrayIndex ? endIndex % BitsPerULong : BitsPerULong - 1;

                // 处理起始位偏移
                ulong masked = inverted & (ulong.MaxValue << (BitsPerULong - 1 - endBit));
                masked &= ulong.MaxValue >> startBit;

                if (masked != 0)
                {
                    int trailingZeros = BitOperations.TrailingZeroCount(masked);
                    return i * BitsPerULong + (BitsPerULong - 1 - trailingZeros);
                }
            }

            return -1;
        }

        /// <summary>
        /// 将 BitField 转换为字节数组
        /// </summary>
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[LengthInBytes];
            ToBytes(bytes.AsSpan());
            return bytes;
        }

        /// <summary>
        /// 将 BitField 写入到给定的字节 Span
        /// </summary>
        public void ToBytes(Span<byte> destination)
        {
            if (destination.Length < LengthInBytes)
                throw new ArgumentException("Destination span is too small", nameof(destination));

            for (int i = 0; i < _data.Length; i++)
            {
                ulong value = _data[i];
                int bitsRemaining = Math.Min(BitsPerULong, Length - i * BitsPerULong);
                int bytesToWrite = (bitsRemaining + BitsPerByte - 1) / BitsPerByte;

                for (int j = bytesToWrite - 1; j >= 0; j--)
                {
                    destination[i * 8 + j] = (byte)(value & 0xFF);
                    value >>= BitsPerByte;
                }
            }
        }

        /// <summary>
        /// 对所有位执行逻辑非操作
        /// </summary>
        public void Not()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = ~_data[i];
            }

            // 处理最后一个 ulong 中可能的无效位
            ZeroUnusedBits();

            // 更新 TrueCount
            _trueCount = 0;
            foreach (ulong value in _data)
            {
                _trueCount += BitOperations.PopCount(value);
            }
        }

        /// <summary>
        /// 对两个 BitField 执行逻辑与操作
        /// </summary>
        public void And(BitFieldData other)
        {
            ValidateSameLength(other);

            _trueCount = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] &= other._data[i];
                _trueCount += BitOperations.PopCount(_data[i]);
            }
        }

        /// <summary>
        /// 对两个 BitField 执行逻辑或操作
        /// </summary>
        public void Or(BitFieldData other)
        {
            ValidateSameLength(other);

            _trueCount = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] |= other._data[i];
                _trueCount += BitOperations.PopCount(_data[i]);
            }
        }

        /// <summary>
        /// 对两个 BitField 执行逻辑异或操作
        /// </summary>
        public void Xor(BitFieldData other)
        {
            ValidateSameLength(other);

            _trueCount = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] ^= other._data[i];
                _trueCount += BitOperations.PopCount(_data[i]);
            }
        }

        /// <summary>
        /// 将所有位设置为 true
        /// </summary>
        public void SetAll()
        {
            Array.Fill(_data, ulong.MaxValue);
            ZeroUnusedBits();
            _trueCount = Length;
        }

        /// <summary>
        /// 将所有位设置为 false
        /// </summary>
        public void ClearAll()
        {
            Array.Clear(_data, 0, _data.Length);
            _trueCount = 0;
        }

        /// <summary>
        /// 计算与另一个 BitField 相同的位的数量
        /// </summary>
        public int CountSameBits(BitFieldData other)
        {
            ValidateSameLength(other);

            int count = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                count += BitOperations.PopCount(_data[i] & other._data[i]);
            }

            return count;
        }

        /// <summary>
        /// 比较两个 BitField 是否相等
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is not BitFieldData other)
                return false;

            if (Length != other.Length)
                return false;

            for (int i = 0; i < _data.Length; i++)
            {
                if (_data[i] != other._data[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            hashCode.Add(Length);

            foreach (ulong value in _data)
            {
                hashCode.Add(value);
            }

            return hashCode.ToHashCode();
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        public IEnumerator<bool> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return Get(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// 将 BitField 转换为字符串表示形式
        /// </summary>
        public override string ToString()
        {
            return string.Concat(this.Select(b => b ? '1' : '0'));
        }

        /// <summary>
        /// 验证索引是否有效
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException($"Index {index} is out of range (0-{Length - 1})");
        }

        /// <summary>
        /// 验证范围是否有效
        /// </summary>
        private void ValidateRange(int startIndex, int endIndex)
        {
            if (startIndex < 0 || startIndex >= Length)
                throw new IndexOutOfRangeException($"Start index {startIndex} is out of range (0-{Length - 1})");

            if (endIndex < 0 || endIndex >= Length)
                throw new IndexOutOfRangeException($"End index {endIndex} is out of range (0-{Length - 1})");

            if (startIndex > endIndex)
                throw new ArgumentException("Start index must be less than or equal to end index");
        }

        /// <summary>
        /// 验证两个 BitField 长度是否相同
        /// </summary>
        private void ValidateSameLength(BitFieldData other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (Length != other.Length)
                throw new ArgumentException("BitFields must have the same length");
        }

        /// <summary>
        /// 将最后一个 ulong 中超出实际长度的位清零
        /// </summary>
        private void ZeroUnusedBits()
        {
            if (Length % BitsPerULong == 0)
                return;

            int lastArrayIndex = _data.Length - 1;
            int unusedBits = BitsPerULong - (Length % BitsPerULong);
            ulong mask = ulong.MaxValue >> unusedBits;

            _data[lastArrayIndex] &= mask;
        }
    }
}
