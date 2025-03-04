using System.Reflection;

namespace AppHost.Extensions.DependencyInjection
{
    public static class ServiceCollectionBuilderExtensions
    {
        private static readonly object[] _objects = new object[1];

        /// <summary>
        /// 用<see cref="IServiceCollection"/>创建一个服务提供器<see cref="ServiceProvider"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns><see cref="ServiceProvider"/></returns>
        public static IServiceProvider BuildServiceProvider(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddIEnumerable();
            services.AddList();

            return new ServiceProvider(services);
        }

        /// <summary>
        /// 向IServiceCollection中添加IEnumerable类型的服务，该服务是一个包含所有可分配给指定元素类型的服务实例的列表。
        /// </summary>
        /// <param name="services">IServiceCollection实例，用于注册服务。</param>
        /// <returns>IServiceCollection的实例，用于链式注册服务。</returns>
        public static IServiceCollection AddIEnumerable(this IServiceCollection services)
        {
            services.AddTransient(typeof(IEnumerable<>), (p, obj) =>
            {
                var types = obj as Type[];
                //ArgumentNullException.ThrowIfNull(types, "type");
                if (types is null) return default;

                Type elementType = types[0];

                // 使用反射创建 List<T> 的类型
                Type listType = typeof(List<>).MakeGenericType(elementType);
                // 使用反射创建 List<T> 的实例
                object listInstance = Activator.CreateInstance(listType);
                //向 List<T> 添加元素（使用反射调用 Add 方法）
                MethodInfo addMethod = listType.GetMethod("Add");


                var collection = p.GetRequiredService<IServiceCollection>();


                foreach (var item in collection)
                {
                    if (elementType.IsAssignableFrom(item.ServiceType))
                    {
                        _objects[0] = p.GetRequiredService(item.ServiceType);
                        addMethod!.Invoke(listInstance, _objects);
                    }
                }

                return listInstance;
            });

            return services;
        }

        /// <summary>
        /// 向IServiceCollection中添加IList类型的服务，该服务是一个包含所有可分配给指定元素类型的服务实例的列表。
        /// </summary>
        /// <param name="services">IServiceCollection实例，用于注册服务。</param>
        /// <returns>IServiceCollection的实例，用于链式注册服务。</returns>
        public static IServiceCollection AddList(this IServiceCollection services)
        {
            services.AddTransient(typeof(List<>), (p, obj) =>
            {
                var types = obj as Type[];
                //ArgumentNullException.ThrowIfNull(types, "type");
                if (types is null) return default;

                Type elementType = types[0];

                Type listType = typeof(List<>).MakeGenericType(elementType);
                // 使用反射创建 List<T> 的实例
                object listInstance = Activator.CreateInstance(listType);
                //向 List<T> 添加元素（使用反射调用 Add 方法）
                MethodInfo addMethod = listType.GetMethod("Add");

                var collection = p.GetRequiredService<IServiceCollection>();


                foreach (var item in collection)
                {
                    if (elementType.IsAssignableFrom(item.ServiceType))
                    {
                        _objects[0] = p.GetRequiredService(item.ServiceType);
                        addMethod!.Invoke(listInstance, _objects);
                    }
                }

                return listInstance;
            });

            return services;
        }
    }
}
