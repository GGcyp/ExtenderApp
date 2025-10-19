using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Pipelines
{
    internal static class PipelineExtensions
    {
        public static IServiceCollection AddPipeline(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBuilder<>), (p, o) =>
            {
                if (o is not Type[] types || types.Length > 1)
                    return null;

                var pipelineBuilderType = typeof(PipelineBuilder<>).MakeGenericType(types);
                return Activator.CreateInstance(pipelineBuilderType, p);
            });
            return services;
        }

        public static IPipelineBuilder<T> CreatePipelineBuilder<T>(this IServiceProvider serviceProvider)
            where T : IPipelineContext
        {
            return new PipelineBuilder<T>(serviceProvider);
        }
    }
}
