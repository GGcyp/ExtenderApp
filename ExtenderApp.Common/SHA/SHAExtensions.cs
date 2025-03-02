using System.Security.Cryptography;
using AppHost.Extensions.DependencyInjection;

namespace ExtenderApp.Common.SHA
{
    /// <summary>
    /// SHA扩展类，提供SHA相关的扩展方法。
    /// </summary>
    internal static class SHAExtensions
    {
        /// <summary>
        /// 为IServiceCollection添加SHA256服务。
        /// </summary>
        /// <param name="services">IServiceCollection实例。</param>
        /// <returns>返回添加了SHA256服务的IServiceCollection实例。</returns>
        public static IServiceCollection AddSHA(this IServiceCollection services)
        {
            services.AddSingleton<SHA256>(p => SHA256.Create());
            return services;
        }
    }
}
