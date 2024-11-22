using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Common.File;

namespace ExtenderApp.Common
{
    internal static class FileParserProviderExtensions
    {
        public static IServiceCollection AddFileParserProvider(this IServiceCollection services)
        {
            services.AddSingleton<IJsonPareserProvider, JsonPareserProvider>();

            return services;
        }

        /// <summary>
        /// 根据库名称获取指定的文件解析器实例。
        /// </summary>
        /// <param name="LibraryName">库名称，如果为 null，则返回默认的解析器实例。</param>
        /// <returns>返回指定库名称的文件解析器实例，如果库名称为 null 或没有找到匹配的解析器，则返回默认的解析器实例。如果解析器实例不存在，则返回 null。</returns>
        public static T? GetParser<T>(this IFileParserProvider<T> provider, string libraryName) where T : class, IFileParser
        {
            return provider.GetParser(libraryName) as T;
        }
    }
}
