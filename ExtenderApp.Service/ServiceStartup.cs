using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;

namespace ExtenderApp.Service
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

            AddRefreshService(services);
        }

        /// <summary>
        /// 添加刷新服务到服务集合中。
        /// </summary>
        /// <param name="services">服务集合。</param>
        private void AddRefreshService(IServiceCollection services)
        {
            services.AddHosted<RefreshServiceExecutor>();
            services.AddSingleton<RefreshStore>();
            services.AddSingleton<IRefreshService, RefreshService>();
        }
    }
}
