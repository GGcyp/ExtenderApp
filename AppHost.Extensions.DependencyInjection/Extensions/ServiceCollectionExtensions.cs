using System.Reflection;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 这个静态类提供了一系列用于扩展 <see cref="IServiceCollection"/> 接口的扩展方法，
    /// 旨在帮助开发者更便捷、灵活地向依赖注入容器中注册不同类型、不同生命周期以及各种配置的服务，以满足多样化的应用开发需求。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        #region Add

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的单例服务，使用默认的构造函数来实例化服务类型。
        /// 即容器在整个应用程序生命周期内只会创建一个该服务的实例，并在每次请求该服务时都返回这个唯一实例。
        /// </summary>
        /// <param name="services">
        /// <see cref="IServiceCollection"/> 实例，代表依赖注入容器的服务集合，用于添加服务注册信息。
        /// </param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便链式调用。</returns>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services)
        {
            services.Add(ServiceDescriptor.Singleton<TService>());
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的单例服务，并指定其具体的实现类型 <typeparamref name="TImplementation"/>。
        /// 要求 <typeparamref name="TImplementation"/> 必须是类并且实现了 <typeparamref name="TService"/> 接口或继承自 <typeparamref name="TService"/> 类（用于接口与实现类的关联注册）。
        /// 容器会创建 <typeparamref name="TImplementation"/> 的单例实例，并在请求 <typeparamref name="TService"/> 时返回该实例。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类，通常是接口或者抽象类等用于定义服务契约。</typeparam>
        /// <typeparam name="TImplementation">具体实现 <typeparamref name="TService"/> 的类型，必须是类并且要符合继承或实现关系要求。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，便于进行链式调用。</returns>
        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.Add(ServiceDescriptor.Singleton<TService, TImplementation>());
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的单例服务，并直接提供该服务的实例 <paramref name="implementationInstance"/>。
        /// 容器会将给定的实例作为单例保存，并在每次请求 <typeparamref name="TService"/> 时返回这个提供的实例。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="implementationInstance">要注册为单例的 <typeparamref name="TService"/> 类型的具体实例。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便后续继续添加其他服务注册。</returns>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService implementationInstance)
            where TService : class
        {
            services.Add(ServiceDescriptor.Singleton<TService>(implementationInstance));
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的单例服务，通过给定的工厂函数 <paramref name="factory"/> 来创建服务实例。
        /// 工厂函数接收一个 <see cref="IServiceProvider"/> 对象（可用于从容器中获取其他依赖服务），并返回一个对象（作为 <typeparamref name="TService"/> 的实例）。
        /// 容器在首次请求该服务时会调用此工厂函数创建实例，并在后续请求中都返回这个创建好的单例实例。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="factory">用于创建 <typeparamref name="TService"/> 实例的工厂函数，它接受 <see cref="IServiceProvider"/> 作为参数并返回创建的服务实例对象。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，便于链式调用其他扩展方法继续添加服务注册。</returns>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, object> factory)
            where TService : class
        {
            services.Add(typeof(TService), factory, ServiceLifetime.Singleton);
            return services;
        }

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
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的单例服务，通过给定的工厂函数 <paramref name="factory"/> 来创建服务实例，并关联一个服务键 <paramref name="serviceKey"/>。
        /// 服务键可用于区分同一服务类型的不同注册情况（例如在有多个不同配置的相同类型服务注册时）。
        /// 工厂函数接收一个 <see cref="IServiceProvider"/> 对象（用于获取其他依赖服务）以及一个可选的对象参数（可能用于配置等情况），并返回服务实例对象。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="factory">创建服务实例的工厂函数，接收 <see cref="IServiceProvider"/> 和可选对象参数，返回服务实例对象。</param>
        /// <param name="serviceKey">用于区分服务注册的键，可为 null。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便后续继续添加服务注册。</returns>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, object?, object> factory, object? serviceKey)
            where TService : class
        {
            services.Add(typeof(TService), factory, serviceKey, ServiceLifetime.Singleton);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的瞬时（Transient）服务，并指定其具体的实现类型 <typeparamref name="TImplementation"/>。
        /// 要求 <typeparamref name="TImplementation"/> 必须是类并且实现了 <typeparamref name="TService"/> 接口或继承自 <typeparamref name="TService"/> 类（用于接口与实现类的关联注册）。
        /// 每次从容器中请求该服务时，都会创建一个新的 <typeparamref name="TImplementation"/> 实例（即服务不是共享的，每次获取都是全新的实例）。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类，通常是接口或者抽象类等用于定义服务契约。</typeparam>
        /// <typeparam name="TImplementation">具体实现 <typeparamref name="TService"/> 的类型，必须是类并且要符合继承或实现关系要求。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便进行链式调用。</returns>
        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient(typeof(TService), typeof(TImplementation));
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <paramref name="serviceType"/> 的瞬时（Transient）服务，并指定其具体的实现类型 <paramref name="implementationType"/>。
        /// 每次请求该服务时，容器都会根据 <paramref name="implementationType"/> 创建一个新的实例返回（实例不共享，每次都是新的）。
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="serviceType">需要注册的服务类型，一般是接口或者抽象类等，用于定义服务契约。</param>
        /// <param name="implementationType">具体实现 <paramref name="serviceType"/> 的类型，用于创建实际的服务实例。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，便于后续继续添加其他服务注册。</returns>
        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            services.Add(serviceType, implementationType, ServiceLifetime.Transient);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <paramref name="serviceType"/> 的瞬时（Transient）服务，通过给定的工厂函数 <paramref name="factory"/> 来创建服务实例，并关联相关配置信息。
        /// 每次请求该服务时，都会调用工厂函数创建一个新的实例（保证每次获取的服务实例都是新创建的）。
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="serviceType">需要注册的服务类型。</param>
        /// <param name="implementationType">具体实现该服务的类型。</param>
        /// <param name="factory">创建服务实例的工厂函数，接收 <see cref="IServiceProvider"/> 和可选对象参数，返回服务实例对象。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便继续进行其他服务注册操作。</returns>
        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Type implementationType, Func<IServiceProvider, object?, object> factory)
        {
            services.Add(serviceType, implementationType, factory, null, ServiceLifetime.Transient);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <paramref name="serviceType"/> 的瞬时（Transient）服务，通过给定的工厂函数 <paramref name="factory"/> 来创建服务实例。
        /// 每次请求该服务时，都会调用工厂函数创建一个新的实例返回（实现了瞬时创建新实例的功能）。
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="serviceType">需要注册的服务类型。</param>
        /// <param name="factory">创建服务实例的工厂函数，接收 <see cref="IServiceProvider"/> 并返回服务实例对象。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，便于链式调用其他扩展方法继续添加服务注册。</returns>
        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object?, object> factory)
        {
            services.Add(serviceType, factory, ServiceLifetime.Transient);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的瞬时（Transient）服务，使用默认的构造函数来实例化服务类型。
        /// 每次从容器中请求该服务时，都会创建一个新的该服务实例（因为是瞬时生命周期）。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便进行链式调用。</returns>
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services)
            where TService : class
        {
            services.Add(typeof(TService), typeof(TService), ServiceLifetime.Transient);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的瞬时（Transient）服务，通过给定的工厂函数 <paramref name="factory"/> 来创建服务实例。
        /// 每次请求该服务时，都会调用工厂函数创建一个新的实例返回（符合瞬时服务每次获取都是新实例的特点）。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="factory">创建服务实例的工厂函数，接收 <see cref="IServiceProvider"/> 并返回服务实例对象。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，便于后续继续添加服务注册。</returns>
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, object> factory)
            where TService : class
        {
            services.Add(typeof(TService), factory, ServiceLifetime.Transient);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的瞬时（Transient）服务，通过给定的工厂函数 <paramref name="factory"/> 来创建服务实例，并关联一个服务键 <paramref name="serviceKey"/>。
        /// 服务键可用于区分同一服务类型的不同注册情况，每次请求该服务时都会调用工厂函数创建新的实例。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="factory">创建服务实例的工厂函数，接收 <see cref="IServiceProvider"/> 和可选对象参数，返回服务实例对象。</param>
        /// <param name="serviceKey">用于区分服务注册的键，可为 null。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便后续继续添加服务注册。</returns>
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, object?, object> factory, object? serviceKey)
            where TService : class
        {
            services.Add(typeof(TService), factory, serviceKey, ServiceLifetime.Transient);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的作用域（Scoped）服务，并指定其具体的实现类型 <typeparamref name="TImplementation"/>。
        /// 要求 <typeparamref name="TImplementation"/> 必须是类并且实现了 <typeparamref name="TService"/> 接口或继承自 <typeparamref name="TService"/> 类（用于接口与实现类的关联注册）。
        /// 在同一个作用域内（例如一个 HTTP 请求处理过程、一个特定的业务操作范围等），只会创建一个该服务的实例，在不同作用域中则会创建不同实例，
        /// 适用于那些在特定范围内需要共享状态，但在不同范围之间又要保持独立的服务，比如在一次 Web 请求中多个组件共享的业务逻辑服务等。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类，通常是接口或者抽象类等用于定义服务契约。</typeparam>
        /// <typeparam name="TImplementation">具体实现 <typeparamref name="TService"/> 的类型，必须是类并且要符合继承或实现关系要求。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便进行链式调用，
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.Add(new ScopeDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped));
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的作用域（Scoped）服务，使用默认的构造函数来实例化服务类型。
        /// 在同一个作用域内，容器只会创建一个该服务的实例，不同作用域会有不同的实例（实现了作用域内单例的功能），
        /// 通常用于那些本身内部逻辑适合在特定范围内共享状态的服务，简化了服务注册时指定实现类的过程（使用服务类型自身作为实现类型）。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便进行链式调用，
        public static IServiceCollection AddScoped<TService>(this IServiceCollection services)
            where TService : class
        {
            services.AddScoped<TService, TService>();
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中注册一个指定类型 <typeparamref name="TService"/> 的作用域（Scoped）服务，通过给定的工厂函数 <paramref name="factory"/> 来创建服务实例。
        /// 在同一个作用域内，首次请求该服务时调用工厂函数创建实例，后续在该作用域内的请求都会返回这个创建好的实例，不同作用域会重新创建，
        /// 借助工厂函数可以根据作用域内的依赖情况或其他逻辑灵活地创建服务实例，方便处理一些实例创建依赖于当前作用域资源的场景。
        /// </summary>
        /// <typeparam name="TService">需要注册的服务类型，必须是类。</typeparam>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="factory">创建服务实例的工厂函数，接收 <see cref="IServiceProvider"/> 并返回服务实例对象。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，便于后续继续添加服务注册，
        /// 比如添加其他不同类型或生命周期的服务到依赖注入容器中。</returns>
        public static IServiceCollection AddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, object> factory)
            where TService : class
        {
            services.Add(typeof(TService), factory, ServiceLifetime.Scoped);
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中添加一个服务注册信息，通过给定的服务类型 <paramref name="serviceType"/>、工厂函数 <paramref name="factory"/> 以及服务生命周期 <paramref name="lifetime"/> 来注册服务。
        /// 工厂函数用于创建服务实例，服务生命周期决定了实例的创建和复用策略（如单例、瞬时、作用域），
        /// 此方法提供了一种通用的方式来注册服务，根据传入的不同参数可以灵活配置服务的创建和管理方式。
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="serviceType">需要注册的服务类型，一般是接口或者抽象类等用于定义服务契约，也可以是具体的类类型。</param>
        /// <param name="factory">创建服务实例的工厂函数，接收 <see cref="IServiceProvider"/> 和可选对象参数，返回服务实例对象，
        /// 通过工厂函数可以实现复杂的实例创建逻辑，依赖于容器内其他服务或者进行特定的初始化操作等。</param>
        /// <param name="lifetime">服务的生命周期，指定实例的创建和复用规则，如 <see cref="ServiceLifetime.Singleton"/>（单例）、
        /// <see cref="ServiceLifetime.Transient"/>（瞬时）、<see cref="ServiceLifetime.Scoped"/>（作用域）等不同策略。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便继续添加其他服务注册，
        /// 例如可以接着注册更多不同类型、不同生命周期或者不同配置的服务到依赖注入容器中。</returns>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object?, object> factory, ServiceLifetime lifetime)
        {
            services.Add(ServiceDescriptorFactory.Create(serviceType, factory, lifetime));
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中添加一个服务注册信息，通过给定的服务类型 <paramref name="serviceType"/>、工厂函数 <paramref name="factory"/>、服务键 <paramref name="serviceKey"/> 以及服务生命周期 <paramref name="lifetime"/> 来注册服务。
        /// 服务键可用于区分同一服务类型的不同注册情况（例如在有多个不同配置的相同类型服务注册时），工厂函数用于创建服务实例，生命周期决定实例创建和复用策略，
        /// 这种方式增加了服务注册的灵活性，允许根据不同的键获取特定配置的服务实例，满足更复杂多样的业务需求。
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="serviceType">需要注册的服务类型，通常是接口或者抽象类等用于定义服务契约，也可以是具体类类型。</param>
        /// <param name="factory">创建服务实例的工厂函数，接收 <see cref="IServiceProvider"/> 和可选对象参数，返回服务实例对象，
        /// 方便按照不同的逻辑和依赖关系创建服务实例。</param>
        /// <param name="serviceKey">用于区分服务注册的键，可为 null，通过该键可以在获取服务时指定获取特定配置的实例。</param>
        /// <param name="lifetime">服务的生命周期，如单例、瞬时、作用域等不同规则，决定了服务实例的创建和复用方式。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，便于后续继续添加服务注册，
        /// 比如继续注册其他服务或者添加更多基于不同键的相同类型服务注册等。</returns>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object?, object> factory, object? serviceKey, ServiceLifetime lifetime)
        {
            services.Add(ServiceDescriptorFactory.Create(serviceType, factory, serviceKey, lifetime));
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中添加一个服务注册信息，通过给定的服务类型 <paramref name="serviceType"/>、工厂函数 <paramref name="factory"/> 以及服务生命周期 <paramref name="lifetime"/> 来注册服务。
        /// 工厂函数用于创建服务实例，服务生命周期决定了实例的创建和复用策略（比如单例、瞬时、作用域等情况），
        /// 提供了一种相对简洁的方式来注册服务，适用于不需要服务键区分不同配置的常见服务注册场景。
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于添加服务注册信息。</param>
        /// <param name="serviceType">需要注册的服务类型，一般是接口或者抽象类等用于定义服务契约，也可以是具体类类型。</param>
        /// <param name="factory">创建服务实例的工厂函数，接收 <see cref="IServiceProvider"/> 并返回服务实例对象，
        /// 可用于实现根据容器内其他服务或特定逻辑来创建服务实例的功能。</param>
        /// <param name="lifetime">服务的生命周期，指定实例的创建和复用规则，例如单例、瞬时、作用域等不同策略。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便继续添加其他服务注册，
        /// 例如可以接着注册更多不同类型、不同生命周期的服务到依赖注入容器中。</returns>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            services.Add(ServiceDescriptorFactory.Create(serviceType, factory, lifetime));
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中添加一个服务注册信息，通过给定的服务类型 <paramref name="serviceType"/>、实现类型 <paramref name="implementationType"/>、
        /// 工厂函数 <paramref name="factory"/>、服务键 <paramref name="serviceKey"/> 以及服务生命周期 <paramref name="lifetime"/> 来注册服务。
        /// 此方法综合了多种配置元素，使得服务注册更加灵活，适用于复杂的依赖注入场景，例如需要根据不同条件创建实例、区分不同配置的相同服务类型等情况。
        /// 实现类型用于指定具体创建服务实例的类型，工厂函数可按照更复杂的逻辑（可能依赖于容器内其他服务等情况）来创建实例，服务键可用于区分不同配置的注册，
        /// 生命周期则决定了服务实例在容器中的创建和复用策略。
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，是依赖注入容器的服务集合，用于添加和管理服务注册信息。</param>
        /// <param name="serviceType">需要注册的服务类型，通常为接口或者抽象类等用于定义服务契约，也可以是具体类类型，表示对外暴露的服务接口或抽象定义。</param>
        /// <param name="implementationType">具体实现 <paramref name="serviceType"/> 的类型，必须是类类型，用于创建实际的服务实例，它实现了 <paramref name="serviceType"/> 所定义的契约。</param>
        /// <param name="factory">创建服务实例的工厂函数，接收 <see cref="IServiceProvider"/>（用于获取容器内其他已注册的依赖服务）和可选的对象参数（可用于传递额外配置等信息），
        /// 并返回服务实例对象，通过该函数可以灵活控制实例创建过程，例如根据不同的依赖情况或配置来创建不同的服务实例。</param>
        /// <param name="serviceKey">用于区分服务注册的键，可为 null，在有多个针对同一 <paramref name="serviceType"/> 但不同配置的注册时，
        /// 通过此键可以在从容器获取服务时指定获取特定配置的服务实例，增加了服务注册和获取的灵活性。</param>
        /// <param name="lifetime">服务的生命周期，指定了实例的创建和复用规则，常见的有 <see cref="ServiceLifetime.Singleton"/>（单例，整个应用生命周期内共享一个实例）、
        /// <see cref="ServiceLifetime.Transient"/>（瞬时，每次获取都创建新的实例）、<see cref="ServiceLifetime.Scoped"/>（作用域，在特定范围内共享实例）等不同策略。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，方便继续添加其他服务注册，支持链式调用，
        /// 可以接着注册更多不同类型、不同生命周期、不同配置的服务到依赖注入容器中。</returns>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Type implementationType, Func<IServiceProvider, object?, object> factory, object? serviceKey, ServiceLifetime lifetime)
        {
            services.Add(ServiceDescriptorFactory.Create(serviceType, implementationType, factory, serviceKey, lifetime));
            return services;
        }

        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 中添加一个服务注册信息，通过给定的服务类型 <paramref name="serviceType"/>、实现类型 <paramref name="implementationType"/> 以及服务生命周期 <paramref name="lifetime"/> 来注册服务。
        /// 这是一种相对简洁的服务注册方式，适用于服务实例创建逻辑相对简单，不需要通过工厂函数传入额外配置参数以及不需要服务键区分不同配置的常见情况，
        /// 根据指定的服务类型、实现类型和生命周期，将服务注册到依赖注入容器中，由容器按照相应的生命周期策略管理服务实例的创建和复用。
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> 实例，用于承载和管理服务注册相关操作，是依赖注入容器的服务集合。</param>
        /// <param name="serviceType">需要注册的服务类型，一般为接口或者抽象类等用于定义服务契约，表明服务对外暴露的功能接口，也可以是具体类类型。</param>
        /// <param name="implementationType">具体实现 <paramref name="serviceType"/> 的类型，必须是类且实现了 <paramref name="serviceType"/> 所定义的契约，用于创建实际的服务实例。</param>
        /// <param name="lifetime">服务的生命周期，确定了服务实例在容器中的创建和复用规则，比如 <see cref="ServiceLifetime.Singleton"/>（整个应用生命周期内共享一个实例）、
        /// <see cref="ServiceLifetime.Transient"/>（每次获取都创建新实例）、<see cref="ServiceLifetime.Scoped"/>（在特定范围内共享实例）等不同策略。</param>
        /// <returns>添加服务后的 <see cref="IServiceCollection"/> 实例，便于进行链式调用，可继续添加其他服务注册信息，
        /// 例如注册更多不同类型或不同生命周期的服务到依赖注入容器中。</returns>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            services.Add(ServiceDescriptorFactory.Create(serviceType, implementationType, lifetime));
            return services;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// 配置指定类型的服务
        /// </summary>
        /// <typeparam name="T">要配置的服务类型</typeparam>
        /// <param name="services">服务集合</param>
        /// <param name="action">用于配置服务的操作</param>
        /// <returns>配置后的服务集合</returns>
        /// <exception cref="ArgumentNullException">当 services 或 action 为 null 时抛出</exception>
        /// <exception cref="InvalidOperationException">当要配置的服务不是单例模式时抛出</exception>
        /// <exception cref="NullReferenceException">当要配置的服务还未创建时抛出</exception>
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

        #endregion
    }
}
