using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Threads
{
    /// <summary>
    /// 主线程上下文扩展方法
    /// </summary>
    internal static class MainThreadContextExtensions
    {
        /// <summary>
        /// 添加主线程上下文服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddMainThreadContext(this IServiceCollection services)
        {
            services.AddSingleton<IMainThreadContext, MainThreadContext>();
            return services;
        }
    }
}