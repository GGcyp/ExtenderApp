using ExtenderApp.Abstract;
using ExtenderApp.Common.Compressions.LZ4;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    public static class CompressionExtenssions
    {
        public static IServiceCollection AddCompressions(this IServiceCollection services)
        {
            // 注册 LZ4 压缩服务
            services.AddSingleton<ILZ4Compression, LZ4Compression>();
            return services;
        }
    }
}