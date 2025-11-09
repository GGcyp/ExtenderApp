using ExtenderApp.Abstract;
using ExtenderApp.Common;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Services
{
    /// <summary>
    /// ServiceStartup 类，继承自 Startups 类，用于配置应用程序的服务。
    /// </summary>
    public class ServiceStartup : Startup
    {
        /// <summary>
        /// 重写 AddService 方法，用于向服务集合中添加服务。
        /// </summary>
        /// <param name="services">服务集合，用于添加服务。</param>
        public override void AddService(IServiceCollection services)
        {
            services.AddSingleton<IServiceStore, ServiceStore>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDispatcherService, DispatcherService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<IMainWindowService, MainWindowService>();
            services.AddSingleton<ISystemService, SystemService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IPluginService, PluginService>();
            services.AddSingleton<PluginStore>();
            services.AddSingleton<ILocalDataService, LocalDataService>();
        }
    }
}