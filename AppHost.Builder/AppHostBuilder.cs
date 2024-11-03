using AppHost.Builder.Extensions;
using AppHost.Extensions.Configuration;
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

        public AppHostBuilder()
        {
            Services = SericeBuilder.CreateServiceCollection();
            HostEnvironment = HostEnvironmentBuilder.CreateEnvironment();
            Properties = new Dictionary<object, object>();

            AddHostService();
        }

        /// <summary>
        /// 添加主机服务
        /// </summary>
        private void AddHostService()
        {
            this.AddHostedServiceExecutor();
        }

        public AppHostApplication Builde()
        {
            var host = new Host(Services);
            return new AppHostApplication(host);
        }
    }
}
