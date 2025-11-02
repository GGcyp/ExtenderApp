using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 这个静态类提供了一系列用于扩展 <see
    /// cref="IServiceCollection"/> 接口的扩展方法， 旨在帮助开发者更便捷、灵活地向依赖注入容器中注册不同类型、不同生命周期以及各种配置的服务，以满足多样化的应用开发需求。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        #region Add

        /// <summary>
        /// 将一个服务添加到服务集合中，并注册为单例模式。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="serviceType">要注册的服务类型。</param>
        /// <param name="factory">用于创建服务实例的工厂方法。</param>
        /// <returns>返回添加服务后的服务集合。</returns>
        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object?, object> factory)
        {
            services.Add(serviceType, factory, ServiceLifetime.Singleton);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/>
        /// 中注册一个指定类型 <paramref
        /// name="serviceType"/>
        /// 的瞬时（Transient）服务，通过给定的工厂函数 <paramref
        /// name="factory"/> 来创建服务实例。 每次请求该服务时，都会调用工厂函数创建一个新的实例返回（实现了瞬时创建新实例的功能）。
        /// </summary>
        /// <param name="services">
        /// <see cref="IServiceCollection"/> 实例，用于添加服务注册信息。
        /// </param>
        /// <param name="serviceType">需要注册的服务类型。</param>
        /// <param name="factory">
        /// 创建服务实例的工厂函数，接收 <see
        /// cref="IServiceProvider"/> 并返回服务实例对象。
        /// </param>
        /// <returns>
        /// 添加服务后的 <see
        /// cref="IServiceCollection"/> 实例，便于链式调用其他扩展方法继续添加服务注册。
        /// </returns>
        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object?, object> factory)
        {
            services.Add(serviceType, factory, ServiceLifetime.Transient);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/>
        /// 中注册一个指定类型 <typeparamref
        /// name="TService"/>
        /// 的瞬时（Transient）服务，通过给定的工厂函数 <paramref
        /// name="factory"/> 来创建服务实例。 每次请求该服务时，都会调用工厂函数创建一个新的实例返回（符合瞬时服务每次获取都是新实例的特点）。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类。</typeparam>
        /// <param name="services">
        /// <see cref="IServiceCollection"/> 实例，用于添加服务注册信息。
        /// </param>
        /// <param name="factory">
        /// 创建服务实例的工厂函数，接收 <see
        /// cref="IServiceProvider"/> 并返回服务实例对象。
        /// </param>
        /// <returns>
        /// 添加服务后的 <see
        /// cref="IServiceCollection"/> 实例，便于后续继续添加服务注册。
        /// </returns>
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, object> factory)
        {
            services.Add(typeof(TService), factory, ServiceLifetime.Transient);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/>
        /// 中添加一个服务注册信息，通过给定的服务类型 <paramref
        /// name="serviceType"/>、工厂函数 <paramref
        /// name="factory"/> 以及服务生命周期 <paramref
        /// name="lifetime"/> 来注册服务。
        /// 工厂函数用于创建服务实例，服务生命周期决定了实例的创建和复用策略（如单例、瞬时、作用域）， 此方法提供了一种通用的方式来注册服务，根据传入的不同参数可以灵活配置服务的创建和管理方式。
        /// </summary>
        /// <param name="services">
        /// <see cref="IServiceCollection"/> 实例，用于添加服务注册信息。
        /// </param>
        /// <param name="serviceType">需要注册的服务类型，一般是接口或者抽象类等用于定义服务契约，也可以是具体的类类型。</param>
        /// <param name="factory">
        /// 创建服务实例的工厂函数，接收 <see
        /// cref="IServiceProvider"/>
        /// 和可选对象参数，返回服务实例对象， 通过工厂函数可以实现复杂的实例创建逻辑，依赖于容器内其他服务或者进行特定的初始化操作等。
        /// </param>
        /// <param name="lifetime">
        /// 服务的生命周期，指定实例的创建和复用规则，如 <see
        /// cref="ServiceLifetime.Singleton"/>（单例）、
        /// <see
        /// cref="ServiceLifetime.Transient"/>（瞬时）、
        /// <see cref="ServiceLifetime.Scoped"/>（作用域）等不同策略。
        /// </param>
        /// <returns>
        /// 添加服务后的 <see
        /// cref="IServiceCollection"/>
        /// 实例，方便继续添加其他服务注册， 例如可以接着注册更多不同类型、不同生命周期或者不同配置的服务到依赖注入容器中。
        /// </returns>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object?, object> factory, ServiceLifetime lifetime)
        {
            services.Add(ServiceDescriptorFactory.Create(serviceType, factory, lifetime));
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/>
        /// 中添加一个服务注册信息，通过给定的服务类型 <paramref
        /// name="serviceType"/>、工厂函数 <paramref
        /// name="factory"/> 以及服务生命周期 <paramref
        /// name="lifetime"/> 来注册服务。
        /// 工厂函数用于创建服务实例，服务生命周期决定了实例的创建和复用策略（比如单例、瞬时、作用域等情况）， 提供了一种相对简洁的方式来注册服务，适用于不需要服务键区分不同配置的常见服务注册场景。
        /// </summary>
        /// <param name="services">
        /// <see cref="IServiceCollection"/> 实例，用于添加服务注册信息。
        /// </param>
        /// <param name="serviceType">需要注册的服务类型，一般是接口或者抽象类等用于定义服务契约，也可以是具体类类型。</param>
        /// <param name="factory">
        /// 创建服务实例的工厂函数，接收 <see
        /// cref="IServiceProvider"/> 并返回服务实例对象， 可用于实现根据容器内其他服务或特定逻辑来创建服务实例的功能。
        /// </param>
        /// <param name="lifetime">服务的生命周期，指定实例的创建和复用规则，例如单例、瞬时、作用域等不同策略。</param>
        /// <returns>
        /// 添加服务后的 <see
        /// cref="IServiceCollection"/>
        /// 实例，方便继续添加其他服务注册， 例如可以接着注册更多不同类型、不同生命周期的服务到依赖注入容器中。
        /// </returns>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            services.Add(ServiceDescriptorFactory.Create(serviceType, factory, lifetime));
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/>
        /// 中添加一个服务注册信息，通过给定的服务类型 <paramref
        /// name="serviceType"/>、实现类型 <paramref
        /// name="implementationType"/> 以及服务生命周期
        /// <paramref name="lifetime"/> 来注册服务。
        /// 这是一种相对简洁的服务注册方式，适用于服务实例创建逻辑相对简单，不需要通过工厂函数传入额外配置参数以及不需要服务键区分不同配置的常见情况， 根据指定的服务类型、实现类型和生命周期，将服务注册到依赖注入容器中，由容器按照相应的生命周期策略管理服务实例的创建和复用。
        /// </summary>
        /// <param name="services">
        /// <see cref="IServiceCollection"/> 实例，用于承载和管理服务注册相关操作，是依赖注入容器的服务集合。
        /// </param>
        /// <param name="serviceType">需要注册的服务类型，一般为接口或者抽象类等用于定义服务契约，表明服务对外暴露的功能接口，也可以是具体类类型。</param>
        /// <param name="implementationType">
        /// 具体实现 <paramref name="serviceType"/>
        /// 的类型，必须是类且实现了 <paramref
        /// name="serviceType"/> 所定义的契约，用于创建实际的服务实例。
        /// </param>
        /// <param name="lifetime">
        /// 服务的生命周期，确定了服务实例在容器中的创建和复用规则，比如 <see
        /// cref="ServiceLifetime.Singleton"/>（整个应用生命周期内共享一个实例）、
        /// <see
        /// cref="ServiceLifetime.Transient"/>（每次获取都创建新实例）、
        /// <see cref="ServiceLifetime.Scoped"/>（在特定范围内共享实例）等不同策略。
        /// </param>
        /// <returns>
        /// 添加服务后的 <see
        /// cref="IServiceCollection"/>
        /// 实例，便于进行链式调用，可继续添加其他服务注册信息， 例如注册更多不同类型或不同生命周期的服务到依赖注入容器中。
        /// </returns>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            services.Add(ServiceDescriptorFactory.Create(serviceType, implementationType, lifetime));
            return services;
        }

        #endregion Add

        #region Configuration

        /// <summary>
        /// 配置指定类型的服务
        /// </summary>
        /// <typeparam name="T">要配置的服务类型</typeparam>
        /// <param name="services">服务集合</param>
        /// <param name="action">用于配置服务的操作</param>
        /// <returns>配置后的服务集合</returns>
        /// <exception cref="ArgumentNullException">
        /// 当 services 或 action 为 null 时抛出
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// 当要配置的服务不是单例模式时抛出
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// 当要配置的服务还未创建时抛出
        /// </exception>
        public static IServiceCollection Configuration<T>(this IServiceCollection services, Action<T> action) where T : IConfiguration
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Type serviceType = typeof(T);
            T value = default;
            ConstructorInfo constructorInfo;
            for (var i = 0; i < services.Count; i++)
            {
                if (services[i].ServiceType == serviceType)
                {
                    if (services[i].Lifetime != ServiceLifetime.Singleton)
                        throw new InvalidOperationException(string.Format("要配置的文件不是全局唯一:{0}", serviceType.Name));

                    value = (T)services[i].ImplementationInstance;

                    if (value == null)
                    {
                        var type = services[i].ImplementationType;
                        constructorInfo = type.GetConstructors().First(c => c.GetParameters().Length == 0);

                        if (constructorInfo == null)
                            throw new NullReferenceException(string.Format("要配置的类需要手动创建:{0}", serviceType.Name));

                        value = (T)constructorInfo.Invoke(null);
                    }
                    action.Invoke(value);
                    return services;
                }
            }

            if (serviceType.IsAbstract)
                throw new KeyNotFoundException(string.Format("还未创建配置类：{0}", serviceType.Name));

            constructorInfo = serviceType.GetConstructors().First(c => c.GetParameters().Length == 0);

            if (constructorInfo == null)
                throw new NullReferenceException(string.Format("要配置的类需要手动创建:{0}", serviceType.Name));

            value = (T)constructorInfo.Invoke(null);
            action.Invoke(value);
            return services;
        }

        #endregion Configuration
    }
}