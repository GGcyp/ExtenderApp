using ExtenderApp.Abstract;
using ExtenderApp.Common.Compressions.LZ4;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 压缩服务扩展类，提供注册压缩服务和相关扩展方法的功能。
    /// </summary>
    internal static class CompressionExtenssions
    {
        /// <summary>
        /// 注册压缩服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回更新后的服务集合</returns>
        public static IServiceCollection AddCompressions(this IServiceCollection services)
        {
            // 注册 LZ4 压缩服务
            services.AddSingleton<ILZ4Compression, LZ4Compression>();
            return services;
        }
    }
}