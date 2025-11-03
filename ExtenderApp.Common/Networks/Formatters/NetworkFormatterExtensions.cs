
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.Formatters
{
    internal static class NetworkFormatterExtensions
    {
        /// <summary>
        /// 添加网络数据格式化器。
        /// </summary>
        /// <param name="services">需要被添加进的服务收集接口</param>
        /// <returns>服务收集接口</returns>
        public static IServiceCollection AddFormatter(this IServiceCollection services)
        {
            //services.Configuration<IBinaryFormatterStore>(s =>
            //{

            //});
            return services;
        }
    }
}
