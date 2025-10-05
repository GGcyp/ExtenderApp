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
        /// 创建一个用于安全关闭/释放 <see cref="IServiceProvider"/> 的包装器。
        /// </summary>
        /// <param name="provider">要被关闭/释放的服务提供者实例。</param>
        /// <returns>返回 <see cref="IServiceProviderCloser"/>，可用于同步或异步释放。</returns>
        /// <remarks>
        /// - 不假设 provider 实现 <see cref="IDisposable"/>/<see cref="IAsyncDisposable"/>，释放时会按实际类型选择调用；
        /// - 返回对象线程安全且幂等，多次调用释放只生效一次。
        /// </remarks>
        /// <example>
        /// using var closer = provider.CreateCloser(); // 同步释放
        /// // 或
        /// await using var asyncCloser = provider.CreateCloser(); // 异步释放
        /// </example>
        internal static IServiceProviderCloser CreateCloser(this IServiceProvider provider)
        {
            return new ServiceProviderCloser(provider);
        }
    }
}
