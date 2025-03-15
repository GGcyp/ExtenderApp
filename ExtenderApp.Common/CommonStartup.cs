using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Common.Networks;
using ExtenderApp.Common.SHA;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 通用层启动类，继承自Startup基类
    /// </summary>
    public class CommonStartup : Startup
    {
        public override void AddService(IServiceCollection services)
        {
            services.AddFile();
            services.AddObjectPool();
            services.AddNetwork();
            services.AddSHA();
        }
    }
}
