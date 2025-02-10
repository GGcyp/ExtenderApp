using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Files;
using ExtenderApp.Data;
using ExtenderApp.Service;

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
            services.AddSingleton<ITemporarilyService, TemporarilyService>();
            services.AddSingleton<ILogingService, LoggingService>();
            services.AddSingleton<IPathService, PathService>();
            services.AddSingleton<IScheduledTaskService, ScheduledTaskService>();

            AddModService(services);
            AddLocaDataService(services);
        }

        /// <summary>
        /// 向服务容器中添加Mod服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        private void AddModService(IServiceCollection services)
        {
            services.AddSingleton<IModService, ModService>();
            services.AddSingleton<ModStore>();
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
