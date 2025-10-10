using System.Buffers;
using System.Security.Cryptography;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 不可变哈希值的轻量封装。
    /// - 内部以连续的 <see cref="ulong"/> 按小端方式打包存储（每 8 字节合并为 1 个 <see cref="ulong"/>，不足 8 字节的高位补 0）。
    /// - 提供从字节/ulong 构造、等值比较、转 Hex/Base64 以及常见算法（SHA-256/SHA-1/MD5）的计算辅助。
    /// </summary>
    public readonly struct HashValue : IEquatable<HashValue>
    {
        /// <summary>
        /// 与 SHA-1 输出长度相同的“全零占位”哈希值（20 字节）。
        /// 注意：这不是对空输入计算得到的实际 SHA-1 摘要，仅用于占位或默认值场景。
        /// </summary>
        public static HashValue SHA1Empty = new HashValue(new byte[20]);

        /// <summary>
        /// 与 SHA-256 输出长度相同的“全零占位”哈希值（32 字节）。
        /// 注意：这不是对空输入计算得到的实际 SHA-256 摘要，仅用于占位或默认值场景。
        /// </summary>
        public static HashValue SHA256Empty = new HashValue(new byte[32]);

        /// <summary>
        /// 计算给定数据的 SHA-256 哈希。
        /// </summary>
        /// <param name="span">要计算的字节序列。</param>
        /// <returns>计算得到的 <see cref="HashValue"/>。</returns>
        /// <remarks>内部会分配一个长度为 32 的托管数组以承载算法输出。</remarks>
        public static HashValue SHA256ComputeHash(ReadOnlySpan<byte> span)
        {
            var bytes = SHA256.HashData(span);
            return new HashValue(bytes);
        }

        /// <summary>
        /// 计算给定数据的 SHA-1 哈希。
        /// </summary>
        /// <param name="span">要计算的字节序列。</param>
        /// <returns>计算得到的 <see cref="HashValue"/>。</returns>
        /// <remarks>内部会分配一个长度为 20 的托管数组以承载算法输出。</remarks>
        public static HashValue SHA1ComputeHash(ReadOnlySpan<byte> span)
        {
            var bytes = SHA1.HashData(span);
            return new HashValue(bytes);
        }

        /// <summary>
        /// 计算给定数据的 MD5 哈希（非安全用途）。
        /// </summary>
        /// <param name="span">要计算的字节序列。</param>
        /// <returns>计算得到的 <see cref="HashValue"/>。</returns>
        /// <remarks>MD5 不适合安全场景；仅用于校验或非安全一致性用途。</remarks>
        public static HashValue MD5ComputeHash(ReadOnlySpan<byte> span)
        {
            var bytes = MD5.HashData(span);
            return new HashValue(bytes);
        }

        /// <summary>
        /// 以小端打包的只读内存（每 8 字节合并为 1 个 <see cref="ulong"/>）。
        /// </summary>
        public ReadOnlyMemory<ulong> HashMemory { get; }

        /// <summary>
        /// 哈希的字节长度。
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 指示是否未包含任何哈希数据。
        /// </summary>
        public bool IsEmpty => HashMemory.IsEmpty;

        /// <summary>
        /// 获取只读的 <see cref="ulong"/> 跨度视图（与 <see cref="HashMemory"/> 对应）。
        /// </summary>
        public ReadOnlySpan<ulong> ULongSpan => HashMemory.Span;

        /// <summary>
        /// 以新数组形式返回哈希的字节内容。
        /// </summary>
        /// <remarks>会分配长度为 <see cref="Length"/> 的新字节数组。</remarks>
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
        /// 按字节索引访问哈希内容（0 基）。
        /// </summary>
        /// <param name="index">字节索引（范围：0..Length-1）。</param>
        /// <returns>指定位置的字节值。</returns>
        /// <exception cref="IndexOutOfRangeException">当索引越界时抛出。</exception>
        /// <remarks>内部从打包的 <see cref="ulong"/> 中以“小端顺序”提取目标字节。</remarks>
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
        /// 使用字节序列构造哈希（内部按小端打包为 <see cref="ulong"/> 数组）。
        /// </summary>
        /// <param name="span">源字节序列。</param>
        /// <remarks>会分配一个新的 <see cref="ulong"/> 数组；不足 8 字节的尾部以 0 填充高位。</remarks>
        public HashValue(ReadOnlySpan<byte> span)
        {
            Length = span.Length;
            // 转换为 ulong[] 存储（每 8 字节合并为 1 个 ulong；尾部不足 8 字节高位补 0）
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
        /// 使用现有的 <see cref="ulong"/> 内存与字节长度构造哈希（不拷贝）。
        /// </summary>
        /// <param name="ulongMemory">已打包的小端 <see cref="ulong"/> 连续内存。</param>
        /// <param name="length">哈希的字节长度（必须 ≤ ulongMemory.Length * 8）。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="ulongMemory"/> 为空时。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="length"/> 小于 0，或与 <paramref name="ulongMemory"/> 容量不匹配时。</exception>
        /// <remarks>调用方需保证内存布局与本类型约定一致（每 8 字节为一组的小端打包）。</remarks>
        public HashValue(ReadOnlyMemory<ulong> ulongMemory, int length)
        {
            if (ulongMemory.IsEmpty)
                throw new ArgumentNullException(nameof(ulongMemory), "哈希值不能为空");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "长度不能小于0");
            if (ulongMemory.Length * 8 < length)
                throw new ArgumentOutOfRangeException(nameof(length), "哈希值长度与ulong数组长度不等");

            HashMemory = ulongMemory;
            Length = length > 0 ? length : HashMemory.Length * 8; // 若传入长度为 0，则回退为最大可表示长度
        }

        /// <summary>
        /// 按内容比较两个哈希是否相等（长度相同且逐个 <see cref="ulong"/> 相等）。
        /// </summary>
        /// <remarks>两侧均为空视为相等。</remarks>
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

        /// <summary>
        /// 为集合键生成非加密的哈希码（采样字节混合，性能导向）。
        /// </summary>
        /// <remarks>仅用于字典/集合分布，不代表任何加密强度。</remarks>
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

        /// <summary>
        /// 以十六进制小写字符串表示当前哈希。
        /// </summary>
        public override string ToString() => ToHexString();

        /// <summary>
        /// 等值运算符（基于 <see cref="Equals(HashValue)"/>）。
        /// </summary>
        public static bool operator ==(HashValue left, HashValue right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// 不等运算符（基于 <see cref="Equals(HashValue)"/>）。
        /// </summary>
        public static bool operator !=(HashValue left, HashValue right)
        {
            return !Equals(left, right);
        }

        #region Convert

        /// <summary>
        /// 转为十六进制小写字符串。
        /// </summary>
        /// <returns>十六进制小写字符串（无分隔符）。</returns>
        /// <remarks>临时租用 <see cref="ArrayPool{T}"/> 缓冲区以降低分配；输出为小写。</remarks>
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
        /// 转为 Base64 字符串。
        /// </summary>
        /// <returns>Base64 编码字符串。</returns>
        /// <remarks>临时租用 <see cref="ArrayPool{T}"/> 缓冲区以降低分配。</remarks>
        public string ToBase64String()
        {
            byte[] bytes = ArrayPool<byte>.Shared.Rent(Length);
            for (int i = 0; i < Length; i++)
            {
                bytes[i] = this[i];
            }
            var result = Convert.ToBase64String(bytes, 0, Length);
            ArrayPool<byte>.Shared.Return(bytes);
            return result;
        }

        #endregion

        /// <summary>
        /// 从十六进制字符串创建 <see cref="HashValue"/>。
        /// </summary>
        /// <param name="hexString">十六进制字符串（大小写均可，长度必须为偶数）。</param>
        /// <returns>解析得到的 <see cref="HashValue"/>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="hexString"/> 为空或 null。</exception>
        /// <exception cref="ArgumentException">当长度不是偶数。</exception>
        /// <exception cref="FormatException">当包含非十六进制字符时。</exception>
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
        /// 从 Base64 字符串创建 <see cref="HashValue"/>。
        /// </summary>
        /// <param name="base64String">Base64 字符串。</param>
        /// <returns>解析得到的 <see cref="HashValue"/>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="base64String"/> 为空或 null。</exception>
        /// <exception cref="ArgumentException">当格式无效时。</exception>
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

        /// <summary>
        /// 与任意对象进行等值比较。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is HashValue && Equals((HashValue)obj);
        }
    }
}
