using System.Text;
using ExtenderApp.Common.Hash;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Caches
{
    /// <summary>
    /// 缓存扩展类
    /// </summary>
    internal static class CacheExtensions
    {
        /// <summary>
        /// 添加缓存服务到IServiceCollection中
        /// </summary>
        /// <param name="services">
        /// IServiceCollection 实例
        /// </param>
        /// <returns>扩展后的IServiceCollection实例</returns>
        internal static IServiceCollection AddCache(this IServiceCollection services)
        {

            return services;
        }
    }
}