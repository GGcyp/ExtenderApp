using Microsoft.Extensions.DependencyInjection;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 作用域执行器类
    /// </summary>
    public class ScopeExecutor : IScopeExecutor
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<string, IScopeServiceProvider> _scopesProviderDict;

        /// <summary>
        /// 初始化作用域执行器
        /// </summary>
        /// <param name="addAction">添加作用域的委托</param>
        /// <param name="removeAction">移除作用域的委托</param>
        public ScopeExecutor(IServiceProvider provider)
        {
            _serviceProvider = provider;
            _scopesProviderDict = new();
        }

        public void LoadScope(IServiceCollection collection, ScopeOptions options)
        {
            var scopeName = options.ScopeName;
            if (string.IsNullOrEmpty(scopeName))
            {
                throw new ArgumentNullException(nameof(options.ScopeName), "作用域名称不能为空");
            }

            if (_scopesProviderDict.ContainsKey(scopeName))
            {
                throw new InvalidOperationException(string.Concat("不可以重复注册作用域: ", options.ScopeName));
            }

            var provider = collection.BuilderScopeServiceProvider(this, options, _serviceProvider);
            _scopesProviderDict.Add(options.ScopeName, provider);
        }

        public void UnLoadScope(string scope)
        {
            if (string.IsNullOrEmpty(scope))
            {
                return;
            }

            _scopesProviderDict.Remove(scope, out var scopeService);
            scopeService?.DisposeAsync().ConfigureAwait(false);
        }

        public IScopeServiceProvider? GetServiceProvider(string scope)
        {
            if (string.IsNullOrEmpty(scope))
            {
                throw new InvalidOperationException(nameof(IScopeExecutor));
            }

            if (_scopesProviderDict.TryGetValue(scope, out var provider))
            {
                return provider;
            }
            return null;
        }
    }
}