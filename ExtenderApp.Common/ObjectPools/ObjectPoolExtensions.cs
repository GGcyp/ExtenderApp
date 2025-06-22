using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;

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
            services.AddSingleton(typeof(ObjectPool<>), CreateObjectPool);
            services.AddSingleton(typeof(IObjectPool<>), CreateObjectPool);
            services.AddSingleton(ObjectPool.PoolStore);
            return services;
        }

        /// <summary>
        /// 创建对象池实例
        /// </summary>
        /// <param name="provider">服务提供者</param>
        /// <param name="obj">对象类型数组</param>
        /// <returns>对象池实例</returns>
        /// <exception cref="ArgumentException">如果对象类型数组为空或包含多个类型时抛出</exception>
        private static object? CreateObjectPool(IServiceProvider provider, object? obj)
        {
            var types = obj as Type[];
            if (types == null || types.Length < 0)
                throw new ArgumentException("需要生成的对象池对象类型为空");
            if (types.Length > 1)
                throw new ArgumentException("只能生成一个对象池对象类型");

            var type = types[0];

            return typeof(ObjectPool).GetMethod(nameof(ObjectPool.CreateDefaultPool)).MakeGenericMethod(type).Invoke(null, null);
        }
    }
}
