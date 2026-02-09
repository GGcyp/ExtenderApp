using System.Text;
using ExtenderApp.Common.Hash;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 提供将字符序列转换为对应哈希值封装 (<see cref="HashValue"/>) 的扩展方法。
    /// </summary>
    public static class HashValueExtensions
    {
        /// <summary>
        /// 使用 SHA1 算法计算字符序列的哈希值。
        /// </summary>
        /// <param name="value">要计算哈希的只读字符序列。</param>
        /// <param name="encoding">字符编码方式。若为 <see langword="null"/> 则使用默认编码。</param>
        /// <returns>计算得到的 <see cref="HashValue"/> 对象。</returns>
        public static HashValue GetHashValue_SHA1(this ReadOnlySpan<char> value, Encoding? encoding = null)
        {
            var block = value.ComputeHash_SHA1(encoding);
            HashValue hash = new(block);
            block.Dispose();
            return hash;
        }

        /// <summary>
        /// 使用 SHA256 算法计算字符序列的哈希值。
        /// </summary>
        /// <param name="value">要计算哈希的只读字符序列。</param>
        /// <param name="encoding">字符编码方式。若为 <see langword="null"/> 则使用默认编码。</param>
        /// <returns>计算得到的 <see cref="HashValue"/> 对象。</returns>
        public static HashValue GetHashValue_SHA256(this ReadOnlySpan<char> value, Encoding? encoding = null)
        {
            var block = value.ComputeHash_SHA256(encoding);
            HashValue hash = new(block);
            block.Dispose();
            return hash;
        }

        /// <summary>
        /// 使用 SHA384 算法计算字符序列的哈希值。
        /// </summary>
        /// <param name="value">要计算哈希的只读字符序列。</param>
        /// <param name="encoding">字符编码方式。若为 <see langword="null"/> 则使用默认编码。</param>
        /// <returns>计算得到的 <see cref="HashValue"/> 对象。</returns>
        public static HashValue GetHashValue_SHA384(this ReadOnlySpan<char> value, Encoding? encoding = null)
        {
            var block = value.ComputeHash_SHA384(encoding);
            HashValue hash = new(block);
            block.Dispose();
            return hash;
        }

        /// <summary>
        /// 使用 MD5 算法计算字符序列的哈希值。
        /// </summary>
        /// <param name="value">要计算哈希的只读字符序列。</param>
        /// <param name="encoding">字符编码方式。若为 <see langword="null"/> 则使用默认编码。</param>
        /// <returns>计算得到的 <see cref="HashValue"/> 对象。</returns>
        public static HashValue GetHashValue_MD5(this ReadOnlySpan<char> value, Encoding? encoding = null)
        {
            var block = value.ComputeHash_MD5(encoding);
            HashValue hash = new(block);
            block.Dispose();
            return hash;
        }

        /// <summary>
        /// 使用 HMACMD5 算法计算字符序列的哈希值。
        /// </summary>
        /// <param name="value">要计算哈希的只读字符序列。</param>
        /// <param name="encoding">字符编码方式。若为 <see langword="null"/> 则使用默认编码。</param>
        /// <returns>计算得到的 <see cref="HashValue"/> 对象。</returns>
        public static HashValue GetHashValue_HMACMD5(this ReadOnlySpan<char> value, Encoding? encoding = null)
        {
            var block = value.ComputeHash_HMACMD5(encoding);
            HashValue hash = new(block);
            block.Dispose();
            return hash;
        }
    }
}