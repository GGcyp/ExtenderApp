using ExtenderApp.Common.Caches;
using ExtenderApp.Common.Hash;
using ExtenderApp.Common.Networks;
using ExtenderApp.Common.Startups;
using ExtenderApp.Common.Threads;
using ExtenderApp.Common.Scopes;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 通用层启动类，继承自Startup基类
    /// </summary>
    public class CommonStartup : Startup
    {
        public override void AddService(IServiceCollection services)
        {
            services.AddIO();
            services.AddNetwork();
            services.AddSerializations();
            services.AddCache();
            services.AddMainThreadContext();
            services.AddStartupExecuter();
            services.AddServiceScopeStore();
        }
    }
}
