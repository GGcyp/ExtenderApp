using System;

namespace AppHost.Extensions.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceImplementationAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }
        public Type ServiceType { get; }
        public ServiceImplementationAttribute(ServiceLifetime lifetime, Type serviceType)
        {
            Lifetime = lifetime;
            ServiceType = serviceType;
        }
    }
}
