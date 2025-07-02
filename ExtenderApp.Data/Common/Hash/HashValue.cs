namespace ExtenderApp.Data
{
    public readonly struct HashValue : IEquatable<HashValue>
    {
        public static HashValue Empty = new HashValue(null);

        /// <summary>
        /// 存储哈希值的字节数组。
        /// </summary>
        internal readonly byte[] _hashBytes;

        /// <summary>
        /// 获取哈希值的字节长度。
        /// </summary>
        /// <returns>哈希值的字节长度。</returns>
        public int Length => _hashBytes.Length;

        /// <summary>
        /// 获取一个布尔值，指示 <see cref="_hashBytes"/> 是否为空。
        /// </summary>
        /// <returns>如果 <see cref="_hashBytes"/> 为空，则返回 true；否则返回 false。</returns>
        /// <remarks>
        /// 当 <see cref="_hashBytes"/> 为 null 时，表示当前对象没有包含有效的哈希值。
        /// </remarks>
        public bool IsEmpty => _hashBytes == null;

        /// <summary>
        /// 初始化 <see cref="HashValue"/> 类的新实例。
        /// </summary>
        /// <param name="hashBytes">包含哈希值的字节数组。</param>
        /// <exception cref="ArgumentNullException"><paramref name="hashBytes"/> 为 null。</exception>
        /// <remarks>
        /// <paramref name="hashBytes"/> 参数不能为 null。
        /// <see cref="_hashBytes"/> 属性会复制传入的字节数组，以确保不可变性。
        /// </remarks>
        public HashValue(byte[] hashBytes)
        {
            _hashBytes = hashBytes;
        }

        // 比较两个哈希值是否相等
        public bool Equals(HashValue other)
        {
            if (other.IsEmpty || IsEmpty) return false;
            if (_hashBytes.Length != other._hashBytes.Length) return false;

            return _hashBytes.SequenceEqual(other._hashBytes);
        }

        public override int GetHashCode()
        {
            //// 使用前4个字节生成哈希码（简化版）
            //if (HashBytes.Length >= 4)
            //    return BitConverter.ToInt32(HashBytes, 0);
            //return HashBytes.Length;

            // 分段组合所有字节，平衡性能和分布
            if (_hashBytes.Length == 0) return 0;

            Span<byte> span = _hashBytes.AsSpan();
            int hash = span[0];
            int step = Math.Max(1, span.Length / 4);

            for (int i = step; i < span.Length; i += step)
            {
                hash = (hash * 42) ^ span[i];
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
            return BitConverter.ToString(_hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 将HashValue转换为Base64字符串。
        /// </summary>
        /// <returns>转换后的Base64字符串。</returns>
        public string ToBase64String()
        {
            return Convert.ToBase64String(_hashBytes.AsSpan());
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
