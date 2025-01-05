

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// TentativeProvider 的扩展方法类。
    /// </summary>
    public static class TentativeProviderExtensions
    {
        /// <summary>
        /// 向服务集合中添加 TentativeProvider 服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>返回扩展后的服务集合。</returns>
        public static IServiceCollection AddTentativeProvider(this IServiceCollection services)
        {
            return services.AddSingleton<ITentativeProvider, TentativeProvider>();
        }
    }
}
