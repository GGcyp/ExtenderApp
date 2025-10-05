

namespace AppHost.Extensions.Hosting
{
    /// <summary>
    /// 主机层，主机接口
    /// </summary>
    public interface IHost : IDisposable
    {
        /// <summary>
        /// 所有存储的服务
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// 启动所有注册的托管服务 <see cref="IHostedService" />
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 停止运行
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
