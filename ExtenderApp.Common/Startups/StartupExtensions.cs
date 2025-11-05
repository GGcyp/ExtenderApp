using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Startups
{
    /// <summary>
    /// 开始扩展方法类
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// 将启动执行器添加到服务集合中。
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddStartupExecuter(this IServiceCollection services)
        {
            services.AddSingleton<IStartupExecuter, StartupExecuter>();
            return services;
        }
    }
}
