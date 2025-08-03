using AppHost.Extensions.DependencyInjection;

namespace AppHost.Extensions.Hosting
{
    public class Host : IHost
    {
        private readonly IServiceCollection _serviceDescriptors;

        private IServiceProvider m_ServiceProvider;
        private HostedServiceExecutor? m_hostedServiceExecutor;

        public IServiceProvider Service => m_ServiceProvider;

        public Host(IServiceCollection services)
        {
            _serviceDescriptors = services;
            m_ServiceProvider = _serviceDescriptors.BuildServiceProvider();

            m_hostedServiceExecutor = m_ServiceProvider.GetService<HostedServiceExecutor>();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (m_hostedServiceExecutor != null)
            {
                await m_hostedServiceExecutor.StartAsync(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (m_hostedServiceExecutor != null)
            {
                await m_hostedServiceExecutor.StopAsync(cancellationToken);
            }
        }

        public void Dispose()
        {
            m_hostedServiceExecutor?.Dispose();
            m_ServiceProvider = null;
        }
    }
}
