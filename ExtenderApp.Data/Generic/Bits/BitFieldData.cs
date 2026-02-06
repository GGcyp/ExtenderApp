using System.Buffers;
using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一组位（BitField）的轻量结构，提供高效的位级操作与统计功能。
    /// 设计为通用数据结构，可用于跟踪分片、块、槽位、标志集合或任意需要按位管理的资源。
    /// 基于 <see cref="ArrayPool{T}"/>
    /// 管理底层存储以减少短期分配；使用完毕应调用 <see
    /// cref="Dispose"/> 归还资源。 支持序列化（ <see
    /// cref="ToBytes"/>）、日志记录（ <see cref="ToString"/>）以及常用的按位运算（AND/OR/XOR/NOT）。
    /// </summary>
    public struct BitFieldData : IEnumerable<bool>, IDisposable, IEquatable<BitFieldData>
    {
        public static BitFieldData Empty => new BitFieldData(0);

        /// <summary>
        /// 每个 <see cref="ulong"/> 存储的位数（64）。
        /// </summary>
        private const int BitsPerULong = 64;

        /// <summary>
        /// 每个字节包含的位数（8）。
        /// </summary>
        private const int BitsPerByte = 8;

        /// <summary>
        /// 字节级位移量（2^3 = 8），保留以便未来位移计算优化使用。
        /// </summary>
        private const int ShiftPerByte = 3; // 2^3 = 8

        /// <summary>
        /// 底层存储单元（租用自 <see cref="ArrayPool{T}"/>）。
        /// </summary>
        private readonly ulong[] _data;

        /// <summary>
        /// 当前为 true 的位的计数（用于统计与进度显示）。
        /// </summary>
        private int _trueCount;

        /// <summary>
        /// 表示的总位数（资源数量或标志数量）。
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 已设置为 true 的位的数量，便于快速获取完成度或已占用数。
        /// </summary>
        public int TrueCount => _trueCount;

        /// <summary>
        /// 是否所有位均为 false（未分配/未完成/未占用）。
        /// </summary>
        public bool AllFalse => _trueCount == 0;

        /// <summary>
        /// 是否所有位均为 true（全部分配/全部完成/全部占用）。
        /// </summary>
        public bool AllTrue => _trueCount == Length;

        /// <summary>
        /// 表示此位字段序列化为字节时所需的字节数（向上取整）。 适用于网络传输、持久化或日志输出的缓冲分配。
        /// </summary>
        public int LengthInBytes => (Length + BitsPerByte - 1) / BitsPerByte;

        /// <summary>
        /// 已完成百分比（0..100），便于展示进度或统计信息。
        /// </summary>
        public double PercentComplete => (double)_trueCount / Length * 100.0;

        /// <summary>
        /// 是否未初始化或长度为 0。
        /// </summary>
        public bool IsEmpty => _data is null || Length == 0;

        /// <summary>
        /// 以 ulong
        /// 单元访问内部数据的只读切片（注意：切片长度以单元数计，不等于位数）。 仅用于诊断或高性能场景的直接读取；对外语义应以位为单位访问。
        /// </summary>
        public ReadOnlySpan<ulong> DataSpan => _data.AsSpan(0, _data.Length);

        /// <summary>
        /// 从字节序列构造一个 BitField。常用于从网络/磁盘/日志中恢复位状态。
        /// 参数 <paramref name="span"/>
        /// 的长度应为所需字节数（见 <see cref="LengthInBytes"/>）。
        /// </summary>
        /// <param name="span">源字节序列（按实现约定解析为位，通常为大端字节序的块顺序）。</param>
        public BitFieldData(ReadOnlySpan<byte> span)
        {
            // 此构造按输入字节数量决定 WrittenCount（将 span.WrittenCount 视为字节长度）
            int lengthInBits = span.Length * BitsPerByte;
            Length = lengthInBits;
            _data = ArrayPool<ulong>.Shared.Rent((Length + BitsPerULong - 1) / BitsPerULong);
            _trueCount = 0;

            // 将字节组装到 ulong 单元（保持实现内定义的位序）
            int byteIndex = 0;
            int units = _data.Length;
            for (int i = 0; i < units; i++)
            {
                ulong value = 0;
                int bitsToRead = Math.Min(BitsPerULong, Length - i * BitsPerULong);
                int bytesToRead = (bitsToRead + BitsPerByte - 1) / BitsPerByte;

                for (int j = 0; j < bytesToRead; j++)
                {
                    value = (value << BitsPerByte) | span[byteIndex++];
                }

                if (bitsToRead < BitsPerULong)
                {
                    value <<= BitsPerULong - bitsToRead;
                }

                _data[i] = value;
                _trueCount += BitOperations.PopCount(value);
            }
        }

        /// <summary>
        /// 创建指定长度（位数）的 BitField，所有位初始化为 false。适合表示资源池或标志集合。
        /// </summary>
        /// <param name="length">位数（必须为正数）。</param>
        public BitFieldData(int length)
        {
            if (length < 0)
                throw new ArgumentException("长度必须为正数", nameof(length));

            Length = length;
            _data = ArrayPool<ulong>.Shared.Rent((Length + BitsPerULong - 1) / BitsPerULong);
            _trueCount = 0;
        }

        /// <summary>
        /// 复制构造：从另一个 BitField 克隆值，创建独立副本（深拷贝）。
        /// </summary>
        /// <param name="other">源 BitField（不得为空）。</param>
        public BitFieldData(BitFieldData other)
        {
            if (other.IsEmpty)
                throw new ArgumentNullException(nameof(other));

            Length = other.Length;
            _data = (other._data.Clone() as ulong[])!;
            _trueCount = other._trueCount;
        }

        /// <summary>
        /// 从布尔序列构造 BitField（每个布尔值对应一位）。适合小规模初始化或测试场景。
        /// </summary>
        /// <param name="span">布尔序列（元素数量即位数）。</param>
        public BitFieldData(ReadOnlySpan<bool> span)
        {
            if (span == null)
                throw new ArgumentNullException(nameof(span));

            Length = span.Length;
            _data = ArrayPool<ulong>.Shared.Rent((Length + BitsPerULong - 1) / BitsPerULong);
            _trueCount = 0;

            for (int i = 0; i < span.Length; i++)
            {
                if (span[i])
                {
                    Set(i);
                }
            }
        }

        /// <summary>
        /// 索引器：按位访问（零基索引）。用于读取或设置单个位状态。
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
        /// 将指定索引处的位设置为 true（并更新统计计数）。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index)
        {
            ValidateIndex(index);
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
        /// 将指定索引处的位清零（false），并更新统计计数。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(int index)
        {
            ValidateIndex(index);
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
        /// 读取指定索引处的位值（true/false）。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index)
        {
            ValidateIndex(index);
            int arrayIndex = index / BitsPerULong;
            int bitIndex = index % BitsPerULong;
            ulong mask = 1UL << (BitsPerULong - 1 - bitIndex);

            return (_data[arrayIndex] & mask) != 0;
        }

        /// <summary>
        /// 查找第一个值为 true 的位索引；找不到返回 -1。
        /// </summary>
        public int FirstTrue() => FirstTrue(0, Length - 1);

        /// <summary>
        /// 在指定区间内查找第一个 true 位的索引；范围检查将在无效时抛出异常。
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

                int startBit = i == startArrayIndex ? startIndex % BitsPerULong : 0;
                int endBit = i == endArrayIndex ? endIndex % BitsPerULong : BitsPerULong - 1;

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
        /// 查找第一个值为 false 的位索引；找不到返回 -1。
        /// </summary>
        public int FirstFalse() => FirstFalse(0, Length - 1);

        /// <summary>
        /// 在指定区间内查找第一个 false 位的索引；范围检查将在无效时抛出异常。
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

                int startBit = i == startArrayIndex ? startIndex % BitsPerULong : 0;
                int endBit = i == endArrayIndex ? endIndex % BitsPerULong : BitsPerULong - 1;

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
        /// 将 BitField 按实现约定序列化为字节数组，适合用于持久化、网络传输或日志记录。
        /// </summary>
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[LengthInBytes];
            ToBytes(bytes.AsSpan());
            return bytes;
        }

        /// <summary>
        /// 将 BitField 写入目标字节缓冲（零拷贝友好），目标长度应至少为
        /// <see cref="LengthInBytes"/>。
        /// </summary>
        /// <param name="destination">目标缓冲。</param>
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
        /// 对所有位执行逻辑非（NOT），并更新统计信息。
        /// </summary>
        public void Not()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = ~_data[i];
            }

            ZeroUnusedBits();

            _trueCount = 0;
            foreach (ulong value in _data)
            {
                _trueCount += BitOperations.PopCount(value);
            }
        }

        /// <summary>
        /// 对输入字节序列执行按位与（AND）操作（先按约定解码为 BitField）。
        /// </summary>
        /// <param name="span">源字节序列。</param>
        public void And(ReadOnlySpan<byte> span)
        {
            var bitField = new BitFieldData(span);
            And(bitField);
            bitField.Dispose();
        }

        /// <summary>
        /// 将当前实例与另一个 BitField 执行按位与（AND）。
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
        /// 对输入字节序列执行按位或（OR）操作（先按约定解码为 BitField）。
        /// </summary>
        /// <param name="span">源字节序列。</param>
        public void Or(ReadOnlySpan<byte> span)
        {
            var bitField = new BitFieldData(span);
            Or(bitField);
            bitField.Dispose();
        }

        /// <summary>
        /// 将当前实例与另一个 BitField 执行按位或（OR）。
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
        /// 对输入字节序列执行按位异或（XOR）操作（先按约定解码为 BitField）。
        /// </summary>
        /// <param name="span">源字节序列。</param>
        public void Xor(ReadOnlySpan<byte> span)
        {
            var bitField = new BitFieldData(span);
            Xor(bitField);
            bitField.Dispose();
        }

        /// <summary>
        /// 将当前实例与另一个 BitField 执行按位异或（XOR）。
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
        /// 将所有位设置为 true（并处理尾部无效位）。
        /// </summary>
        public void SetAll()
        {
            Array.Fill(_data, ulong.MaxValue);
            ZeroUnusedBits();
            _trueCount = Length;
        }

        /// <summary>
        /// 将所有位清零（false）。
        /// </summary>
        public void ClearAll()
        {
            Array.Clear(_data, 0, _data.Length);
            _trueCount = 0;
        }

        /// <summary>
        /// 计算与另一个 BitField 相同为 true 的位数（用于比较或统计重叠）。
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
        /// 比较两个 BitField 是否逐位相等（包含长度比较）。
        /// </summary>
        public override bool Equals(object? obj)
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
        /// 生成哈希值，适合用于集合键或快速比较。
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
        /// 枚举位序列（按索引从 0 到 WrittenCount-1），可用于记录、导出或逐位处理。
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
        /// 将 BitField 转换为 '1'/'0' 字符串，便于日志、调试或简易导出。
        /// </summary>
        public override string ToString()
        {
            return string.Concat(this.Select(b => b ? '1' : '0'));
        }

        /// <summary>
        /// 验证索引是否有效；在索引越界时抛出 <see cref="IndexOutOfRangeException"/>。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException($"索引 {index} 不处于 (0-{Length - 1})范围内");
        }

        /// <summary>
        /// 验证范围有效性（起止索引），在无效时抛出相应异常。
        /// </summary>
        private void ValidateRange(int startIndex, int endIndex)
        {
            if (startIndex < 0 || startIndex >= Length)
                throw new IndexOutOfRangeException($"start index {startIndex} is out of range (0-{Length - 1})");

            if (endIndex < 0 || endIndex >= Length)
                throw new IndexOutOfRangeException($"end index {endIndex} is out of range (0-{Length - 1})");

            if (startIndex > endIndex)
                throw new ArgumentException("start index must be less than or equal to end index");
        }

        /// <summary>
        /// 验证两个 BitField 长度一致性并在不满足时抛出异常。
        /// </summary>
        private void ValidateSameLength(BitFieldData other)
        {
            if (other.IsEmpty)
                throw new ArgumentNullException(nameof(other));

            if (Length != other.Length)
                throw new ArgumentException("位字段都必须具有相同的长度。");
        }

        /// <summary>
        /// 将最后一个存储单元中超出实际长度的位清零，避免垃圾位影响统计或序列化。
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

        /// <summary>
        /// 归还底层租用数组到数组池；调用后不应再使用该实例。
        /// </summary>
        public void Dispose()
        {
            ArrayPool<ulong>.Shared.Return(_data);
        }

        public bool Equals(BitFieldData other)
        {
            if (IsEmpty && other.IsEmpty) return true;
            if (IsEmpty || other.IsEmpty) return false;

            return _data.AsSpan().SequenceEqual(other._data.AsSpan());
        }
    }
}