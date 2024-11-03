namespace AppHost.Extensions.Hosting
{
    /// <summary>
    /// 托管服务启动项
    /// </summary>
    public sealed class HostedServiceExecutor : IDisposable
    {
        private readonly IEnumerable<IHostedService> _services;

        public HostedServiceExecutor(IEnumerable<IHostedService> services)
        {
            _services = services;
        }

        public async Task StartAsync(CancellationToken token)
        {
            foreach (var service in _services)
            {
                await service.StartAsync(token);
            }
        }

        public async Task StopAsync(CancellationToken token)
        {
            List<Exception>? exceptions = null;

            foreach (var service in _services)
            {
                try
                {
                    await service.StopAsync(token);
                }
                catch (Exception ex)
                {
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }

            // 如果存在异常，则触发
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }

        public void Dispose()
        {
            List<Exception>? exceptions = null;

            foreach (var service in _services)
            {
                try
                {
                    service.Dispose();
                }
                catch (Exception ex)
                {
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }

            // 如果存在异常，则触发
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
