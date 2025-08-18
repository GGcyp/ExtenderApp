using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO;
using ExtenderApp.Data;
using ExtenderApp.Services;

namespace ExtenderApp.Services
{
    /// <summary>
    /// ServiceStartup 类，继承自 Startup 类，用于配置应用程序的服务。
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
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<ILogingService, LoggingService>();
            services.AddSingleton<IPathService, PathService>();
            services.AddSingleton<IMainWindowService, MainWindowService>();

            AddPluginService(services);
            AddLocaDataService(services);
        }

        /// <summary>
        /// 添加插件服务到服务集合中。
        /// </summary>
        /// <param name="services">服务集合。</param>
        private void AddPluginService(IServiceCollection services)
        {
            /// <summary>
            /// 向服务集合中添加一个单例的IPluginService实现。
            /// </summary>
            services.AddSingleton<IPluginService, PluginService>();

            /// <summary>
            /// 向服务集合中添加一个单例的PluginStore。
            /// </summary>
            services.AddSingleton<PluginStore>();
        }

        /// <summary>
        /// 添加本地化数据服务
        /// </summary>
        /// <param name="services">服务集合</param>
        private void AddLocaDataService(IServiceCollection services)
        {
            services.AddSingleton<ILocalDataService, LocalDataService>();
            services.Configuration<IBinaryFormatterStore>(s => { s.AddFormatter(typeof(LocalData<>), typeof(LocalDataFormatter<>)); });
        }
    }
}
