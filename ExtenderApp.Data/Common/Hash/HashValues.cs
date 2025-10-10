using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一组“分片哈希”的只读视图与操作集合。
    /// 支持的单片哈希长度为 20/24/32/40 字节（如 SHA-1/192/256/320），
    /// 内部以连续的 <see cref="ulong"/> 序列按小端方式打包存储，每片占用 ⌈Length/8⌉ 个 ulong。
    /// 提供按索引访问、验证、查找与等值比较等功能。
    /// </summary>
    public struct HashValues : IEquatable<HashValues>
    {
        /// <summary>
        /// 空实例。
        /// </summary>
        public static HashValues Empty => new HashValues();

        /// <summary>
        /// 受支持的单片哈希长度（字节）。
        /// </summary>
        private static int[] HashLengths = new int[] { 20, 24, 32, 40 };

        /// <summary>
        /// 分片哈希的底层存储（连续的 ulong 内存）。
        /// 每片哈希占用 <see cref="ULongsPerHash"/> 个 ulong；若长度非 8 的倍数，最后一个 ulong 的高位字节为填充（0）。
        /// </summary>
        private readonly ReadOnlyMemory<ulong> _pieceHashes;

        /// <summary>
        /// 单片哈希的字节长度（例如 20、24、32、40）。
        /// </summary>
        public int HashLength { get; }

        /// <summary>
        /// 分片总数。
        /// </summary>
        public int PieceCount => _pieceHashes.Length / HashLength;

        /// <summary>
        /// 单片哈希占用的 ulong 数（= ⌈<see cref="HashLength"/>/8⌉）。
        /// </summary>
        private int ULongsPerHash { get; }

        /// <summary>
        /// 指示当前是否为“空”（无任何分片）。
        /// </summary>
        public bool IsEmpty => _pieceHashes.IsEmpty || _pieceHashes.Length == 0;

        /// <summary>
        /// 获取底层只读的 ulong 连续内存视图（按片依次排布）。
        /// </summary>
        public ReadOnlyMemory<ulong> ULongMemory => _pieceHashes;

        /// <summary>
        /// 按索引获取分片哈希值。
        /// </summary>
        /// <param name="index">分片索引（从 0 开始）。</param>
        /// <returns>对应索引的 <see cref="HashValue"/>。</returns>
        public HashValue this[int index]
        {
            get
            {
                return GetPieceHash(index);
            }
        }

        /// <summary>
        /// 通过连续的分片哈希字节序列构造。
        /// 会根据总长度自动推断单片长度（必须能被 20/24/32/40 中某个整除），
        /// 并按“小端”方式打包至新的 ulong 数组。
        /// </summary>
        /// <param name="pieceHashes">按片拼接的哈希字节序列。</param>
        /// <exception cref="ArgumentException">字节序列为空或长度不合法时抛出。</exception>
        public HashValues(ReadOnlySpan<byte> pieceHashes)
        {
            HashLength = ValidateHashLength(pieceHashes);

            // 转换为 ulong[] 存储（每片占 ⌈HashLength/8⌉ 个 ulong；不足 8 的尾部以 0 填充）
            var ulongArray = new ulong[(pieceHashes.Length + 7) / 8];
            var span = ulongArray.AsSpan();
            ULongsPerHash = (HashLength + 7) / 8;

            for (int i = 0; i < pieceHashes.Length; i++)
            {
                int ulongIndex = i / 8;
                int byteOffset = i % 8;
                span[ulongIndex] |= ((ulong)pieceHashes[i]) << (byteOffset * 8);
            }
            _pieceHashes = ulongArray;
        }

        /// <summary>
        /// 使用现有的 ulong 只读内存作为底层存储创建实例（不会拷贝）。
        /// 调用方需保证该内存遵循“每片连续 <see cref="ULongsPerHash"/> 个 <see cref="ulong"/>”的布局约定。
        /// </summary>
        /// <param name="pieceHashes">连续的分片哈希存储（只读）。</param>
        /// <param name="hashLength">单片哈希的字节长度（20/24/32/40）。</param>
        /// <exception cref="ArgumentException">存储为空或长度不受支持时抛出。</exception>
        public HashValues(ReadOnlyMemory<ulong> pieceHashes, int hashLength)
        {
            if (pieceHashes.IsEmpty || pieceHashes.Length == 0)
                throw new ArgumentException("分片哈希数组不能为空", nameof(pieceHashes));
            if (hashLength <= 0 || !HashLengths.Contains(hashLength))
                throw new ArgumentException($"不支持的哈希长度: {hashLength}，支持的长度为: {string.Join(", ", HashLengths)}", nameof(hashLength));

            _pieceHashes = pieceHashes;
            HashLength = hashLength;
            ULongsPerHash = (hashLength + 7) / 8;
        }

        /// <summary>
        /// 获取指定分片索引的哈希值。
        /// </summary>
        /// <param name="pieceIndex">分片索引（从 0 开始）。</param>
        /// <returns>对应的 <see cref="HashValue"/>。</returns>
        /// <exception cref="IndexOutOfRangeException">索引越界时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue GetPieceHash(int pieceIndex)
        {
            if ((uint)pieceIndex >= (uint)PieceCount)
                throw new IndexOutOfRangeException($"分片索引 {pieceIndex} 超出范围");

            int offset = pieceIndex * ULongsPerHash;
            return new HashValue(_pieceHashes.Slice(offset, ULongsPerHash), HashLength);
        }

        /// <summary>
        /// 根据总字节序列推断单片哈希长度。
        /// </summary>
        /// <param name="hashSpan">拼接后的哈希字节序列。</param>
        /// <returns>合法的单片长度（字节）。</returns>
        /// <exception cref="ArgumentException">序列为空或无法被任何受支持长度整除时抛出。</exception>
        public int ValidateHashLength(ReadOnlySpan<byte> hashSpan)
        {
            if (hashSpan.IsEmpty || hashSpan.Length == 0)
                throw new ArgumentException("哈希字节数组不能为空", nameof(hashSpan));

            // 检查哈希长度是否在支持的范围内
            for (int i = 0; i < HashLengths.Length; i++)
            {
                int supportedLength = HashLengths[i];
                if (hashSpan.Length % supportedLength == 0)
                    return supportedLength;
            }
            throw new ArgumentException($"不支持的哈希长度: {hashSpan.Length}，支持的长度为: {string.Join(", ", HashLengths)}", nameof(hashSpan));
        }

        /// <summary>
        /// 验证指定分片是否与给定哈希相等。
        /// </summary>
        /// <param name="pieceIndex">分片索引。</param>
        /// <param name="hash">要比对的哈希。</param>
        /// <returns>匹配返回 true；否则返回 false。</returns>
        public bool ValidatePiece(int pieceIndex, HashValue hash)
        {
            if ((uint)pieceIndex >= (uint)PieceCount || hash.IsEmpty)
                return false;

            var pieceHash = GetPieceHash(pieceIndex);
            return pieceHash.Equals(hash);
        }

        /// <summary>
        /// 验证指定分片是否与给定字节序列相等（长度需等于 <see cref="HashLength"/>）。
        /// </summary>
        /// <param name="pieceIndex">分片索引。</param>
        /// <param name="span">要比对的哈希字节序列。</param>
        /// <returns>匹配返回 true；否则返回 false。</returns>
        public bool ValidatePiece(int pieceIndex, ReadOnlySpan<byte> span)
        {
            if ((uint)pieceIndex >= (uint)PieceCount || span.IsEmpty || span.Length != HashLength)
                return false;

            // 比较前 n 个完整的 ulong
            int ulongOffset = pieceIndex * ULongsPerHash;
            int remainderLength = HashLength % 8 > 0 ? HashLength % 8 : 0;
            int ulongLength = remainderLength > 0 ? ULongsPerHash - 1 : ULongsPerHash;

            for (int i = ulongOffset; i < ulongLength; i++)
            {
                ulong storedValue = _pieceHashes.Span[i];
                ulong computedValue = BinaryPrimitives.ReadUInt64LittleEndian(
                    span.Slice(i * 8, 8));

                if (storedValue != computedValue)
                    return false;
            }

            if (ulongLength == ULongsPerHash)
                return true;

            // 处理最后不足 8 字节的部分（按小端比较低位）。
            int index = ulongOffset + ULongsPerHash;
            ulong lastStored = _pieceHashes.Span[index];
            ulong lastComputed = BinaryPrimitives.ReadUInt32LittleEndian(
                span.Slice(ULongsPerHash * 8, remainderLength));

            return (lastStored & 0xFFFFFFFF) == lastComputed;
        }

        /// <summary>
        /// 在线性扫描中查找与给定字节序列匹配的分片索引。
        /// </summary>
        /// <param name="hash">目标哈希字节序列（长度必须等于 <see cref="HashLength"/>）。</param>
        /// <returns>返回首个匹配的分片索引；未找到返回 -1。</returns>
        /// <exception cref="ArgumentException">长度不等于 <see cref="HashLength"/> 时抛出。</exception>
        public int FindPieceIndex(ReadOnlySpan<byte> hash)
        {
            if (hash.Length != HashLength)
                throw new ArgumentException($"哈希长度必须为{HashLength}字节", nameof(hash));

            ReadOnlySpan<uint> hashAsUInts = MemoryMarshal.Cast<byte, uint>(hash);

            for (int i = 0; i < PieceCount; i++)
            {
                int uintOffset = i * ULongsPerHash;
                bool match = true;

                for (int j = 0; j < ULongsPerHash; j++)
                {
                    if (_pieceHashes.Span[uintOffset + j] != hashAsUInts[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 在线性扫描中查找与给定 <see cref="HashValue"/> 匹配的分片索引。
        /// </summary>
        /// <param name="hash">目标哈希值（长度必须等于 <see cref="HashLength"/>）。</param>
        /// <returns>返回首个匹配的分片索引；未找到返回 -1。</returns>
        /// <exception cref="ArgumentException">长度不等于 <see cref="HashLength"/> 时抛出。</exception>
        public int FindPieceIndex(HashValue hash)
        {
            if (hash.Length != HashLength)
                throw new ArgumentException($"哈希长度必须为{HashLength}字节", nameof(hash));

            for (int i = 0; i < PieceCount; i++)
            {
                int ulongOffset = i * ULongsPerHash;
                bool match = true;

                for (int j = 0; j < ULongsPerHash; j++)
                {
                    if (_pieceHashes.Span[ulongOffset + j] != hash.ULongSpan[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 判断与另一实例的分片内容是否完全一致（长度与每个 <see cref="ulong"/> 均相等）。
        /// </summary>
        public bool Equals(HashValues other)
        {
            if (PieceCount != other.PieceCount)
                return false;

            return _pieceHashes.Span.SequenceEqual(other._pieceHashes.Span);
        }

        /// <summary>
        /// 等值比较运算符。
        /// </summary>
        public static bool operator ==(HashValues left, HashValues right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 不等比较运算符。
        /// </summary>
        public static bool operator !=(HashValues left, HashValues right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 计算实例的哈希码（基于前最多 4 个 <see cref="ulong"/> 与 <see cref="HashLength"/>）。
        /// </summary>
        public override int GetHashCode()
        {
            if (_pieceHashes.Length >= 4)
            {
                return HashCode.Combine(
                    _pieceHashes.Span[0],
                    _pieceHashes.Span[1],
                    _pieceHashes.Span[2],
                    _pieceHashes.Span[3],
                    HashLength
                );
            }
            return HashCode.Combine(HashLength, _pieceHashes.Length);
        }

        /// <summary>
        /// 与对象进行等值比较。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is HashValues && Equals((HashValues)obj);
        }
    }
}
