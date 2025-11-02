using AppHost.Extensions.DependencyInjection;
using AppHost.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost.Builder.Extensions
{
    /// <summary>
    /// HostServiceExtensions 类，提供扩展方法来向
    /// HostApplicationBuilder 添加服务执行器。
    /// </summary>
    internal static class HostServiceExtensions
    {
        /// <summary>
        /// 向 HostApplicationBuilder 添加
        /// HostedServiceExecutor 服务执行器。
        /// </summary>
        /// <param name="builder">
        /// HostApplicationBuilder 实例。
        /// </param>
        /// <returns>
        /// 返回修改后的 HostApplicationBuilder 实例。
        /// </returns>
        public static IHostApplicationBuilder AddHostedServiceExecutor(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<HostedServiceExecutor>();
            return builder;
        }
    }
}