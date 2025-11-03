using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Abstract
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
        IServiceProvider? GetServiceProvider(string scope);

        /// <summary>
        /// 加载作用域。
        /// </summary>
        /// <param name="collection">服务集合。</param>
        /// <param name="scopeName">作用域名称。</param>
        void LoadScope(IServiceCollection collection, string scopeName);

        /// <summary>
        /// 卸载作用域。
        /// </summary>
        /// <param name="scopeName">要卸载的作用域名称。</param>
        /// <exception cref="ArgumentNullException">
        /// 当 scopeName 为 null 或空字符串时抛出。
        /// </exception>
        void UnLoadScope(string scopeName);
    }
}