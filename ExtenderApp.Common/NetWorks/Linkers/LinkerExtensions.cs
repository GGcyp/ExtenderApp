using System.Reflection;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 提供与链接操作相关的扩展方法。
    /// </summary>
    internal static class LinkerExtensions
    {
        private static readonly MethodInfo createListenerLinkerMethodInfo = typeof(IListenerLinkerFactory).GetMethod("CreateListenerLinker");

        /// <summary>
        /// 向服务集合中添加链接操作相关服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        public static IServiceCollection AddLinker(this IServiceCollection services)
        {
            services.AddTcpLinker();
            services.AddScoped<ILinkerFactory, LinkerFactory>();
            services.AddScoped<IListenerLinkerFactory, ListenerLinkerFactory>();

            services.AddTransient(typeof(IListenerLinker<>), (p, o) =>
            {
                var types = o as Type[];
                if (types is null)
                    return null;

                Type elementType = types[0];
                if (!elementType.IsAssignableTo(typeof(ILinker)))
                    return null;

                var listenerMethodInfo = createListenerLinkerMethodInfo.MakeGenericMethod(elementType);

                var factory = p.GetRequiredService<IListenerLinkerFactory>();

                var listenerLinker = listenerMethodInfo.Invoke(factory, null);

                return listenerLinker;
            });

            return services;
        }
    }
}
