using AppHost.Extensions.DependencyInjection;

namespace MainApp.Common.File 
{ 
    internal static class FileParserStoreExtensions
    {
        /// <summary>
        /// 向 IHostApplicationBuilder 中添加 FileParserStore 的注册
        /// </summary>
        /// <param name="services">IHostApplicationBuilder 实例</param>
        /// <returns>返回添加 FileParserStore 后的 IHostApplicationBuilder 实例</returns>
        public static IServiceCollection AddFileParserStore(this IServiceCollection services)
        {
            services.AddTransient<FileParserStore>();
            return services;
        }
    }
}
