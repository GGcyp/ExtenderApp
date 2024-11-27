
namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 作用域执行器类
    /// </summary>
    internal class ScopeExecutor : IScopeExecutor
    {
        /// <summary>
        /// 添加作用域到描述符的委托
        /// </summary>
        private readonly Func<ServiceDescriptor, ServiceDescriptor?> _addAction;

        /// <summary>
        /// 从描述符中移除作用域的委托
        /// </summary>
        private readonly Action<ServiceDescriptor, ServiceDescriptor> _removeAction;

        private Dictionary<string, ScopeServiceCollection> scopes;

        /// <summary>
        /// 初始化作用域执行器
        /// </summary>
        /// <param name="addAction">添加作用域的委托</param>
        /// <param name="removeAction">移除作用域的委托</param>
        public ScopeExecutor(Func<ServiceDescriptor, ServiceDescriptor?> addAction, Action<ServiceDescriptor, ServiceDescriptor> removeAction)
        {
            _addAction = addAction;
            _removeAction = removeAction;
        }

        public void LoadScope(ScopeStartup startup)
        {
            if (scopes is null)
                scopes = new();

            if (startup is null)
                throw new ArgumentNullException(nameof(startup), "The startup parameter cannot be null.");

            if (string.IsNullOrEmpty(startup.ScopeName))
                throw new ArgumentNullException(nameof(startup.ScopeName), "The ScopeName property cannot be null or empty.");

            if (scopes.ContainsKey(startup.ScopeName))
                throw new InvalidOperationException(string.Format("the {0} scopes service already loaded", startup.ScopeName));

            ScopeServiceCollection collection = new();

            scopes.Add(startup.ScopeName, collection);

            startup.AddService(collection);

            AddScopes(collection);
        }

        public void UnLoadScope(string scopeName)
        {
            if (this.scopes is null) 
                return;

            if (string.IsNullOrEmpty(scopeName))
                throw new ArgumentNullException(nameof(scopeName), "The ScopeName property cannot be null or empty.");

            if (!this.scopes.TryGetValue(scopeName, out var scopes))
                return;

            this.scopes.Remove(scopeName);
            RemoveScopes(scopes);
        }


        /// <summary>
        /// 添加作用域
        /// </summary>
        /// <param name="services">作用域服务集合</param>
        private void AddScopes(ScopeServiceCollection services)
        {
            int socpeIndex = 0;
            for (int i = 0; i < services.Count; i++)
            {
                var service = services[i];
                var originaService = _addAction(service);
                if (originaService == null) continue;

                services.ScopeServices[socpeIndex].OriginaService = originaService;
                socpeIndex++;
            }
        }

        /// <summary>
        /// 移除作用域
        /// </summary>
        /// <param name="services">作用域服务集合</param>
        private void RemoveScopes(ScopeServiceCollection services)
        {
            int socpeIndex = 0;
            for (int i = 0; i < services.Count; i++)
            {
                var service = services[i];
                if (service.Lifetime != ServiceLifetime.Scoped)
                {
                    _removeAction(service, null);
                    continue;
                }

                _removeAction(service, services.ScopeServices[socpeIndex].OriginaService!);
                socpeIndex++;
            }
        }
    }
}
