
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 对象池扩展类
    /// </summary>
    internal static class ObjectPoolExtensions
    {
        /// <summary>
        /// 向服务集合中添加对象池服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddObjectPool(this IServiceCollection services)
        {
            services.AddSingleton(ObjectPool.PoolStore);
            return services;
        }
    }
}
