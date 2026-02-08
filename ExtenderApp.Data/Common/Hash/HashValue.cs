namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个不可变的哈希值轻量级封装结构。
    /// <para>内部基于 <see cref="ByteBlock"/> 管理内存，提供对哈希摘要的等值比较、生命周期管理以及十六进制/Base64 格式转换。</para>
    /// </summary>
    public readonly struct HashValue : IEquatable<HashValue>, IDisposable
    {
        /// <summary>
        /// 与 SHA-1 输出长度相同的“全零占位”哈希值（20 字节）。 <remarks>注意：这不是对空输入计算得到的实际 SHA-1 摘要，仅用于占位或默认值场景。</remarks>
        /// </summary>
        public static HashValue SHA1Empty = new HashValue();

        /// <summary>
        /// 与 SHA-256 输出长度相同的“全零占位”哈希值（32 字节）。 <remarks>注意：这不是对空输入计算得到的实际 SHA-256 摘要，仅用于占位或默认值场景。</remarks>
        /// </summary>
        public static HashValue SHA256Empty = new HashValue();

        /// <summary>
        /// 哈希值长度数组，包含常见哈希算法的输出长度（以字节为单位）。
        /// </summary>
        private static int[] HashLengths = new int[] { 20, 24, 32, 40 };

        /// <summary>
        /// 存储哈希摘要数据的字节块。
        /// </summary>
        private readonly ByteBlock _block;

        /// <summary>
        /// 获取哈希值的原始字节长度。
        /// </summary>
        /// <value>哈希摘要的字节总数。</value>
        public int Length => _block.Committed;

        /// <summary>
        /// 获取一个值，指示当前哈希值是否为空或未初始化。
        /// </summary>
        /// <value>如果哈希值为空，则为 <see langword="true"/>；否则为 <see langword="false"/>。</value>
        public bool IsEmpty => _block.IsEmpty;

        /// <summary>
        /// 获取指向当前哈希摘要内存的只读跨度 ( <see cref="ReadOnlySpan{T}"/>)。
        /// </summary>
        public ReadOnlySpan<byte> CommittedSpan => _block.CommittedSpan;

        /// <summary>
        /// 获取表示当前哈希摘要内存的只读内存块 ( <see cref="ReadOnlyMemory{T}"/>)。
        /// </summary>
        public ReadOnlyMemory<byte> UnreadMemory => _block.CommittedMemory;

        /// <summary>
        /// 使用指定的 <see cref="ByteBlock"/> 初始化 <see cref="HashValue"/> 的新实例。
        /// </summary>
        /// <param name="block">包含哈希摘要数据的字节块。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="block"/> 为空或不包含有效数据时抛出。</exception>
        public HashValue(ByteBlock block)
        {
            if (block.IsEmpty || CheckHashLength(block.Committed))
                throw new ArgumentNullException(nameof(block));

            _block = new(block.CommittedSpan);
        }

        /// <summary>
        /// 从指定的字节跨度初始化 <see cref="HashValue"/> 的新实例。
        /// </summary>
        /// <param name="span">包含原始哈希摘要的字节数据。</param>
        /// <exception cref="ArgumentException">当 <paramref name="span"/> 为空或长度为 0 时抛出。</exception>
        public HashValue(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty || CheckHashLength(span.Length))
                throw new ArgumentException("哈希数据不能为空。", nameof(span));

            _block = new(span);
        }

        /// <summary>
        /// 检查输入的哈希长度是否合法
        /// </summary>
        /// <param name="length">哈希长度</param>
        /// <returns>如果合法返回 true，否者返回 false</returns>
        private static bool CheckHashLength(int length)
        {
            for (int i = 0; i < HashLengths.Length; i++)
            {
                if (HashLengths[i] == length)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 按内容比较两个哈希值是否相等。
        /// </summary>
        /// <param name="other">要与当前实例进行比较的另一个 <see cref="HashValue"/>。</param>
        /// <returns>如果两个哈希值的长度和字节内容完全一致，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <remarks>两个未初始化的空哈希值被视为相等。</remarks>
        public bool Equals(HashValue other)
        {
            if (other.IsEmpty && IsEmpty) return true;
            if (other.IsEmpty || IsEmpty || Length != other.Length) return false;
            return _block.Equals(other._block);
        }

        /// <summary>
        /// 生成当前哈希值的哈希代码，用于哈希表等集合。
        /// </summary>
        /// <returns>一个整数哈希代码。</returns>
        public override int GetHashCode()
        {
            return _block.GetHashCode();
        }

        /// <summary>
        /// 返回当前哈希值的十六进制小写字符串表示。
        /// </summary>
        /// <returns>等效于调用 <see cref="ToHexString"/> 的结果。</returns>
        public override string ToString() => ToHexString();

        /// <summary>
        /// 确定指定对象是否为 <see cref="HashValue"/> 且内容相等。
        /// </summary>
        /// <param name="obj">要比较的对象。</param>
        /// <returns>如果相等则为 <see langword="true"/>。</returns>
        public override bool Equals(object? obj)
        {
            return obj is HashValue other && Equals(other);
        }

        /// <summary>
        /// 释放由该哈希值持有的底层 <see cref="ByteBlock"/> 资源。
        /// </summary>
        public void Dispose()
        {
            _block.Dispose();
        }

        /// <summary>
        /// 确定两个 <see cref="HashValue"/> 实例是否相等。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        /// <returns>如果相等则为 <see langword="true"/>。</returns>
        public static bool operator ==(HashValue left, HashValue right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 确定两个 <see cref="HashValue"/> 实例是否不相等。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        /// <returns>如果不相等则为 <see langword="true"/>。</returns>
        public static bool operator !=(HashValue left, HashValue right)
        {
            return !left.Equals(right);
        }

        #region Convert

        /// <summary>
        /// 将当前哈希值转换为十六进制格式的小写字符串。
        /// </summary>
        /// <returns>不带分隔符的十六进制小写字符串。如果哈希为空，则返回空字符串。</returns>
        /// <remarks>此方法在 .NET 8 下使用 <see cref="Convert.ToHexString(ReadOnlySpan{byte})"/> 实现以获得高性能。</remarks>
        public string ToHexString()
        {
            if (IsEmpty) return string.Empty;
            return Convert.ToHexString(CommittedSpan).ToLowerInvariant();
        }

        /// <summary>
        /// 将当前哈希值转换为 Base64 编码的字符串。
        /// </summary>
        /// <returns>Base64 编码字符串。如果哈希为空，则返回空字符串。</returns>
        public string ToBase64String()
        {
            if (IsEmpty) return string.Empty;
            return Convert.ToBase64String(CommittedSpan);
        }

        #endregion Convert

        /// <summary>
        /// 从十六进制字符串解析并创建 <see cref="HashValue"/>。
        /// </summary>
        /// <param name="hexString">十六进制字符串（支持大小写）。</param>
        /// <returns>解析得到的 <see cref="HashValue"/> 实例。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="hexString"/> 为 null 或空时抛出。</exception>
        /// <exception cref="FormatException">当字符串长度不是偶数或包含无效的十六进制字符时抛出。</exception>
        public static HashValue FromHexString(string hexString)
        {
            if (string.IsNullOrEmpty(hexString)) throw new ArgumentNullException(nameof(hexString));

            byte[] bytes = Convert.FromHexString(hexString);
            return new HashValue(bytes);
        }

        /// <summary>
        /// 从 Base64 编码字符串解析并创建 <see cref="HashValue"/>。
        /// </summary>
        /// <param name="base64String">Base64 编码的字符串。</param>
        /// <returns>解析得到的 <see cref="HashValue"/> 实例。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="base64String"/> 为 null 或空时抛出。</exception>
        /// <exception cref="ArgumentException">当 Base64 格式无效时抛出。</exception>
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
                throw new ArgumentException("Base64 字符串格式无效。", nameof(base64String), ex);
            }
        }

        #region Implicit

        public static implicit operator HashValue(ByteBlock block)
            => new(block);

        public static implicit operator ByteBlock(HashValue hashValue)
            => new(hashValue._block.CommittedSpan);

        #endregion Implicit
    }
}