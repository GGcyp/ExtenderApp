namespace AppHost.Extensions.Hosting
{
    /// <summary>
    /// 提供<see cref="IHost"/>的抽象方法
    /// </summary>
    public static class HostingAbstractionsHostExtensions
    {
        /// <summary>
        /// 同步启动
        /// </summary>
        /// <param name="host"></param>
        public static void Start(this IHost host)
        {
            host.StartAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 在超时的情况下关闭主机
        /// </summary>
        /// <param name="host"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task StopAsync(this IHost host, TimeSpan timeout)
        {
            using CancellationTokenSource cts = new CancellationTokenSource(timeout);
            await host.StopAsync(cts.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// 运行应用程序并阻止调用线程，直到主机关闭。
        /// </summary>
        /// <param name="host"></param>
        public static void Run(this IHost host)
        {
            host.RunAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 运行应用程序并返回一个<see cref=“Task”/>，该任务仅在触发令牌或触发关闭时完成。
        /// <paramref name=“host”/>实例在运行后被丢弃。
        /// </summary>
        /// <param name="host">要运行的<see cref=“IHost”/>。</param>
        /// <param name="token">触发关闭的令牌。</param>
        /// <returns>表示异步操作。</returns>
        public static async Task RunAsync(this IHost host, CancellationToken token = default)
        {
            try
            {
                await host.StartAsync(token).ConfigureAwait(false);
            }
            finally
            {
                if (host is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    host.Dispose();
                }
            }
        }
    }
}
