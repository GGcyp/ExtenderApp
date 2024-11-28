
namespace AppHost.Extensions.DependencyInjection
{
    public interface IScopeExecutor
    {
        /// <summary>
        /// 根据作用域名称加载作用域。
        /// </summary>
        /// <param name="scopeName">作用域名称。</param>
        /// <param name="collection">作用域服务集合。</param>
        void LoadScope(string scopeName, IScopeServiceCollection collection);

        /// <summary>
        /// 卸载作用域。
        /// </summary>
        /// <param name="scopeName">要卸载的作用域名称。</param>
        /// <exception cref="ArgumentNullException">当 scopeName 为 null 或空字符串时抛出。</exception>
        void UnLoadScope(string scopeName);
    }
}
