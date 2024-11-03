using AppHost.Extensions.DependencyInjection;

namespace MainApp.Common.File
{
    internal static class FileParserExtensions
    {
        public static IServiceCollection AddFileParser(this IServiceCollection services)
        {
            services.AddSingleton<XmlParser>();
            services.AddSingleton<ExcelParser>();

            return services;
        }
    }
}
