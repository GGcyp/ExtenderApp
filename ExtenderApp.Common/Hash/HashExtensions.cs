using System.Security.Cryptography;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Hash;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Hash
{
    /// <summary>
    /// SHA扩展类，提供SHA相关的扩展方法。
    /// </summary>
    public static class HashExtensions
    {
        // FNV-1a算法的32位参数
        const uint FNV_offset_basis = 2166136261;
        const uint FNV_prime = 16777619;

        /// <summary>
        /// 为服务集合添加哈希服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>修改后的服务集合。</returns>
        internal static IServiceCollection AddHash(this IServiceCollection services)
        {
            services.AddSingleton<IHashProvider, HashProvider>();
            services.Configuration<IBinaryFormatterStore>(b =>
            {
                b.AddStructFormatter<HashValue, HashValueFormatter>();
                b.AddStructFormatter<HashValues, HashValuesFormatter>();
            });
            return services;
        }

        /// <summary>
        /// 计算文件的哈希值。
        /// </summary>
        /// <typeparam name="T">哈希算法的类型，需要继承自<see cref="HashAlgorithm"/>。</typeparam>
        /// <param name="hashProvider">哈希提供者实例。</param>
        /// <param name="localFileInfo">本地文件信息。</param>
        ///// <returns>计算得到的哈希值。</returns>
        //public static HashValue ComputeHash<T>(this IHashProvider hashProvider, LocalFileInfo localFileInfo) where T : HashAlgorithm
        //{
        //    return hashProvider.ComputeHash<T>(localFileInfo.CreateReadWriteOperate());
        //}

        ///// <summary>
        ///// 计算给定文件的哈希值。
        ///// </summary>
        ///// <typeparam name="T">哈希值的类型。</typeparam>
        ///// <param name="hashProvider">哈希提供者接口。</param>
        ///// <param name="localFileInfo">包含文件信息的对象。</param>
        ///// <returns>异步返回计算出的哈希值。</returns>
        //public static Task<HashValue> ComputeHashAsync<T>(this IHashProvider hashProvider, LocalFileInfo localFileInfo) where T : HashAlgorithm
        //{
        //    return hashProvider.ComputeHashAsync<T>(localFileInfo.CreateReadWriteOperate());
        //}

        /// <summary>
        /// 使用FNV-1a算法计算字符串的哈希值
        /// </summary>
        /// <param name="str">要计算哈希值的字符串</param>
        /// <returns>计算得到的哈希值，如果输入为null或空字符串则返回0</returns>
        public static int ComputeHash_FNV_1a(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;
            uint hash = FNV_offset_basis;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                // 先异或，再乘以质数
                hash ^= c;
                hash *= FNV_prime;
            }
            return (int)hash;
        }

        /// <summary>
        /// 计算给定字节序列的FNV-1a哈希值。
        /// </summary>
        /// <param name="span">要计算哈希值的字节序列。</param>
        /// <returns>计算得到的哈希值。</returns>
        public static int ComputeHash_FNV_1a(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return 0;

            uint hash = FNV_offset_basis;
            // 对每个字节应用FNV-1a算法
            for (int i = 0; i < span.Length; i++)
            {
                // 先异或，再乘以质数
                hash ^= span[i];
                hash *= FNV_prime;
            }

            // 转换为int并返回
            return (int)hash;
        }

        /// <summary>
        /// 使用MD5算法计算给定字节序列的哈希值，并返回其前4个字节的整数表示。
        /// </summary>
        /// <param name="span">包含要计算哈希值的字节序列的<see cref="ReadOnlySpan{byte}"/>。</param>
        /// <returns>返回计算出的哈希值的前4个字节的整数表示。如果输入为空或发生异常，则返回0。</returns>
        public static int ComputeHash_MD5(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return 0;

            try
            {
                byte[] hashBytes = MD5.HashData(span);
                int hash = BitConverter.ToInt32(hashBytes, 0);
                return hash;
            }
            catch (Exception)
            {
                // 处理可能的异常（如字节数组长度不足4）
                return 0;
            }
        }
    }
}
