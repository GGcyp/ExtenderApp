
namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 作用域执行器接口。
    /// </summary>
    public interface IScopeExecutor
    {
        /// <summary>
        /// 根据指定的作用域名称获取服务提供程序。
        /// </summary>
        /// <param name="scope">作用域的名称。</param>
        /// <returns>返回指定作用域的服务提供程序。如果未找到，则返回null。</returns>
        IScopeServiceProvider? GetServiceProvider(string scope);

        /// <summary>
        /// 加载作用域。
        /// </summary>
        /// <param name="options">作用域选项。</param>
        /// <param name="collection">服务集合。</param>
        void LoadScope(ScopeOptions options, IServiceCollection collection);

        /// <summary>
        /// 卸载作用域。
        /// </summary>
        /// <param name="scopeName">要卸载的作用域名称。</param>
        /// <exception cref="ArgumentNullException">当 scopeName 为 null 或空字符串时抛出。</exception>
        void UnLoadScope(string scopeName);
    }
}
