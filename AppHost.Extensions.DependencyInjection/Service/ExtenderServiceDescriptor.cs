
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 服务描述类,描述服务的类型实现和生存周期
    /// </summary>
    public class ExtenderServiceDescriptor : ServiceDescriptor
    {
        public ExtenderServiceDescriptor(Type serviceType, object instance) : base(serviceType, instance)
        {
        }

        public ExtenderServiceDescriptor(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, ServiceLifetime lifetime) : base(serviceType, implementationType, lifetime)
        {
        }

        public ExtenderServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime) : base(serviceType, factory, lifetime)
        {
        }

        public ExtenderServiceDescriptor(Type serviceType, object? serviceKey, object instance) : base(serviceType, serviceKey, instance)
        {
        }

        public ExtenderServiceDescriptor(Type serviceType, object? serviceKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, ServiceLifetime lifetime) : base(serviceType, serviceKey, implementationType, lifetime)
        {
        }

        public ExtenderServiceDescriptor(Type serviceType, object? serviceKey, Func<IServiceProvider, object?, object> factory, ServiceLifetime lifetime) : base(serviceType, serviceKey, factory, lifetime)
        {
            
        }
    }
}
