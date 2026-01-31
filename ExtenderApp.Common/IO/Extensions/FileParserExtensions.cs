using ExtenderApp.Abstract;
using ExtenderApp.Common.IO;
using Microsoft.Extensions.DependencyInjection;

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
        internal static IServiceCollection AddIO(this IServiceCollection services)
        {
            //StreamOperate
            services.AddSingleton<IFileOperateProvider, FileOperateProvider>();

            return services;
        }
    }
}