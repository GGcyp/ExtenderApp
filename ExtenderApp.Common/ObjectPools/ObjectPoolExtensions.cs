using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common
{
    internal static class ObjectPoolExtensions
    {
        public static IServiceCollection AddObjectPool(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ObjectPool<>), (p, obj) =>
            {
                var types = obj as Type[];
                if (types == null || types.Length < 0)
                    throw new ArgumentException("需要生成的对象池对象类型为空");
                if (types.Length > 1)
                    throw new ArgumentException("只能生成一个对象池对象类型");

                var type = types[0];

                var method = typeof(ObjectPool).GetMethod("CreateDefault").MakeGenericMethod(type);
                object temp = method.Invoke(null, null);
                return temp;
            });
            return services;
        }
    }
}
