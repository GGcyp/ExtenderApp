namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 服务描述符工厂类，用于创建不同类型的服务描述符。
    /// </summary>
    public static class ServiceDescriptorFactory
    {
        /// <summary>
        /// 创建一个服务描述符。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="implementationType">实现类型。</param>
        /// <param name="lifetime">服务生命周期。</param>
        /// <returns>返回创建的服务描述符。</returns>
        /// <exception cref="NotImplementedException">如果生命周期不匹配，则抛出此异常。</exception>
        public static ServiceDescriptor Create(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            return lifetime switch
            {
                ServiceLifetime.Singleton => new ServiceDescriptor(serviceType, implementationType, lifetime),
                ServiceLifetime.Transient => new ServiceDescriptor(serviceType, implementationType, lifetime),
                ServiceLifetime.Scoped => new ScopeDescriptor(serviceType, implementationType, lifetime),
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// 创建一个服务描述符，使用工厂方法和服务键。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="implementationType">实现类型。</param>
        /// <param name="factory">用于创建服务的工厂方法。</param>
        /// <param name="serviceKey">服务的键。</param>
        /// <param name="lifetime">服务生命周期。</param>
        /// <returns>返回创建的服务描述符。</returns>
        /// <exception cref="NotImplementedException">如果生命周期不匹配，则抛出此异常。</exception>
        public static ServiceDescriptor Create(Type serviceType, Type implementationType, Func<IServiceProvider, object?, object> factory, object? serviceKey, ServiceLifetime lifetime)
        {
            return lifetime switch
            {
                ServiceLifetime.Singleton => new ServiceDescriptor(serviceType, implementationType, factory, serviceKey, lifetime),
                ServiceLifetime.Transient => new ServiceDescriptor(serviceType, implementationType, factory, serviceKey, lifetime),
                ServiceLifetime.Scoped => new ScopeDescriptor(serviceType, implementationType, factory, serviceKey, lifetime),
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// 创建一个服务描述符，使用工厂方法和无服务键。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="factory">用于创建服务的工厂方法。</param>
        /// <param name="lifetime">服务生命周期。</param>
        /// <returns>返回创建的服务描述符。</returns>
        /// <exception cref="NotImplementedException">如果生命周期不匹配，则抛出此异常。</exception>
        public static ServiceDescriptor Create(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            return lifetime switch
            {
                ServiceLifetime.Singleton => new ServiceDescriptor(serviceType, factory, lifetime),
                ServiceLifetime.Transient => new ServiceDescriptor(serviceType, factory, lifetime),
                ServiceLifetime.Scoped => new ScopeDescriptor(serviceType, factory, lifetime),
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// 创建一个服务描述符，使用工厂方法和服务键。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="factory">用于创建服务的工厂方法。</param>
        /// <param name="serviceKey">服务的键。</param>
        /// <param name="lifetime">服务生命周期。</param>
        /// <returns>返回创建的服务描述符。</returns>
        /// <exception cref="NotImplementedException">如果生命周期不匹配，则抛出此异常。</exception>
        public static ServiceDescriptor Create(Type serviceType, Func<IServiceProvider, object?, object> factory, object? serviceKey, ServiceLifetime lifetime)
        {
            return lifetime switch
            {
                ServiceLifetime.Singleton => new ServiceDescriptor(serviceType, factory, serviceKey, lifetime),
                ServiceLifetime.Transient => new ServiceDescriptor(serviceType, factory, serviceKey, lifetime),
                ServiceLifetime.Scoped => new ScopeDescriptor(serviceType, factory, serviceKey, lifetime),
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// 创建一个服务描述符，使用工厂方法和服务键。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="factory">用于创建服务的工厂方法。</param>
        /// <param name="lifetime">服务生命周期。</param>
        /// <returns>返回创建的服务描述符。</returns>
        /// <exception cref="NotImplementedException">如果生命周期不匹配，则抛出此异常。</exception>
        public static ServiceDescriptor Create(Type serviceType, Func<IServiceProvider, object?, object> factory, ServiceLifetime lifetime)
        {
            return lifetime switch
            {
                ServiceLifetime.Singleton => new ServiceDescriptor(serviceType, factory, lifetime),
                ServiceLifetime.Transient => new ServiceDescriptor(serviceType, factory, lifetime),
                ServiceLifetime.Scoped => new ScopeDescriptor(serviceType, factory, lifetime),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
