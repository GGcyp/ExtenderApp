namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 范围描述符类
    /// </summary>
    public class ScopeDescriptor : ServiceDescriptor
    {
        /// <summary>
        /// 被替换的服务
        /// </summary>
        public ServiceDescriptor? OriginaService { get; set; }

        public ScopeDescriptor(Type serviceType, object implementationInstance) : base(serviceType, implementationInstance)
        {
        }

        public ScopeDescriptor(Type serviceType, Type ImplementationType, ServiceLifetime lifetime) : base(serviceType, ImplementationType, lifetime)
        {
        }

        public ScopeDescriptor(Type serviceType, Func<IServiceProvider, object> factoryFunc, ServiceLifetime lifetime) : base(serviceType, factoryFunc, lifetime)
        {
        }

        public ScopeDescriptor(Type serviceType, Func<IServiceProvider, object?, object> factoryFunc, ServiceLifetime lifetime) : base(serviceType, factoryFunc, lifetime)
        {
        }

        public ScopeDescriptor(Type serviceType, Func<IServiceProvider, object?, object> factoryFunc, object? serviceKey, ServiceLifetime lifetime) : base(serviceType, factoryFunc, serviceKey, lifetime)
        {
        }

        public ScopeDescriptor(Type serviceType, Type ImplementationType, Func<IServiceProvider, object?, object> factoryFunc, object? serviceKey, ServiceLifetime lifetime) : base(serviceType, ImplementationType, factoryFunc, serviceKey, lifetime)
        {
        }

    }
}
