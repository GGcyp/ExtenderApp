using System.Reflection;
using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 作用域执行器类
    /// </summary>
    public class ScopeExecutor : IScopeExecutor
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<string, IServiceScope> _scopesDict;

        /// <summary>
        /// 初始化作用域执行器
        /// </summary>
        /// <param name="addAction">添加作用域的委托</param>
        /// <param name="removeAction">移除作用域的委托</param>
        public ScopeExecutor(IServiceProvider provider)
        {
            _serviceProvider = provider;
            _scopesDict = new();
        }

        public void LoadScope(IServiceCollection collection, string scopeName)
        {
            if (string.IsNullOrEmpty(scopeName))
            {
                throw new ArgumentNullException(nameof(scopeName), "作用域名称不能为空");
            }

            if (_scopesDict.ContainsKey(scopeName))
            {
                throw new InvalidOperationException(string.Concat("不可以重复注册作用域: ", scopeName));
            }

        }

        public void UnLoadScope(string scope)
        {
            if (string.IsNullOrEmpty(scope))
            {
                return;
            }

            _scopesDict.Remove(scope, out var scopeService);
            //scopeService?.DisposeAsync().ConfigureAwait(false);
        }

        public IServiceProvider? GetServiceProvider(string scope)
        {
            if (string.IsNullOrEmpty(scope))
            {
                throw new InvalidOperationException(nameof(IScopeExecutor));
            }

            if (_scopesDict.TryGetValue(scope, out var provider))
            {
                return provider;
            }
            return null;
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
        public TStartup? LoadScope<TStartup>(IScopeExecutor executor, string assemblyPath, Action<IServiceCollection, string> callback = null) where TStartup : ScopeStartup
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            return executor.LoadScope<TStartup>(executor, assembly, callback);
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
        public static TStartup? LoadScope<TStartup>(IScopeExecutor executor, Assembly assembly, Action<IServiceCollection, string> callback = null) where TStartup : ScopeStartup
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            var startType = assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && ScopeStartup.Type.IsAssignableFrom(t));
            if (startType is null) return null;

            var startup = (Activator.CreateInstance(startType) as TStartup)!;

            IServiceCollection services = ServiceBuilder.CreateServiceCollection();
            startup.AddService(services);
            ScopeOptionsBuilder builder = new();
            startup.ConfigureScopeOptions(builder);
            var options = builder.Build();
            callback?.Invoke(services, options);
            executor.LoadScope(services, options);

            return startup;
        }
    }
}