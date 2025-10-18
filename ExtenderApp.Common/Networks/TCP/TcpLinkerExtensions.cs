using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    public static class TcpLinkerExtensions
    {
        /// <summary>
        /// 向服务集合中添加TCP链接器相关服务。
        /// </summary>
        /// <param name="services">服务集合实例</param>
        /// <returns>服务集合实例</returns>
        public static IServiceCollection AddTcpLinker(this IServiceCollection services)
        {
            services.AddILinker<ITcpLinker, TcpLinker, TcpLinkerFactory>();
            return services;
        }
    }
}