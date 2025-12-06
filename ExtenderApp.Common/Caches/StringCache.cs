using System.Text;
using ExtenderApp.Common.Hash;

namespace ExtenderApp.Common.Caches
{
    /// <summary> 字符串缓存类，继承自 EvictionCache<int, string> 类 </summary>
    public class StringCache : EvictionCache<int, string>
    {
        /// <summary>
        /// 通过字节数组获取字符串
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="encoding">编码方式，默认为 null</param>
        /// <returns>字符串</returns>
        public string GetString(byte[] bytes, Encoding? encoding = null)
        {
            return GetString(bytes.AsSpan(), encoding);
        }

        /// <summary>
        /// 通过字节数组的只读跨度获取字符串
        /// </summary>
        /// <param name="span">字节数组的只读跨度</param>
        /// <param name="encoding">编码方式，默认为 null</param>
        /// <returns>字符串</returns>
        public string GetString(ReadOnlySpan<byte> span, Encoding? encoding = null)
        {
            if (span.IsEmpty)
                return string.Empty;

            encoding = encoding ?? Encoding.UTF8;
            int hash = span.ComputeHash_FNV_1a();
            if (TryGet(hash, out var result))
                return result;

            lock (this)
            {
                if (TryGet(hash, out result))
                    return result;

                result = encoding.GetString(span);
                AddOrUpdate(hash, result);
            }
            return result;
        }
    }
}