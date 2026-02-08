using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Hash
{
    /// <summary>
    /// 哈希扩展类，提供基于 FNV-1a、MD5、SHA 系列及 HMAC 算法的哈希计算扩展方法。
    /// </summary>
    public static class HashExtensions
    {
        /// <summary>
        /// FNV-1a 算法的 32 位初始偏移基准值。
        /// </summary>
        private const uint FNV_offset_basis = 2166136261;

        /// <summary>
        /// FNV-1a 算法的 32 位质数因子。
        /// </summary>
        private const uint FNV_prime = 16777619;

        /// <summary>
        /// 默认哈希计算使用的字符编码（UTF-8）。
        /// </summary>
        private static Encoding HashEncoding = Encoding.UTF8;

        /// <summary>
        /// 为服务集合添加哈希服务及相关的二进制格式化器。
        /// </summary>
        /// <param name="services">要配置的服务集合。</param>
        /// <returns>返回修改后的服务集合以支持链式调用。</returns>
        internal static IServiceCollection AddHash(this IServiceCollection services)
        {
            services.AddSingleton<IHashProvider, HashProvider>();
            services.ConfigureSingletonInstance<IBinaryFormatterStore>(b =>
            {
                b.AddStructFormatter<HashValue, HashValueFormatter>();
            });
            return services;
        }

        /// <summary>
        /// 使用 FNV-1a 算法计算指定类型的哈希值（基于类型的名称）。
        /// </summary>
        /// <param name="type">需要计算哈希值的类型。</param>
        /// <param name="hash">初始哈希偏移值（默认使用 FNV_offset_basis）。</param>
        /// <param name="prime">FNV 质数因子（默认使用 FNV_prime）。</param>
        /// <returns>计算出来的类型名称哈希值。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="type"/> 为 null 时抛出。</exception>
        public static int ComputeHash_FNV_1a(this Type type, uint hash = FNV_offset_basis, uint prime = FNV_prime)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            string NameOrFullName = type.FullName ?? type.Name;
            return NameOrFullName.ComputeHash_FNV_1a(hash, prime);
        }

        /// <summary>
        /// 使用 FNV-1a 算法计算字符串的哈希值。
        /// 该方法通过 <see cref="ReadOnlySpan{T}"/> 和 <see cref="Unsafe"/> 访问跳过边界检查以优化密集计算性能。
        /// </summary>
        /// <param name="str">要计算哈希值的字符串。</param>
        /// <param name="hash">初始哈希偏移值（默认使用 FNV_offset_basis）。</param>
        /// <param name="prime">FNV 质数因子（默认使用 FNV_prime）。</param>
        /// <returns>计算得到的哈希值。如果输入字符串为 null 或空，则返回 0。</returns>
        public static int ComputeHash_FNV_1a(this string str, uint hash = FNV_offset_basis, uint prime = FNV_prime)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            ReadOnlySpan<char> span = str.AsSpan();
            ref char p = ref MemoryMarshal.GetReference(span);

            for (int i = 0; i < span.Length; i++)
            {
                hash ^= Unsafe.Add(ref p, i);
                hash *= prime;
            }
            return (int)hash;
        }

        /// <summary>
        /// 计算给定字节序列的 FNV-1a 哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字节序列。</param>
        /// <param name="hash">初始哈希偏移值（默认使用 FNV_offset_basis）。</param>
        /// <param name="prime">FNV 质数因子（默认使用 FNV_prime）。</param>
        /// <returns>计算得到的哈希值。</returns>
        public static int ComputeHash_FNV_1a(this ReadOnlySpan<byte> span, uint hash = FNV_offset_basis, uint prime = FNV_prime)
        {
            if (span.IsEmpty)
                return 0;

            ref byte p = ref MemoryMarshal.GetReference(span);

            for (int i = 0; i < span.Length; i++)
            {
                hash ^= Unsafe.Add(ref p, i);
                hash *= prime;
            }

            return (int)hash;
        }

        /// <summary>
        /// 使用 SHA1 算法计算指定字符串（默认 UTF-8 编码）的哈希值。
        /// </summary>
        /// <param name="str">要计算哈希值的字符串。</param>
        /// <param name="encoding">字符编码方式。为 null 时使用默认编码。</param>
        /// <returns>返回包含 SHA1 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_SHA1(this string str, Encoding? encoding = null)
        {
            return str.AsSpan().ComputeHash_SHA1(encoding);
        }

        /// <summary>
        /// 使用 SHA1 算法计算指定字符序列（默认 UTF-8 编码）的哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字符序列。</param>
        /// <param name="encoding">字符编码方式。为 null 时使用默认编码。</param>
        /// <returns>返回包含 SHA1 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_SHA1(this ReadOnlySpan<char> span, Encoding? encoding = null)
        {
            encoding ??= HashEncoding;
            int length = encoding.GetMaxByteCount(span.Length);
            using ByteBlock block = new(length);
            length = encoding.GetBytes(span, block.GetSpan(length));
            block.Advance(length);
            var result = block.CommittedSpan.ComputeHash_SHA1();
            return result;
        }

        /// <summary>
        /// 使用 SHA1 算法计算给定字节序列的哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字节序列。</param>
        /// <returns>返回包含 SHA1 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_SHA1(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty) return new();
            try
            {
                int length = SHA1.HashSizeInBytes;
                ByteBlock block = new(length);
                Span<byte> hashBytes = stackalloc byte[length];
                SHA1.HashData(span, hashBytes);
                block.Write(hashBytes);
                return block;
            }
            catch { return new(); }
        }

        /// <summary>
        /// 使用 SHA256 算法计算指定字符串（默认 UTF-8 编码）的哈希值。
        /// </summary>
        /// <param name="str">要计算哈希值的字符串。</param>
        /// <param name="encoding">字符编码方式。为 null 时使用默认编码。</param>
        /// <returns>返回包含 SHA256 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_SHA256(this string str, Encoding? encoding = null)
        {
            return str.AsSpan().ComputeHash_SHA256(encoding);
        }

        /// <summary>
        /// 使用 SHA256 算法计算指定字符序列（默认 UTF-8 编码）的哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字符序列。</param>
        /// <param name="encoding">字符编码方式。为 null 时使用默认编码。</param>
        /// <returns>返回包含 SHA256 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_SHA256(this ReadOnlySpan<char> span, Encoding? encoding = null)
        {
            encoding ??= HashEncoding;
            int length = encoding.GetMaxByteCount(span.Length);
            using ByteBlock block = new(length);
            length = encoding.GetBytes(span, block.GetSpan(length));
            block.Advance(length);
            var result = block.CommittedSpan.ComputeHash_SHA256();
            return result;
        }

        /// <summary>
        /// 使用 SHA256 算法计算给定字节序列的哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字节序列。</param>
        /// <returns>返回包含 SHA256 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_SHA256(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty) return new();
            try
            {
                int length = SHA256.HashSizeInBytes;
                ByteBlock block = new(length);
                Span<byte> hashBytes = stackalloc byte[length];
                SHA256.HashData(span, hashBytes);
                block.Write(hashBytes);
                return block;
            }
            catch { return new(); }
        }

        /// <summary>
        /// 使用 SHA384 算法计算指定字符串（默认 UTF-8 编码）的哈希值。
        /// </summary>
        /// <param name="str">要计算哈希值的字符串。</param>
        /// <param name="encoding">字符编码方式。为 null 时使用默认编码。</param>
        /// <returns>返回包含 SHA384 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_SHA384(this string str, Encoding? encoding = null)
        {
            return str.AsSpan().ComputeHash_SHA384(encoding);
        }

        /// <summary>
        /// 使用 SHA384 算法计算指定字符序列（默认 UTF-8 编码）的哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字符序列。</param>
        /// <param name="encoding">字符编码方式。为 null 时使用默认编码。</param>
        /// <returns>返回包含 SHA384 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_SHA384(this ReadOnlySpan<char> span, Encoding? encoding = null)
        {
            encoding ??= HashEncoding;
            int length = encoding.GetMaxByteCount(span.Length);
            using ByteBlock block = new(length);
            length = encoding.GetBytes(span, block.GetSpan(length));
            block.Advance(length);
            var result = block.CommittedSpan.ComputeHash_SHA384();
            return result;
        }

        /// <summary>
        /// 使用 SHA384 算法计算给定字节序列的哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字节序列。</param>
        /// <returns>返回包含 SHA384 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_SHA384(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty) return new();
            try
            {
                int length = SHA384.HashSizeInBytes;
                ByteBlock block = new(length);
                Span<byte> hashBytes = stackalloc byte[length];
                SHA384.HashData(span, hashBytes);
                block.Write(hashBytes);
                return block;
            }
            catch { return new(); }
        }

        /// <summary>
        /// 使用 MD5 算法计算指定字符串（默认 UTF-8 编码）的哈希值。
        /// </summary>
        /// <param name="str">要计算哈希值的字符串。</param>
        /// <param name="encoding">字符编码方式。为 null 时使用默认编码。</param>
        /// <returns>返回包含 MD5 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_MD5(this string str, Encoding? encoding = null)
        {
            return str.AsSpan().ComputeHash_MD5(encoding);
        }

        /// <summary>
        /// 使用 MD5 算法计算指定字符序列（默认 UTF-8 编码）的哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字符序列。</param>
        /// <param name="encoding">字符编码方式。为 null 时使用默认编码。</param>
        /// <returns>返回包含 MD5 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_MD5(this ReadOnlySpan<char> span, Encoding? encoding = null)
        {
            encoding ??= HashEncoding;
            int length = encoding.GetMaxByteCount(span.Length);
            using ByteBlock block = new(length);
            length = encoding.GetBytes(span, block.GetSpan(length));
            block.Advance(length);
            var result = block.CommittedSpan.ComputeHash_MD5();
            return result;
        }

        /// <summary>
        /// 使用 MD5 算法计算给定字节序列的哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字节序列。</param>
        /// <returns>返回包含 MD5 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_MD5(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty) return new();
            try
            {
                int length = MD5.HashSizeInBytes;
                ByteBlock block = new(length);
                Span<byte> hashBytes = stackalloc byte[length];
                MD5.HashData(span, hashBytes);
                block.Write(hashBytes);
                return block;
            }
            catch { return new(); }
        }

        /// <summary>
        /// 使用 HMACMD5 算法计算指定字符串（默认 UTF-8 编码）的哈希值。
        /// </summary>
        /// <param name="str">要计算哈希值的字符串。</param>
        /// <param name="encoding">字符编码方式。为 null 时使用默认编码。</param>
        /// <returns>返回包含 HMACMD5 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_HMACMD5(this string str, Encoding? encoding = null)
        {
            return str.AsSpan().ComputeHash_HMACMD5(encoding);
        }

        /// <summary>
        /// 使用 HMACMD5 算法计算指定字符序列（默认 UTF-8 编码）的哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字符序列。</param>
        /// <param name="encoding">字符编码方式。为 null 时使用默认编码。</param>
        /// <returns>返回包含 HMACMD5 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_HMACMD5(this ReadOnlySpan<char> span, Encoding? encoding = null)
        {
            encoding ??= HashEncoding;
            int length = encoding.GetMaxByteCount(span.Length);
            using ByteBlock block = new(length);
            length = encoding.GetBytes(span, block.GetSpan(length));
            block.Advance(length);
            var result = block.CommittedSpan.ComputeHash_HMACMD5();
            return result;
        }

        /// <summary>
        /// 使用 HMACMD5 算法计算给定字节序列的哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的只读字节序列。</param>
        /// <returns>返回包含 HMACMD5 哈希结果的 <see cref="ByteBlock"/>。</returns>
        public static ByteBlock ComputeHash_HMACMD5(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty) return new();
            try
            {
                int length = HMACMD5.HashSizeInBytes;
                ByteBlock block = new(length);
                Span<byte> hashBytes = stackalloc byte[length];
                HMACMD5.HashData(span, hashBytes);
                block.Write(hashBytes);
                return block;
            }
            catch { return new(); }
        }
    }
}