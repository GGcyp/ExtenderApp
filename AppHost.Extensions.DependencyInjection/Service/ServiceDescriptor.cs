
namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 服务描述类,描述服务的类型实现和生存周期
    /// </summary>
    public class ServiceDescriptor
    {
        /// <summary>
        /// 服务类型
        /// </summary>
        public Type ServiceType { get; }

        private Type? m_ImplementationType;
        /// <summary>
        /// 实例类型
        /// </summary>
        public Type? ImplementationType => m_ImplementationType;

        /// <summary>
        /// 生存周期
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// 工厂生成的实例类钥匙
        /// </summary>
        public object? ServiceKey { get; set; } = null;

        /// <summary>
        /// 实例创建工厂
        /// </summary>
        private object? m_ImplementationFactory;

        /// <summary>
        /// 获取用工厂生成实例的服务实例
        /// </summary>
        public Func<IServiceProvider, object>? ImplementationFactory
            => (Func<IServiceProvider, object>?)m_ImplementationFactory;

        /// <summary>
        /// 有参数，获取用工厂生成实例的服务实例
        /// </summary>
        public Func<IServiceProvider, object?, object>? KeyedImplementationFactory
            => (Func<IServiceProvider, object?, object>?)m_ImplementationFactory;

        /// <summary>
        /// 是需要有传入参数工厂生成实例
        /// </summary>
        public bool IsKeyedService => ServiceKey != null;

        /// <summary>
        /// 是否有创建工厂
        /// </summary>
        public bool HasFactory => m_ImplementationFactory != null;

        /// <summary>
        /// 实例的实体
        /// </summary>
        public object? ImplementationInstance { get; set; }

        public ServiceDescriptor(Type serviceType, object implementationInstance) : this(serviceType, serviceKey: null, ServiceLifetime.Singleton)
        {
            if (implementationInstance == null) throw new ArgumentNullException(nameof(implementationInstance));
            ImplementationInstance = implementationInstance;
            m_ImplementationType = ImplementationInstance.GetType();
        }

        public ServiceDescriptor(Type serviceType, Type ImplementationType, Func<IServiceProvider, object?, object> factoryFunc, object? serviceKey, ServiceLifetime lifetime) : this(serviceType, serviceKey, lifetime)
        {
            if (ImplementationType == null) throw new ArgumentNullException(nameof(ImplementationInstance));
            m_ImplementationType = ImplementationType;
            m_ImplementationFactory = factoryFunc;
        }

        public ServiceDescriptor(Type serviceType, Type ImplementationType, ServiceLifetime lifetime) : this(serviceType, serviceKey: null, lifetime)
        {
            if (ImplementationType == null) throw new ArgumentNullException(nameof(ImplementationInstance));
            m_ImplementationType = ImplementationType;
        }

        public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factoryFunc, ServiceLifetime lifetime) : this(serviceType, serviceKey: null, lifetime)
        {
            m_ImplementationFactory = factoryFunc;
        }

        public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object?, object> factoryFunc, object? serviceKey, ServiceLifetime lifetime) : this(serviceType, serviceKey, lifetime)
        {
            if(serviceKey == null)
            {
                Func<IServiceProvider, object> nullKeyedFactory = sp => factoryFunc(sp, null);
                m_ImplementationFactory = nullKeyedFactory;
            }
            else
            {
                m_ImplementationFactory = factoryFunc;
            }
        }

        public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object?, object> factoryFunc, ServiceLifetime lifetime) : this(serviceType, serviceKey: null, lifetime)
        {
            m_ImplementationFactory = factoryFunc;
        }

        private ServiceDescriptor(Type serviceType, object? serviceKey, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            ServiceKey = serviceKey;
            Lifetime = lifetime;
        }

        public static ServiceDescriptor Singleton<TService>()
            => Singleton<TService, TService>();

        public static ServiceDescriptor Singleton<TService, TImplementation>()
            => Singleton(typeof(TService), typeof(TImplementation));

        public static ServiceDescriptor Singleton(Type serviceType, Type implementationType)
            => new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton);

        public static ServiceDescriptor Singleton<TService>(object implementationInstance)
            => Singleton(typeof(TService), implementationInstance);

        public static ServiceDescriptor Singleton(Type serviceType, object implementationInstance)
            => new ServiceDescriptor(serviceType, implementationInstance);
    }
}
