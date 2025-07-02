using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Splitter;
using ExtenderApp.Common.IO;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Data;
using ExtenderApp.Common.Error;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 文件解析器扩展类
    /// </summary>
    public static class FileParserExtensions
    {
        /// <summary>
        /// 为服务集合添加文件相关服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回添加文件相关服务后的服务集合</returns>
        public static IServiceCollection AddIO(this IServiceCollection services)
        {
            //Parser
            services.AddParser();

            //Binary
            services.AddBinary();

            //Splitter
            services.ConfigurationFileSplitter();

            //StreamOperate
            services.AddFileStore();

            return services;
        }

        /// <summary>
        /// 为服务集合添加解析器服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回添加解析器服务后的服务集合</returns>
        private static IServiceCollection AddParser(this IServiceCollection services)
        {
            services.AddSingleton<IJsonParser, JsonParser>();
            services.AddSingleton<IBinaryParser, BinaryParser>();
            services.AddSingleton<ISplitterParser, SplitterParser>();
            return services;
        }

        /// <summary>
        /// 为服务集合添加文件存储服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回添加文件存储服务后的服务集合</returns>
        private static IServiceCollection AddFileStore(this IServiceCollection services)
        {
            services.AddSingleton<IFileOperateProvider, FileOperateProvider>();
            return services;
        }
    }
}
