using System.Collections.Concurrent;


namespace ExtenderApp.Common.Networks
{
    public class ResourceLimiter : IDisposable
    {
        private readonly ResourceLimitConfig _config;
        private readonly Limiter _sendRateLimiter;
        private readonly Limiter _receiveRateLimiter;
        private readonly ScheduledTask _scheduledTask;
        private readonly SemaphoreSlim _memorySemaphore;
        private long _currentMemoryUsage;
        private bool _disposed;

        /// <summary>
        /// 每秒资源使用统计事件
        /// </summary>
        public event Action<ResourceStats>? OnStatsUpdated;

        public ResourceLimiter(ResourceLimitConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sendRateLimiter = new Limiter(config, c => c.MaxSendRateBytesPerSecond);
            _receiveRateLimiter = new Limiter(config, c => c.MaxTotalMemoryBytes);
            _memorySemaphore = new SemaphoreSlim(1, 1);

            // 启动统计报告定时器
            _scheduledTask = new();
            _scheduledTask.Start(ReportStats, config.StatsReportInterval, config.StatsReportInterval);
        }

        #region 内存管理

        /// <summary>
        /// 申请内存使用权限
        /// </summary>
        public async Task WaitForMemoryAsync(long bytes, CancellationToken ct = default)
        {
            await _memorySemaphore.WaitAsync(ct);

            try
            {
                // 等待直到有足够的内存
                while (_currentMemoryUsage + bytes > _config.MaxTotalMemoryBytes)
                {
                    await Task.Delay(100, ct); // 短暂等待后重试
                }

                Interlocked.Add(ref _currentMemoryUsage, bytes);
            }
            finally
            {
                _memorySemaphore.Release();
            }
        }

        /// <summary>
        /// 释放内存
        /// </summary>
        public void ReleaseMemory(long bytes)
        {
            Interlocked.Add(ref _currentMemoryUsage, -bytes);

            // 确保内存使用不会变为负数
            if (Volatile.Read(ref _currentMemoryUsage) < 0)
            {
                Interlocked.Exchange(ref _currentMemoryUsage, 0);
            }
        }

        #endregion

        #region 流量控制

        /// <summary>
        /// 等待发送许可
        /// </summary>
        public Task WaitForSendPermissionAsync(long bytes, CancellationToken ct = default)
        {
            return _sendRateLimiter.WaitForPermissionAsync(bytes, ct);
        }

        /// <summary>
        /// 等待接收许可
        /// </summary>
        public Task WaitForReceivePermissionAsync(long bytes, CancellationToken ct = default)
        {
            return _receiveRateLimiter.WaitForPermissionAsync(bytes, ct);
        }

        #endregion

        #region 统计信息

        private void ReportStats()
        {
            try
            {
                var stats = new ResourceStats
                {
                    CurrentMemoryUsage = Volatile.Read(ref _currentMemoryUsage),
                    SendBytesPerSecond = _sendRateLimiter.GetCurrentRate(),
                    ReceiveBytesPerSecond = _receiveRateLimiter.GetCurrentRate(),
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
                CurrentMemoryUsage = Volatile.Read(ref _currentMemoryUsage),
                SendBytesPerSecond = _sendRateLimiter.GetCurrentRate(),
                ReceiveBytesPerSecond = _receiveRateLimiter.GetCurrentRate(),
                Timestamp = DateTime.UtcNow
            };
        }

        #endregion

        #region IDisposable 实现

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _scheduledTask.Dispose();
                    _memorySemaphore.Dispose();
                    _sendRateLimiter.Dispose();
                    _receiveRateLimiter.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
