namespace AppHost.Extensions.Hosting
{
    /// <summary>
    /// 抽象基类BackgroundService，实现IHostedService和IDisposable接口。
    /// 用于实现需要在后台运行的服务的基类。
    /// </summary>
    public abstract class BackgroundService : IHostedService, IDisposable
    {
        /// <summary>
        /// 私有字段，存储取消令牌源。
        /// </summary>
        private CancellationTokenSource? stoppingCts;

        /// <summary>
        /// 获取当前正在执行的任务。
        /// </summary>
        public Task ExecuteTask { get; private set; }

        /// <summary>
        /// 当<see cref="IHostedService"/>启动时，会调用此方法。
        /// 实现应返回一个要在后台执行的任务。
        /// </summary>
        /// <param name="stoppingToken">标识任务已终止</param>
        /// <returns>需要执行的任务</returns>
        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        /// <summary>
        /// 启动服务，创建并执行任务。
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>任务对象</returns>
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            //创建链接的取消令牌源: 它允许你将多个取消令牌组合成一个单一的取消令牌源，这样可以简化对多个取消条件的处理。
            //统一取消逻辑: 当任何一个传入的取消令牌被触发时，新的取消令牌源会被取消。这对于需要同时处理多个取消条件的场景非常有用。
            stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            ExecuteTask = ExecuteAsync(cancellationToken);

            // 如果任务已完成则返回它，这将向调用者传播取消和失败
            if (ExecuteTask.IsCompleted)
            {
                return ExecuteTask;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 停止服务，取消正在执行的任务。
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>任务对象</returns>
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            //停止调用而不启动
            if (ExecuteTask == null)
            {
                return;
            }

            try
            {
                //发送取消执行任务的消息
                stoppingCts!.Cancel();
            }
            finally
            {
                //等待任务完成或停止令牌被触发
                var tcs = new TaskCompletionSource<object>();
                using CancellationTokenRegistration registration = cancellationToken.Register(s => ((TaskCompletionSource<object>)s!).SetCanceled(), tcs);
                // 不要等待_executeTask，因为取消它将引发我们明确忽略的OperationCanceledException
                await Task.WhenAny(ExecuteTask, tcs.Task).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            stoppingCts?.Cancel();
        }
    }
}
