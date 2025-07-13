using System;
using System.Buffers;
using System.Security.Cryptography;

namespace ExtenderApp.Data
{
    public readonly struct HashValue : IEquatable<HashValue>
    {
        public static HashValue Empty = new HashValue(null);

        /// <summary>
        /// 使用SHA256算法计算给定字节序列的哈希值。
        /// </summary>
        /// <param name="span">包含要计算哈希值的字节序列的只读跨度。</param>
        /// <returns>返回计算得到的哈希值。</returns>
        public static HashValue SHA256ComputeHash(ReadOnlySpan<byte> span)
        {
            var bytes = SHA256.HashData(span);
            return new HashValue(bytes);
        }

        /// <summary>
        /// 使用SHA1算法计算给定字节序列的哈希值。
        /// </summary>
        /// <param name="span">包含要计算哈希值的字节序列的只读跨度。</param>
        /// <returns>返回计算得到的哈希值。</returns>
        public static HashValue SHA1ComputeHash(ReadOnlySpan<byte> span)
        {
            var bytes = SHA1.HashData(span);
            return new HashValue(bytes);
        }

        /// <summary>
        /// 使用MD5算法计算给定字节序列的哈希值。
        /// </summary>
        /// <param name="span">包含要计算哈希值的字节序列的只读跨度。</param>
        /// <returns>返回计算得到的哈希值。</returns>
        public static HashValue MD5ComputeHash(ReadOnlySpan<byte> span)
        {
            var bytes = MD5.HashData(span);
            return new HashValue(bytes);
        }

        /// <summary>
        /// 用于存储哈希值的ulong数组
        /// </summary>
        public ReadOnlyMemory<ulong> HashMemory { get; }

        /// <summary>
        /// 获取哈希值的字节长度。
        /// </summary>
        /// <returns>哈希值的字节长度。</returns>
        public int Length { get; }

        /// <summary>
        /// 获取一个布尔值，指示 <see cref="HashMemory"/> 是否为空。
        /// </summary>
        /// <returns>如果 <see cref="HashMemory"/> 为空，则返回 true；否则返回 false。</returns>
        /// <remarks>
        /// 当 <see cref="HashMemory"/> 为 null 时，表示当前对象没有包含有效的哈希值。
        /// </remarks>
        public bool IsEmpty => HashMemory.IsEmpty;

        /// <summary>
        /// 获取只读字节跨度，表示哈希值。
        /// </summary>
        /// <returns>返回表示哈希值的只读字节跨度。</returns>
        public ReadOnlySpan<ulong> ULongSpan => HashMemory.Span;

        /// <summary>
        /// 获取当前对象的字节数组形式的哈希值。
        /// </summary>
        /// <returns>包含当前对象哈希值的字节数组。</returns>
        public byte[] HashBytes
        {
            get
            {
                byte[] bytes = new byte[Length];
                for (int i = 0; i < Length; i++)
                {
                    bytes[i] = this[i];
                }
                return bytes;
            }
        }

        /// <summary>
        /// 获取指定索引处的字节值。
        /// </summary>
        /// <param name="index">要获取的字节的索引。</param>
        /// <returns>指定索引处的字节值。</returns>
        /// <exception cref="IndexOutOfRangeException">如果索引小于0或大于等于长度，则抛出此异常。</exception>
        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new IndexOutOfRangeException($"索引 {index} 超出范围，长度为 {Length}。");

                int ulongIndex = index / 8;
                int byteOffset = index % 8;
                return (byte)((ULongSpan[ulongIndex] >> (byteOffset * 8)) & 0xFF);
            }
        }

        /// <summary>
        /// 构造函数，通过ReadOnlySpan<byte>初始化HashValue对象
        /// </summary>
        /// <param name="span">用于初始化的字节序列</param>
        public HashValue(ReadOnlySpan<byte> span)
        {
            Length = span.Length;
            // 转换为ulong[]存储（每个哈希占3个ulong，最后一个ulong的后4字节为0）
            ulong[] memory = new ulong[(span.Length + 7) / 8];
            for (int i = 0; i < span.Length; i++)
            {
                int ulongIndex = i / 8;
                int byteOffset = i % 8;
                memory[ulongIndex] |= ((ulong)span[i]) << (byteOffset * 8);
            }
            HashMemory = memory;
        }

        /// <summary>
        /// 构造函数，通过ulong数组和长度初始化HashValue对象
        /// </summary>
        /// <param name="ulongMemory">用于初始化的ulong数组</param>
        /// <param name="length">哈希值的长度</param>
        /// <exception cref="ArgumentNullException">当ulongArray为null时抛出</exception>
        public HashValue(ReadOnlyMemory<ulong> ulongMemory, int length)
        {
            if (ulongMemory.IsEmpty)
                throw new ArgumentNullException(nameof(ulongMemory), "哈希值不能为空");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "长度不能小于0");
            if (ulongMemory.Length * 8 < length)
                throw new ArgumentOutOfRangeException(nameof(length), "哈希值长度与ulong数组长度不等");

            HashMemory = ulongMemory;
            Length = length > 0 ? length : HashMemory.Length * 8; // 确保长度大于0，如果传入的长度无效，则使用默认长度
        }

        // 比较两个哈希值是否相等
        public bool Equals(HashValue other)
        {
            if ((other.IsEmpty && IsEmpty) || HashMemory.Equals(other.HashMemory)) return true; // 两个都是空的哈希值视为相等
            if (other.IsEmpty || IsEmpty || Length != other.Length) return false;

            var otherSpan = other.HashMemory.Span;
            var span = HashMemory.Span;
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != otherSpan[i])
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            //// 使用前4个字节生成哈希码（简化版）
            //if (HashBytes.Length >= 4)
            //    return BitConverter.ToInt32(HashBytes, 0);
            //return HashBytes.Length;

            // 分段组合所有字节，平衡性能和分布
            if (HashMemory.Length == 0) return 0;

            int hash = this[0];
            int step = Math.Max(1, Length / 4);

            for (int i = step; i < Length; i += step)
            {
                hash = (hash * 42) ^ this[i];
            }

            return hash;
        }

        public override string ToString() => ToHexString();

        public static bool operator ==(HashValue left, HashValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HashValue left, HashValue right)
        {
            return !Equals(left, right);
        }

        #region Convert

        /// <summary>
        /// 将字节数组转换为十六进制字符串表示形式。
        /// </summary>
        /// <returns>返回转换后的十六进制字符串。</returns>
        public string ToHexString()
        {
            byte[] bytes = ArrayPool<byte>.Shared.Rent(Length);
            for (int i = 0; i < Length; i++)
            {
                bytes[i] = this[i];
            }
            var result = BitConverter.ToString(bytes, 0, Length).Replace("-", "").ToLowerInvariant();
            ArrayPool<byte>.Shared.Return(bytes);
            return result;
        }

        /// <summary>
        /// 将HashValue转换为Base64字符串。
        /// </summary>
        /// <returns>转换后的Base64字符串。</returns>
        public string ToBase64String()
        {
            byte[] bytes = ArrayPool<byte>.Shared.Rent(Length);
            for (int i = 0; i < Length; i++)
            {
                bytes[i] = this[i];
            }
            var result = Convert.ToBase64String(bytes);
            ArrayPool<byte>.Shared.Return(bytes);
            return result;
        }

        #endregion

        /// <summary>
        /// 将十六进制字符串转换为 HashValue 对象。
        /// </summary>
        /// <param name="hexString">十六进制字符串。</param>
        /// <returns>转换后的 HashValue 对象。</returns>
        /// <exception cref="ArgumentNullException">如果 hexString 为 null 或空字符串，则引发此异常。</exception>
        /// <exception cref="ArgumentException">如果 hexString 的长度不是偶数，则引发此异常。</exception>
        public static HashValue FromHexString(string hexString)
        {
            if (string.IsNullOrEmpty(hexString)) throw new ArgumentNullException(nameof(hexString));
            if (hexString.Length % 2 != 0) throw new ArgumentException("十六进制字符串长度无效。", nameof(hexString));

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return new HashValue(bytes);
        }

        /// <summary>
        /// 将Base64字符串转换为HashValue对象
        /// </summary>
        /// <param name="base64String">Base64字符串</param>
        /// <returns>转换后的HashValue对象</returns>
        /// <exception cref="ArgumentNullException">如果base64String为空或null，则抛出此异常</exception>
        /// <exception cref="ArgumentException">如果base64String格式无效，则抛出此异常</exception>
        public static HashValue FromBase64String(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) throw new ArgumentNullException(nameof(base64String));

            try
            {
                byte[] bytes = Convert.FromBase64String(base64String);
                return new HashValue(bytes);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid Base64 string format.", nameof(base64String), ex);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is HashValue && Equals((HashValue)obj);
        }
    }
}
