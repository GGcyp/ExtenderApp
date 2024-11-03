
namespace AppHost.Extensions.Hosting
{
    /// <summary>
    /// 托管服务接口
    /// </summary>
    public interface IHostedService : IDisposable
    {
        /// <summary>
        /// 执行托管线程
        /// </summary>
        Task ExecuteTask { get; }

        /// <summary>
        /// 启动服务。
        /// </summary>
        /// <param name="cancellationToken">启动任务时的取消令牌。</param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="cancellationToken">停止任务时的取消令牌。</param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
