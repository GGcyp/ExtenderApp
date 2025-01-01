

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// ScopeServiceProviderExtensions 类的文档注释。
    /// 提供与 IScopeServiceProvider 相关的扩展方法。
    /// </summary>
    internal static class ScopeServiceProviderExtensions
    {
        /// <summary>
        /// 创建一个新的 IScopeServiceProvider 实例。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="executor">作用域执行器。</param>
        /// <returns>返回一个新的 IScopeServiceProvider 实例。</returns>
        public static IScopeServiceProvider BuilderScopeServiceProvider(this IServiceCollection services, IScopeExecutor executor, ScopeOptions options, IServiceProvider provider)
        {
            return new ScopeServiceProvider(executor, options, provider, services);
        }
    }
}
