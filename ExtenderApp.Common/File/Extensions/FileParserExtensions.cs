using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.File
{
    /// <summary>
    /// 文件解析器扩展类
    /// </summary>
    internal static class FileParserExtensions
    {
        /// <summary>
        /// 为服务集合添加文件解析功能
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回修改后的服务集合</returns>
        public static IServiceCollection AddFile(this IServiceCollection services)
        {
            //Parser
            services.AddParser();

            return services;
        }

        /// <summary>
        /// 为服务集合添加JSON解析器
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回修改后的服务集合</returns>
        private static IServiceCollection AddParser(this IServiceCollection services)
        {
            services.AddSingleton<IJsonParser, JsonParser>();
            return services;
        }
    }
}
