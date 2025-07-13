using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// HashValues结构体，实现了IEquatable<HashValues>接口，用于比较两个HashValues实例是否相等。
    /// </summary>
    public struct HashValues : IEquatable<HashValues>
    {
        public static HashValues Empty => new HashValues();

        private static int[] HashLengths = new int[] { 20, 24, 32, 40 }; // 支持的哈希长度

        // 分片哈希数组（每个哈希使用3个ulong存储，最后一个ulong的后4字节为填充）
        private readonly ReadOnlyMemory<ulong> _pieceHashes;

        /// <summary>
        /// 获取哈希值的长度。
        /// </summary>
        public int HashLength { get; }

        /// <summary>
        /// 总分片数量
        /// </summary>
        public int PieceCount => _pieceHashes.Length / HashLength;

        /// <summary>
        /// 获取每个哈希值中无符号长整数的数量。
        /// </summary>
        /// <value>
        /// 每个哈希值中无符号长整数的数量。
        /// </value>
        private int ULongsPerHash { get; }

        /// <summary>
        /// 判断当前实例是否为空。
        /// </summary>
        /// <returns>如果实例为空，则返回true；否则返回false。</returns>
        public bool IsEmpty => _pieceHashes.IsEmpty || _pieceHashes.Length == 0;

        /// <summary>
        /// 获取只读的 64 位无符号整数的内存视图
        /// </summary>
        /// <returns>返回一个只读的 64 位无符号整数的内存视图</returns>
        public ReadOnlyMemory<ulong> ULongMemory => _pieceHashes;

        /// <summary>
        /// 通过索引获取指定分片哈希值。
        /// </summary>
        /// <param name="index">分片索引。</param>
        /// <returns>返回指定索引处的分片哈希值。</returns>
        public HashValue this[int index]
        {
            get
            {
                return GetPieceHash(index);
            }
        }

        /// <summary>
        /// 从字节数组创建分片哈希管理器
        /// </summary>
        public HashValues(ReadOnlySpan<byte> pieceHashes)
        {
            HashLength = ValidateHashLength(pieceHashes);

            // 转换为ulong[]存储（每个哈希占3个ulong，最后一个ulong的后4字节为0）
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
        /// 初始化 HashValues 类的新实例。
        /// </summary>
        /// <param name="pieceHashes">分片哈希数组。</param>
        /// <param name="hashLength">哈希长度。</param>
        /// <exception cref="ArgumentException">
        /// 如果 <paramref name="pieceHashes"/> 为空或为 0，则抛出此异常。
        /// 如果 <paramref name="hashLength"/> 小于等于 0 或不在支持的哈希长度列表中，则抛出此异常。
        /// </exception>
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
        /// 获取指定分片索引的哈希值
        /// </summary>
        /// <param name="pieceIndex">分片索引</param>
        /// <returns>返回指定分片索引的哈希值</returns>
        /// <exception cref="IndexOutOfRangeException">如果分片索引超出范围，则抛出此异常</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashValue GetPieceHash(int pieceIndex)
        {
            if ((uint)pieceIndex >= (uint)PieceCount)
                throw new IndexOutOfRangeException($"分片索引 {pieceIndex} 超出范围");

            int offset = pieceIndex * ULongsPerHash;
            return new HashValue(_pieceHashes.Slice(offset, ULongsPerHash), HashLength);
        }

        /// <summary>
        /// 验证哈希长度是否合法
        /// </summary>
        /// <param name="hashSpan">哈希字节数组</param>
        /// <param name="hashLength">哈希长度，默认为-1</param>
        /// <returns>返回合法的哈希长度</returns>
        /// <exception cref="ArgumentException">如果哈希字节数组为空或哈希长度不符合要求，则抛出此异常</exception>
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
        /// 验证指定索引处的数据块哈希值是否匹配
        /// </summary>
        /// <param name="pieceIndex">数据块索引</param>
        /// <param name="hash">要验证的哈希值</param>
        /// <returns>如果哈希值匹配则返回true，否则返回false</returns>
        public bool ValidatePiece(int pieceIndex, HashValue hash)
        {
            if ((uint)pieceIndex >= (uint)PieceCount || hash.IsEmpty)
                return false;

            var pieceHash = GetPieceHash(pieceIndex);
            return pieceHash.Equals(hash);
        }

        /// <summary>
        /// 验证指定索引处的数据块哈希值是否匹配
        /// </summary>
        /// <param name="pieceIndex">数据块索引</param>
        /// <param name="span">要验证的哈希值字节数组</param>
        /// <returns>如果哈希值匹配则返回true，否则返回false</returns>
        public bool ValidatePiece(int pieceIndex, ReadOnlySpan<byte> span)
        {
            if ((uint)pieceIndex >= (uint)PieceCount || span.IsEmpty || span.Length != HashLength)
                return false;

            // 比较ulong数组
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

            // 最后一个ulong
            int index = ulongOffset + ULongsPerHash;
            ulong lastStored = _pieceHashes.Span[index];
            ulong lastComputed = BinaryPrimitives.ReadUInt32LittleEndian(
                span.Slice(ULongsPerHash * 8, remainderLength));

            return (lastStored & 0xFFFFFFFF) == lastComputed;
        }

        /// <summary>
        /// 通过哈希值查找对应的片段索引。
        /// </summary>
        /// <param name="hash">要查找的哈希值。</param>
        /// <returns>如果找到匹配的哈希值，则返回对应的片段索引；否则返回-1。</returns>
        /// <exception cref="ArgumentException">如果哈希值的长度不等于HashLength。</exception>
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
        /// 通过哈希值查找对应的片段索引。
        /// </summary>
        /// <param name="hash">要查找的哈希值。</param>
        /// <returns>如果找到匹配的哈希值，则返回对应的片段索引；否则返回-1。</returns>
        /// <exception cref="ArgumentException">如果哈希值的长度不等于HashLength。</exception>
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
        /// 比较两个分片哈希数组是否完全相同
        /// </summary>
        public bool Equals(HashValues other)
        {
            if (PieceCount != other.PieceCount)
                return false;

            return _pieceHashes.Span.SequenceEqual(other._pieceHashes.Span);
        }

        public static bool operator ==(HashValues left, HashValues right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HashValues left, HashValues right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 计算结构体的哈希码
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

        public override bool Equals(object obj)
        {
            return obj is HashValues && Equals((HashValues)obj);
        }
    }
}
