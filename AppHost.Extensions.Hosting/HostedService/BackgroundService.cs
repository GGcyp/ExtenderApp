namespace AppHost.Extensions.Hosting
{
    public abstract class BackgroundService : IHostedService, IDisposable
    {
        private Task m_ExecuteTask;
        private CancellationTokenSource? m_StoppingCts;

        public Task ExecuteTask => m_ExecuteTask;

        /// <summary>
        /// 当<see-cref=“IHostedService”/>启动时，会调用此方法。
        /// 实现应返回一个要在后台执行的任务。
        /// </summary>
        /// <param name="stoppingToken">标识任务已终止</param>
        /// <returns>需要执行的任务</returns>
        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            //创建链接的取消令牌源: 它允许你将多个取消令牌组合成一个单一的取消令牌源，这样可以简化对多个取消条件的处理。
            //统一取消逻辑: 当任何一个传入的取消令牌被触发时，新的取消令牌源会被取消。这对于需要同时处理多个取消条件的场景非常有用。
            m_StoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            m_ExecuteTask = ExecuteAsync(cancellationToken);

            // 如果任务已完成则返回它，这将向调用者传播取消和失败
            if (m_ExecuteTask.IsCompleted)
            {
                return m_ExecuteTask;
            }

            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            //停止调用而不启动
            if (m_ExecuteTask == null)
            {
                return;
            }

            try
            {
                //发送取消执行任务的消息
                m_StoppingCts!.Cancel();
            }
            finally
            {
                //等待任务完成或停止令牌被触发
                var tcs = new TaskCompletionSource<object>();
                using CancellationTokenRegistration registration = cancellationToken.Register(s => ((TaskCompletionSource<object>)s!).SetCanceled(), tcs);
                // 不要等待_executeTask，因为取消它将引发我们明确忽略的OperationCanceledException
                await Task.WhenAny(m_ExecuteTask, tcs.Task).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            m_StoppingCts?.Cancel();
        }
    }
}
