using AppHost.Extensions.DependencyInjection;
using AppHost.Extensions.Hosting;

namespace AppHost.Builder
{
    public interface IHostApplicationBuilder
    {
        /// <summary>
        /// 所有配置数据
        /// </summary>
        IDictionary<object, object> Properties { get; }

        /// <summary>
        /// 所有需要添加的服务
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// 配置信息
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// 主机配置信息
        /// </summary>
        IHostEnvironment HostEnvironment { get; }
    }
}
