using AppHost.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost.Extensions.Hosting
{
    public class Host : IHost
    {
        private readonly IServiceCollection _serviceDescriptors;

        private HostedServiceExecutor? hostedServiceExecutor;

        public IServiceProvider ServiceProvider { get; private set; }

        public Host(IServiceCollection services)
        {
            _serviceDescriptors = services;
            ServiceProvider = _serviceDescriptors.BuildServiceProvider();
            hostedServiceExecutor = ServiceProvider.GetService<HostedServiceExecutor>();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (hostedServiceExecutor != null)
            {
                await hostedServiceExecutor.StartAsync(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (hostedServiceExecutor != null)
            {
                await hostedServiceExecutor.StopAsync(cancellationToken);
            }
        }

        public void Dispose()
        {
            hostedServiceExecutor?.Dispose();
            ServiceProvider = null;
        }
    }
}
