using System.Reflection;
using AppHost.Extensions.DependencyInjection;


namespace AppHost.Builder.Extensions
{
    /// <summary>
    /// ScopeExecutor 的扩展方法类。
    /// </summary>
    public static class ScopeExecutorExtensions
    {
        /// <summary>
        /// 为 IHostApplicationBuilder 添加 ScopeExecutor 服务。
        /// </summary>
        /// <param name="host">IHostApplicationBuilder 实例。</param>
        /// <returns>返回添加了 ScopeExecutor 服务的 IHostApplicationBuilder 实例。</returns>
        public static IHostApplicationBuilder AddScopeExecutor(this IHostApplicationBuilder host)
        {
            host.Services.AddSingleton<IScopeExecutor, ScopeExecutor>();
            return host;
        }

        /// <summary>
        /// 根据程序集路径加载作用域。
        /// </summary>
        /// <typeparam name="TStartup">启动类类型，必须继承自 ScopeStartup。</typeparam>
        /// <param name="executor">IScopeExecutor 接口实例。</param>
        /// <param name="assemblyPath">程序集文件路径。</param>
        /// <param name="callback">可选的回调函数，用于配置作用域选项和服务集合。</param>
        /// <returns>加载的启动类实例，如果未找到匹配的启动类则返回 null。</returns>
        /// <exception cref="ArgumentNullException">如果 assembly 参数为 null，则抛出此异常。</exception>
        public static TStartup? LoadScope<TStartup>(this IScopeExecutor executor, string assemblyPath, Action<IServiceCollection, ScopeOptions> callback = null) where TStartup : ScopeStartup
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            return executor.LoadScope<TStartup>(assembly, callback);
        }

        /// <summary>
        /// 根据程序集加载作用域。
        /// </summary>
        /// <typeparam name="TStartup">启动类类型，必须继承自 ScopeStartup。</typeparam>
        /// <param name="executor">IScopeExecutor 接口实例。</param>
        /// <param name="assembly">程序集实例。</param>
        /// <param name="callback">可选的回调函数，用于配置作用域选项和服务集合。</param>
        /// <returns>加载的启动类实例，如果未找到匹配的启动类则返回 null。</returns>
        /// <exception cref="ArgumentNullException">如果 assembly 参数为 null，则抛出此异常。</exception>
        public static TStartup? LoadScope<TStartup>(this IScopeExecutor executor, Assembly assembly, Action<IServiceCollection, ScopeOptions> callback = null) where TStartup : ScopeStartup
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            var startType = assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && ScopeStartup.Type.IsAssignableFrom(t));
            if (startType is null) return null;

            var startup = (Activator.CreateInstance(startType) as TStartup)!;

            IServiceCollection services = ServiceBuilder.CreateServiceCollection();
            startup.AddService(services);
            ScopeOptions options = new ScopeOptions();
            startup.ConfigureScopeOptions(options);
            callback?.Invoke(services, options);
            executor.LoadScope(services, options);

            return startup;
        }
    }
}
