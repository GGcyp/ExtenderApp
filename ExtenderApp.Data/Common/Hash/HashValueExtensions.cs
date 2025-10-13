

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
        /// <param name="buffer">目标缓存对象。</param>
        /// <exception cref="ArgumentException">当 hash 为空或缓存不能写入时抛出异常。</exception>
        public static void CopyTo(this HashValue hash, ref ByteBuffer buffer)
        {
            if (hash.IsEmpty)
                throw new ArgumentException("哈希值不能为空", nameof(hash));
            if (!buffer.CanWrite)
                throw new ArgumentException("字节缓存需要能写入");

            int length = hash.Length;
            hash.CopyTo(buffer.GetSpan(length));
            buffer.WriteAdvance(length);
        }

        /// <summary>
        /// 将 HashValue 对象的内容写入到 ByteBlock 中。
        /// </summary>
        /// <param name="hash">要写入的 HashValue 对象。</param>
        /// <param name="block">目标缓存对象。</param>
        /// <exception cref="ArgumentException">当 hash 为空或缓存为空时抛出异常。</exception>
        public static void CopyTo(this HashValue hash, ref ByteBlock block)
        {
            if (hash.IsEmpty)
                throw new ArgumentException("哈希值不能为空", nameof(hash));
            if (block.IsEmpty)
                throw new ArgumentException("字节缓存块为空");

            int length = hash.Length;
            hash.CopyTo(block.GetSpan(length));
            block.WriteAdvance(length);
        }
    }
}
