namespace ExtenderApp.Common
{
    /// <summary>
    /// 提供一些实用的静态方法。
    /// </summary>
    public static partial class Utility
    {
        /// <summary>
        /// 用于计算哈希值的一个质数常数。
        /// </summary>
        private const int PRIME = 31;

        /// <summary>
        /// 用于将字节转换为十六进制字符的查找表。
        /// </summary>
        private static readonly char[] HexLookup =
            { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// 获取类型T的简单一致性哈希值。
        /// </summary>
        /// <typeparam name="T">要获取哈希值的类型。</typeparam>
        /// <param name="hash">输出的哈希值。</param>
        public static void GetSimpleConsistentHash<T>(out int hash)
        {
            Type type = typeof(T);
            string typeName = type.FullName ?? type.Name;
            GetSimpleConsistentHash(typeName, out hash);
        }

        /// <summary>
        /// 获取字符串的简单一致性哈希值。
        /// </summary>
        /// <param name="input">要计算哈希值的字符串。</param>
        /// <param name="hash">输出的哈希值。</param>
        public static void GetSimpleConsistentHash(string input, out int hash)
        {
            hash = 0;
            if (string.IsNullOrEmpty(input))
                return;

            hash = 0;
            foreach (char c in input)
            {
                hash = (hash * PRIME) + c;
                hash &= 0x7FFFFFFF; // 保持正值
            }
        }

        /// <summary>
        /// 获取类型T的简单一致性哈希值（长整型）。
        /// </summary>
        /// <typeparam name="T">要获取哈希值的类型。</typeparam>
        /// <param name="hash">输出的哈希值。</param>
        public static void GetSimpleConsistentHash<T>(out long hash)
        {
            Type type = typeof(T);
            string typeName = type.FullName ?? type.Name;
            GetSimpleConsistentHash(typeName, out hash);
        }

        /// <summary>
        /// 获取字符串的简单一致性哈希值（长整型）。
        /// </summary>
        /// <param name="input">要计算哈希值的字符串。</param>
        /// <param name="hash">输出的哈希值。</param>
        public static void GetSimpleConsistentHash(string input, out long hash)
        {
            hash = 0;
            if (string.IsNullOrEmpty(input))
                return;

            hash = 0;
            foreach (char c in input)
            {
                hash = (hash * PRIME) + c;
                hash &= 0x7FFFFFFF; // 保持正值
            }
        }

        /// <summary>
        /// 将字节序列转换为十六进制字符串。
        /// </summary>
        /// <param name="span">要转换的字节序列。</param>
        /// <returns>转换后的十六进制字符串。</returns>
        public static string BytesToHex(ReadOnlySpan<byte> span)
        {
            char[] chars = new char[span.Length * 2];
            int b;
            for (int i = 0; i < span.Length; i++)
            {
                b = span[i] >> 4;
                chars[i * 2] = HexLookup[b];
                b = span[i] & 0xF;
                chars[i * 2 + 1] = HexLookup[b];
            }
            return new string(chars);
        }
    }
}
