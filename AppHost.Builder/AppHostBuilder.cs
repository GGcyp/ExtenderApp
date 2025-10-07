using AppHost.Builder.Extensions;
using AppHost.Extensions.DependencyInjection;
using AppHost.Extensions.Hosting;

namespace AppHost.Builder
{
    /// <summary>
    /// 主机创建者类
    /// </summary>
    public class AppHostBuilder : IHostApplicationBuilder
    {
        public IDictionary<object, object> Properties { get; }

        public IServiceCollection Services { get; }

        public IConfiguration Configuration => throw new NotImplementedException();

        public IHostEnvironment HostEnvironment { get; }

        public IMainThreadContext MainThreadContext { get; }

        public AppHostBuilder()
        {
            Services = ServiceBuilder.CreateServiceCollection();
            HostEnvironment = HostEnvironmentBuilder.CreateEnvironment();
            MainThreadContext = HostEnvironmentBuilder.CreateMainThreadContext();
            Properties = new Dictionary<object, object>();

            AddHostService();
        }

        /// <summary>
        /// 添加主机服务
        /// </summary>
        private void AddHostService()
        {
            Services.AddSingleton(Services);
            Services.AddSingleton(HostEnvironment);
            Services.AddSingleton(MainThreadContext);

            this.AddHostedServiceExecutor();
            this.AddScopeExecutor();
            Services.AddTentativeProvider();
        }

        public AppHostApplication Builde()
        {
            var host = new Host(Services);
            return new AppHostApplication(host);
        }
    }
}
