using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.NetWorks.Linkers.SendDtos;

namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// 提供与链接操作相关的扩展方法。
    /// </summary>
    internal static class LinkerExtensions
    {
        /// <summary>
        /// 向服务集合中添加链接操作相关服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        public static IServiceCollection AddLinkOperate(this IServiceCollection services)
        {
            services.AddTcpLinkOperate();
            services.Configuration<IBinaryFormatterStore>(s =>
            {
                s.Add<LinkerDto, LinkerDtoFormatter>();
                s.Add<SendHead, SendHeadFormatter>();
            });

            return services;
        }
    }
}
