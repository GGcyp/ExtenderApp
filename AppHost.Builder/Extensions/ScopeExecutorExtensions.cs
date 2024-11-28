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
        /// 从指定程序集路径加载作用域。
        /// </summary>
        /// <param name="executor">ScopeExecutor 实例。</param>
        /// <param name="assemblyPath">程序集路径。</param>
        /// <returns>返回更新后的 ScopeExecutor 实例。</returns>
        public static TStartup? LoadScope<TStartup>(this IScopeExecutor executor, string assemblyPath) where TStartup : ScopeStartup
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            return executor.LoadScope<TStartup>(assembly);
        }

        /// <summary>
        /// 从指定程序集加载作用域。
        /// </summary>
        /// <param name="executor">ScopeExecutor 实例。</param>
        /// <param name="assembly">程序集实例。</param>
        /// <returns>返回更新后的 ScopeExecutor 实例。</returns>
        /// <exception cref="ArgumentNullException">如果程序集参数为 null，则抛出此异常。</exception>
        public static TStartup? LoadScope<TStartup>(this IScopeExecutor executor, Assembly assembly) where TStartup : ScopeStartup
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            var startType = assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && ScopeStartup.Type.IsAssignableFrom(t));
            if (startType is null) return null;

            var startup = (Activator.CreateInstance(startType) as TStartup)!;

            IScopeServiceCollection serviceDescriptors = ScopeServiceCollectionFactory.CreateScopeServiceCollection();
            startup.AddService(serviceDescriptors);
            executor.LoadScope(startup.ScopeName, serviceDescriptors);

            return startup;
        }
    }
}
