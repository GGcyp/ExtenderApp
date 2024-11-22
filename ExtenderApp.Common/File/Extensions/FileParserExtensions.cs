using AppHost.Extensions.DependencyInjection;

namespace ExtenderApp.Common.File
{
    internal static class FileParserExtensions
    {
        public static IServiceCollection AddFileParser(this IServiceCollection services)
        {
            //Provider
            services.AddFileParserProvider();

            //Parser
            services.AddJsonParser();
            
            return services;
        }

        private static IServiceCollection AddJsonParser(this IServiceCollection services)
        {
            services.AddSingleton<JsonParser_Microsoft>();

            return services;
        }
    }
}
