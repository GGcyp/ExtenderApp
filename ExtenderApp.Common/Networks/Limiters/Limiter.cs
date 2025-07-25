﻿
namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 限制器类
    /// </summary>
    public class Limiter : DisposableObject
    {
        /// <summary>
        /// 信号量，用于线程同步
        /// </summary>
        private readonly SemaphoreSlim _semaphore;

        private readonly ResourceLimitConfig _config;

        private readonly Func<ResourceLimitConfig, long> _getLimitFunc;

        /// <summary>
        /// 当前时间窗口内的令牌数
        /// </summary>
        private long _tokensInWindow;

        /// <summary>
        /// 当前时间窗口的开始时间
        /// </summary>
        private DateTime _windowStart;

        /// <summary>
        /// 限制值
        /// </summary>
        public long Limit => _getLimitFunc.Invoke(_config);

        /// <summary>
        /// 时间窗口
        /// </summary>
        public TimeSpan WindowTime => _config.RateWindow;

        /// <summary>
        /// 初始化速率限制器
        /// </summary>
        /// <param name="rateLimit">速率限制值</param>
        /// <param name="window">时间窗口</param>
        public Limiter(ResourceLimitConfig config, Func<ResourceLimitConfig, long> getLimitFunc)
        {
            _getLimitFunc = getLimitFunc;
            _config = config;
            _semaphore = new SemaphoreSlim(1, 1);
            _windowStart = DateTime.UtcNow;
        }

        /// <summary>
        /// 异步等待许可
        /// </summary>
        /// <param name="requiredTokens">所需的令牌数</param>
        /// <param name="ct">取消令牌</param>
        /// <returns>异步任务</returns>
        public void WaitForPermissionAsync(long requiredTokens, CancellationToken ct = default)
        {
            //_semaphore.WaitAsync(ct).Wait();

            //try
            //{
            //    // 如果有足够的令牌，立即返回
            //    while (_tokensInWindow + requiredTokens >= Limit)
            //    {
            //        Task.Delay(100, ct).Wait(); // 等待一段时间，直到有足够的令牌
            //    }

            //    // 重置窗口并授予许可
            //    _tokensInWindow += requiredTokens;
            //}
            //finally
            //{
            //    _semaphore.Release();
            //}
        }

        public void WaitForPermissionWithTimeoutAsync(long requiredTokens, CancellationToken ct = default)
        {
            //_semaphore.WaitAsync(ct).Wait();

            //try
            //{
            //    // 检查是否需要重置窗口
            //    var now = DateTime.UtcNow;
            //    if (now - _windowStart > WindowTime)
            //    {
            //        _windowStart = now;
            //        _tokensInWindow = 0;
            //    }

            //    // 如果有足够的令牌，立即返回
            //    if (_tokensInWindow + requiredTokens <= Limit)
            //    {
            //        _tokensInWindow += requiredTokens;
            //        return;
            //    }

            //    // 计算需要等待的时间
            //    double waitSeconds = (double)(_tokensInWindow + requiredTokens - Limit) / Limit * WindowTime.TotalSeconds;
            //    int waitMilliseconds = System.Math.Max(1, (int)(waitSeconds * 1000));

            //    // 等待窗口重置
            //    Task.Delay(waitMilliseconds, ct).Wait();

            //    // 重置窗口并授予许可
            //    _windowStart = DateTime.UtcNow;
            //    _tokensInWindow = requiredTokens;
            //}
            //finally
            //{
            //    _semaphore.Release();
            //}
        }

        /// <summary>
        /// 获取当前限流器速率。
        /// </summary>
        /// <returns>返回当前限流器速率，单位为每秒处理令牌数。</returns>
        public long GetCurrentLimiter()
        {
            //_semaphore.Wait();
            //try
            //{
            //    return Limit - _tokensInWindow;
            //}
            //finally
            //{
            //    _semaphore.Release();
            //}
            return Limit;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _semaphore.Dispose();
        }
    }
}
