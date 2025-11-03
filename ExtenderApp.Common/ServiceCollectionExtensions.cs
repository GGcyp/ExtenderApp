using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 配置已注册的单例服务实例。
        /// 如果该服务已经以 ImplementationInstance 注册，则直接调用 configure。
        /// 如果以 ImplementationType 或 ImplementationFactory 注册，并且能够安全创建实例，则会创建实例、调用 configure，并用实例替换原有注册。
        /// 注意：此方法在需要临时构建 IServiceProvider 时会触发构建（开销较大，且可能有副作用），仅在确实需要时使用。
        /// </summary>
        public static IServiceCollection ConfigureSingletonInstance<T>(this IServiceCollection services, Action<T> configure)
            where T : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var serviceType = typeof(T);
            // 找最后注册的那个（与 DI 行为一致）
            var descriptor = services.LastOrDefault(sd => sd.ServiceType == serviceType);
            if (descriptor == null)
                throw new InvalidOperationException($"服务类型 {serviceType.FullName} 尚未注册。");

            if (descriptor.Lifetime != ServiceLifetime.Singleton)
                throw new InvalidOperationException("仅支持对单例服务进行配置。");

            // 已有实例，直接配置
            if (descriptor.ImplementationInstance is T instance)
            {
                configure(instance);
                return services;
            }

            // 如果能够直接通过 ImplementationType 创建（无参构造）
            if (descriptor.ImplementationType == null)
                throw new InvalidOperationException($"服务类型 {serviceType.FullName} 的注册方式不支持配置实例。");

            var implType = descriptor.ImplementationType;
            var ctor = implType.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                throw new InvalidOperationException($"服务类型 {serviceType.FullName} 的实现类型 {implType.FullName} 不包含无参构造函数，无法创建实例进行配置。");

            var created = (T?)Activator.CreateInstance(implType);
            if (created == null)
                throw new InvalidOperationException($"无法创建类型 {implType.FullName} 的实例。");

            configure(created);

            // 用实例替换原有注册
            services.Remove(descriptor);
            services.Add(new ServiceDescriptor(serviceType, created));
            return services;
        }

        /// <summary>
        /// 直接替换/设置单例实例（更明确、安全、开销小）。
        /// </summary>
        public static IServiceCollection ReplaceSingletonInstance<T>(this IServiceCollection services, T instance)
            where T : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var serviceType = typeof(T);
            var descriptor = services.LastOrDefault(sd => sd.ServiceType == serviceType);
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.Add(new ServiceDescriptor(serviceType, instance));
            return services;
        }
    }
}
