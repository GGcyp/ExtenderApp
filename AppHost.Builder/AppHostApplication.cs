using AppHost.Builder;
using AppHost.Extensions.Hosting;

namespace AppHost
{
    public class AppHostApplication : IHost
    {
        private readonly IHost _host;

        public IServiceProvider Service => _host.Service;

        public AppHostApplication(IHost host)
        {
            _host = host;
        }

        public static AppHostBuilder CreateBuilder()
        {
            return new AppHostBuilder();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
             await _host.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _host.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Run()
        {
            _host.Run();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
