using System.Security.Cryptography;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Hash;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// SHA扩展类，提供SHA相关的扩展方法。
    /// </summary>
    public static class HashExtensions
    {
        /// <summary>
        /// 为服务集合添加哈希服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>修改后的服务集合。</returns>
        internal static IServiceCollection AddHash(this IServiceCollection services)
        {
            services.AddSingleton<IHashProvider, HashProvider>();
            services.Configuration<BinaryFormatterStore>(b =>
            {
                b.AddStructFormatter<HashValue, HashValueFormatter>();
            });
            return services;
        }

        /// <summary>
        /// 计算文件的哈希值。
        /// </summary>
        /// <typeparam name="T">哈希算法的类型，需要继承自<see cref="HashAlgorithm"/>。</typeparam>
        /// <param name="hashProvider">哈希提供者实例。</param>
        /// <param name="localFileInfo">本地文件信息。</param>
        /// <returns>计算得到的哈希值。</returns>
        public static HashValue ComputeHash<T>(this IHashProvider hashProvider, LocalFileInfo localFileInfo) where T : HashAlgorithm
        {
            return hashProvider.ComputeHash<T>(localFileInfo.CreateReadWriteOperate());
        }

        /// <summary>
        /// 计算给定文件的哈希值。
        /// </summary>
        /// <typeparam name="T">哈希值的类型。</typeparam>
        /// <param name="hashProvider">哈希提供者接口。</param>
        /// <param name="localFileInfo">包含文件信息的对象。</param>
        /// <returns>异步返回计算出的哈希值。</returns>
        public static HashValue ComputeHashAsync<T>(this IHashProvider hashProvider, LocalFileInfo localFileInfo) where T : HashAlgorithm
        {
            return hashProvider.ComputeHashAsync<T>(localFileInfo.CreateReadWriteOperate());
        }
    }
}
