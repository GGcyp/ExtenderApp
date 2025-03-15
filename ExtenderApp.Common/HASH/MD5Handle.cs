using System.Security.Cryptography;
using System.Text;

namespace ExtenderApp.Common
{
    /// <summary>
    /// MD5处理工具类
    /// </summary>
    public static class MD5Handle
    {
        /// <summary>
        /// 延迟初始化的MD5实例
        /// </summary>
        private readonly static Lazy<MD5> _md5Lazy = new Lazy<MD5>(() => MD5.Create());

        /// <summary>
        /// 获取MD5实例
        /// </summary>
        public static MD5 MD5 => _md5Lazy.Value;

        /// <summary>
        /// 延迟初始化的StringBuilder实例
        /// </summary>
        private readonly static Lazy<StringBuilder> _stringBuilderLazy = new Lazy<StringBuilder>(() => new StringBuilder(32));

        /// <summary>
        /// 获取StringBuilder实例
        /// </summary>
        private static StringBuilder stringBuilder => _stringBuilderLazy.Value;

        /// <summary>
        /// 线程同步锁对象
        /// </summary>
        private static object _lock = new object();

        /// <summary>
        /// MD5哈希字节长度
        /// </summary>
        private const int BYTES_LENGTH = 16;

        /// <summary>
        /// 获取输入字符串的MD5哈希值。
        /// </summary>
        /// <param name="input">输入字符串。</param>
        /// <returns>输入字符串的MD5哈希值。</returns>
        public static string GetMD5Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return GetMD5Hash(bytes);
        }

        /// <summary>
        /// 获取输入字节数组的MD5哈希值。
        /// </summary>
        /// <param name="input">输入字节数组。</param>
        /// <returns>输入字节数组的MD5哈希值。</returns>
        public static string GetMD5Hash(byte[] input)
        {
            if (input == null || input.Length == 0)
            {
                return string.Empty;
            }

            return GetMD5Hash(input, 0, input.Length);
        }

        /// <summary>
        /// 获取输入字节数组指定部分的MD5哈希值。
        /// </summary>
        /// <param name="input">输入字节数组。</param>
        /// <param name="offset">起始偏移量。</param>
        /// <param name="count">要计算哈希的字节数。</param>
        /// <returns>输入字节数组指定部分的MD5哈希值。</returns>
        public static string GetMD5Hash(byte[] input, int offset, int count)
        {
            if (input == null || input.Length == 0)
            {
                return string.Empty;
            }

            var result = string.Empty;
            lock (_lock)
            {
                Span<byte> bytes = stackalloc byte[BYTES_LENGTH];
                MD5.HashData(new ReadOnlySpan<byte>(input, offset, count), bytes);
                for (int i = 0; i < BYTES_LENGTH; i++)
                {
                    stringBuilder.Append(bytes[i].ToString("x2"));
                }
                result = stringBuilder.ToString();
                stringBuilder.Clear();
            }
            return result;
        }

        /// <summary>
        /// 获取输入流的MD5哈希值。
        /// </summary>
        /// <param name="stream">输入流。</param>
        /// <returns>输入流的MD5哈希值。</returns>
        public static string GetMD5Hash(Stream stream)
        {
            if (stream == null || stream.Length == 0)
            {
                return string.Empty;
            }

            var result = string.Empty;
            lock (_lock)
            {
                Span<byte> bytes = stackalloc byte[BYTES_LENGTH];
                MD5.HashData(stream, bytes);
                for (int i = 0; i < BYTES_LENGTH; i++)
                {
                    stringBuilder.Append(bytes[i].ToString("x2"));
                }
                result = stringBuilder.ToString();
                stringBuilder.Clear();
            }
            return result;
        }
    }
}
