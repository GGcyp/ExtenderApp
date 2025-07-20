using System.Collections.Concurrent;
using System.Diagnostics;


namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 资源限制器类，用于控制资源的发送和接收速率，并监控资源使用情况。
    /// </summary>
    public class ResourceLimiter : DisposableObject
    {
        /// <summary>
        /// 资源限制配置对象。
        /// </summary>
        private readonly ResourceLimitConfig _config;

        /// <summary>
        /// 发送速率限制器。
        /// </summary>
        private readonly Limiter _sendRateLimiter;

        /// <summary>
        /// 接收速率限制器。
        /// </summary>
        private readonly Limiter _receiveRateLimiter;

        /// <summary>
        /// 内存限制器实例
        /// </summary>
        private readonly Limiter _memotyLimiter;

        /// <summary>
        /// 计划任务对象，用于定期报告资源使用情况。
        /// </summary>
        private readonly ScheduledTask _scheduledTask;

        /// <summary>
        /// 每秒资源使用统计事件，当资源使用情况更新时触发。
        /// </summary>
        /// <remarks>
        /// 触发时，事件处理程序将接收到一个<see cref="ResourceStats"/>对象，表示当前的资源使用情况。
        /// </remarks>
        public event Action<ResourceStats>? OnStatsUpdated;

        public Action<long> ReleaseMemoryAction { get; }

        /// <summary>
        /// 初始化<see cref="ResourceLimiter"/>类的新实例。
        /// </summary>
        /// <param name="config">资源限制配置对象。</param>
        /// <exception cref="ArgumentNullException"><paramref name="config"/>为<c>null</c>。</exception>
        public ResourceLimiter(ResourceLimitConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sendRateLimiter = new Limiter(config, c => c.MaxSendRateBytesPerSecond);
            _receiveRateLimiter = new Limiter(config, c => c.MaxReceiveRateBytesPerSecond); // 修正：使用MaxReceiveRateBytesPerSecond
            _memotyLimiter = new Limiter(config, c => c.MaxTotalMemoryBytes);
            ReleaseMemoryAction = ReleaseMemory;

            // 启动统计报告定时器
            _scheduledTask = new ScheduledTask();
            _scheduledTask.Start(ReportStats, config.StatsReportInterval, config.StatsReportInterval);
        }

        #region 内存管理

        /// <summary>
        /// 申请内存使用权限
        /// </summary>
        public void WaitForMemoryAsync(long bytes, CancellationToken ct = default)
        {
            if (bytes > _config.MaxTotalMemoryBytes)
            {
                throw new InvalidOperationException($"请求的内存大小 {bytes} 超过了最大限制 {_config.MaxTotalMemoryBytes}");
            }

            _memotyLimiter.WaitForPermissionAsync(bytes, ct);
        }

        /// <summary>
        /// 释放内存
        /// </summary>
        public void ReleaseMemory(long bytes)
        {
            _memotyLimiter.WaitForPermissionAsync(-bytes, default);
        }

        #endregion

        #region 流量控制

        /// <summary>
        /// 等待发送许可
        /// </summary>
        public void WaitForSendPermissionAsync(long bytes, CancellationToken ct = default)
        {
            if (bytes > _config.MaxSendRateBytesPerSecond)
            {
                throw new InvalidOperationException($"申请的发送字节数 {bytes} 超过了最大发送速率 {_config.MaxSendRateBytesPerSecond} 字节/秒。");
            }

            _sendRateLimiter.WaitForPermissionWithTimeoutAsync(bytes, ct);
        }

        /// <summary>
        /// 等待接收许可
        /// </summary>
        public void WaitForReceivePermissionAsync(long bytes, CancellationToken ct = default)
        {
            if (bytes > _config.MaxReceiveRateBytesPerSecond)
            {
                throw new InvalidOperationException($"申请的接收字节数 {bytes} 超过了最大接收速率 {_config.MaxReceiveRateBytesPerSecond} 字节/秒。");
            }

            _receiveRateLimiter.WaitForPermissionWithTimeoutAsync(bytes, ct);
        }

        #endregion

        #region 统计信息

        private void ReportStats()
        {
            try
            {
                var stats = new ResourceStats
                {
                    CurrentMemoryUsage = _memotyLimiter.GetCurrentLimiter(),
                    SendBytesPerSecond = _sendRateLimiter.GetCurrentLimiter(),
                    ReceiveBytesPerSecond = _receiveRateLimiter.GetCurrentLimiter(),
                    Timestamp = DateTime.UtcNow
                };

                OnStatsUpdated?.Invoke(stats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"统计报告出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取当前资源使用情况
        /// </summary>
        public ResourceStats GetCurrentStats()
        {
            return new ResourceStats
            {
                CurrentMemoryUsage = _memotyLimiter.GetCurrentLimiter(),
                SendBytesPerSecond = _sendRateLimiter.GetCurrentLimiter(),
                ReceiveBytesPerSecond = _receiveRateLimiter.GetCurrentLimiter(),
                Timestamp = DateTime.UtcNow
            };
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _scheduledTask.Dispose();
            _sendRateLimiter.Dispose();
            _receiveRateLimiter.Dispose();
        }
    }
}
