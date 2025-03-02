using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.NetWorks.Send;
using ExtenderApp.Data;

namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// 提供与链接操作相关的扩展方法。
    /// </summary>
    internal static class LinkOperateExtensions
    {
        /// <summary>
        /// 向服务集合中添加链接操作相关服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        public static IServiceCollection AddLinkOperate(this IServiceCollection services)
        {
            services.Configuration<IBinaryFormatterStore>(s =>
            {
                s.AddFormatter(typeof(NetworkPacket), typeof(NetworkPacketFormatter));
            });

            services.AddTcpLinkOperate();

            return services;
        }
    }
}
