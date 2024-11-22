namespace AppHost.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services)
        {
            services.Add(ServiceDescriptor.Singleton<TService>());
            return services;
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.Add(ServiceDescriptor.Singleton<TService, TImplementation>());
            return services;
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService implementationInstance)
            where TService : class
        {
            services.Add(ServiceDescriptor.Singleton<TService>(implementationInstance));
            return services;
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, object> factory)
            where TService : class
        {
            services.Add(typeof(TService), factory, ServiceLifetime.Singleton);
            return services;
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, object?, object> factory, object? serviceKey)
            where TService : class
        {
            services.Add(typeof(TService), factory, serviceKey, ServiceLifetime.Singleton);
            return services;
        }

        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient(typeof(TService), typeof(TImplementation));
            return services;
        }

        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            services.Add(serviceType, implementationType, ServiceLifetime.Transient);
            return services;
        }

        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Type implementationType, Func<IServiceProvider, object?, object> factory)
        {
            services.Add(serviceType, implementationType, factory, null, ServiceLifetime.Transient);
            return services;
        }

        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object?, object> factory)
        {
            services.Add(serviceType, factory, ServiceLifetime.Transient);
            return services;
        }

        public static IServiceCollection AddTransient<TService>(this IServiceCollection services)
            where TService : class
        {
            services.Add(typeof(TService), typeof(TService), ServiceLifetime.Transient);
            return services;
        }

        public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, object> factory)
            where TService : class
        {
            services.Add(typeof(TService), factory, ServiceLifetime.Transient);
            return services;
        }

        public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, object?, object> factory, object? serviceKey)
            where TService : class
        {
            services.Add(typeof(TService), factory, serviceKey, ServiceLifetime.Transient);
            return services;
        }

        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped));
            return services;
        }

        public static IServiceCollection AddScoped<TService>(this IServiceCollection services)
            where TService : class
        {
            services.AddScoped<TService,TService>();
            return services;
        }

        public static IServiceCollection AddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, object> factory)
            where TService : class
        {
            services.Add(typeof(TService), factory, ServiceLifetime.Scoped);
            return services;
        }

        public static IServiceCollection Add<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
        {
            services.Add(typeof(TService), typeof(TImplementation), lifetime);
            return services;
        }

        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object?, object> factory, ServiceLifetime lifetime)
        {
            services.Add(new ServiceDescriptor(serviceType, factory, lifetime));
            return services;
        }

        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object?, object> factory, object? serviceKey, ServiceLifetime lifetime)
        {
            services.Add(new ServiceDescriptor(serviceType, factory, serviceKey, lifetime));
            return services;
        }

        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            services.Add(new ServiceDescriptor(serviceType, factory, lifetime));
            return services;
        }

        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Type implementationType, Func<IServiceProvider, object?, object> factory, object? serviceKey, ServiceLifetime lifetime)
        {
            services.Add(new ServiceDescriptor(serviceType, implementationType, factory, serviceKey, lifetime));
            return services;
        }

        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
            return services;
        }
    }
}
