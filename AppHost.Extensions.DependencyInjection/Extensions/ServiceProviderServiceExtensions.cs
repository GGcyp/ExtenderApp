using System.Diagnostics;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 服务提供者扩展
    /// </summary>
    public static class ServiceProviderServiceExtensions
    {
        /// <summary>
        /// 从<see cref="IServiceProvider"/>获取指定服务类型实例<typeparamref name="T"/>。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static T? GetService<T>(this IServiceProvider provider)
        {
            return (T?)provider.GetService(typeof(T));
        }

        /// <summary>
        /// 从<see cref="IServiceProvider"/>获取指定服务类型实例<see cref="Type"/>。
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
        {
            object? service = provider.GetService(serviceType);
            if (service == null)
            {
                throw new InvalidOperationException(serviceType.ToString());
            }

            return service;
        }

        /// <summary>
        /// 从<see cref="IServiceProvider"/>获取指定服务类型实例<typeparamref name="T"/>。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static T GetRequiredService<T>(this IServiceProvider provider) where T : notnull
        {
            return (T)provider.GetRequiredService(typeof(T));
        }

        /// <summary>
        /// 从<see cref="IServiceProvider"/>获取指定服务类型实例<see cref="ServiceDescriptor"/>。
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="serviceDescriptor"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static object GetRequiredService(this IServiceProvider provider, ServiceDescriptor serviceDescriptor)
        {
            object? service = provider.GetService(serviceDescriptor.ServiceType);
            if (service == null)
            {
                throw new InvalidOperationException(serviceDescriptor.ServiceType.ToString());
            }

            return service;
        }
    }
}
