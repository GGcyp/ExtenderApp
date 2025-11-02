using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 提供文件分割器扩展方法的静态类
    /// </summary>
    public static class SplitterExtensions
    {
        /// <summary>
        /// 扩展方法，用于将文件分割器配置添加到服务集合中。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>配置后的服务集合。</returns>
        public static IServiceCollection ConfigurationFileSplitter(this IServiceCollection services)
        {
            //services.AddSingleton<IFileSplitter, FileSplitter>();

            services.Configuration<IBinaryFormatterStore>(s =>
            {
                s.Add<SplitterInfo, SplitterInfoFormatter>();
                s.Add<SplitterDto, SplitterDtoFormatter>();
                s.Add<PieceData, PieceDataFormatter>();
            });

            return services;
        }
    }
}
