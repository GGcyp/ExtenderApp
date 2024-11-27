
namespace AppHost.Extensions.DependencyInjection
{
    public interface IScopeExecutor
    {
        /// <summary>
        /// 加载作用域。
        /// </summary>
        /// <param name="startup">作用域启动参数。</param>
        /// <exception cref="ArgumentNullException">当 startup 或 startup.ScopeName 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当作用域已加载时抛出。</exception>
        void LoadScope(ScopeStartup startup);

        /// <summary>
        /// 卸载作用域。
        /// </summary>
        /// <param name="scopeName">要卸载的作用域名称。</param>
        /// <exception cref="ArgumentNullException">当 scopeName 为 null 或空字符串时抛出。</exception>
        void UnLoadScope(string scopeName);
    }
}
