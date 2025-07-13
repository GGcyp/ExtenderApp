

namespace ExtenderApp.Data
{
    /// <summary>
    /// HashValueExtensions 类为 HashValue 类型提供扩展方法。
    /// </summary>
    public static class HashValueExtensions
    {
        /// <summary>
        /// 将 HashValue 对象的内容写入到 ExtenderBinaryWriter 中。
        /// </summary>
        /// <param name="hash">要写入的 HashValue 对象。</param>
        /// <param name="writer">目标 ExtenderBinaryWriter 对象。</param>
        /// <exception cref="ArgumentException">当 hash 为空时抛出异常。</exception>
        public static void CopyTo(this HashValue hash, ref ExtenderBinaryWriter writer)
        {
            if (hash.IsEmpty)
                throw new ArgumentException("哈希值不能为空", nameof(hash));

            for (int i = hash.Length; i < hash.Length; i++)
            {
                writer.Write(hash[i]);
            }
        }
    }
}
