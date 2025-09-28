using ExtenderApp.Common;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// 线程安全的缓存状态控制器（用于手动控制解码节奏）
    /// </summary>
    public class CacheStateController : DisposableObject
    {
        private readonly AutoResetEvent _cacheAvailableEvent; // 缓存可用信号（用于唤醒等待的解码线程）
        private int maxCacheLength; // 最大缓存长度（如100帧）
        private int currentCacheCount; // 当前缓存帧数量（线程安全访问）

        public CacheStateController(int maxCacheLength)
        {
            this.maxCacheLength = maxCacheLength;
            currentCacheCount = 0;
            _cacheAvailableEvent = new(true); // 初始状态：缓存可用
        }

        /// <summary>
        /// 检查缓存是否有剩余空间（可继续解码）
        /// </summary>
        public bool HasCacheSpace => Volatile.Read(ref currentCacheCount) < maxCacheLength;

        /// <summary>
        /// 等待缓存空间（非阻塞，支持取消）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="timeoutMs">单次等待超时时间（避免死等）</param>
        /// <returns>true：缓存有空间；false：等待超时或被取消</returns>
        public bool WaitForCacheSpace(CancellationToken cancellationToken, int timeoutMs = 50)
        {
            // 循环等待：直到缓存有空间、被取消或超时
            while (!HasCacheSpace && !cancellationToken.IsCancellationRequested)
            {
                // 等待信号：超时50ms后重试（避免线程长期阻塞）
                if (!_cacheAvailableEvent.WaitOne(timeoutMs, !cancellationToken.IsCancellationRequested))
                {
                    continue; // 超时后继续轮询
                }
            }
            return HasCacheSpace && !cancellationToken.IsCancellationRequested;
        }

        /// <summary>
        /// 缓存添加帧（更新计数并唤醒等待的解码线程）
        /// </summary>
        public void OnFrameAdded()
        {
            Interlocked.Increment(ref currentCacheCount);
            // 若缓存仍有空间，继续通知（可选，避免频繁唤醒）
            if (HasCacheSpace)
            {
                _cacheAvailableEvent.Set();
            }
        }

        /// <summary>
        /// 缓存移除帧（更新计数并唤醒等待的解码线程）
        /// </summary>
        public void OnFrameRemoved()
        {
            if (currentCacheCount <= 0)
            {
                Interlocked.Exchange(ref currentCacheCount, 0);
            }
            else
            {
                Interlocked.Decrement(ref currentCacheCount);
            }

            // 缓存有空间了，唤醒解码线程继续解码
            _cacheAvailableEvent.Set();
        }

        /// <summary>
        /// 更新最大缓存长度
        /// </summary>
        /// <param name="newLength">新的最大缓存长度</param>
        public void UpdateMaxCacheLength(int newLength)
        {
            if (newLength <= 0)
            {
                //throw new ArgumentOutOfRangeException(nameof(newLength), "最大缓存长度必须大于0");
                return;
            }
            Volatile.Write(ref maxCacheLength, newLength);
            // 如果当前缓存小于新长度，唤醒等待的解码线程
            if (HasCacheSpace)
            {
                _cacheAvailableEvent.Set();
            }
        }

        /// <summary>
        /// 重置缓存状态（如Seek或停止时）
        /// </summary>
        public void Reset()
        {
            Reset(maxCacheLength);
        }

        /// <summary>
        /// 重置缓存状态并设置新的最大缓存长度
        /// </summary>
        /// <param name="newLength">新的最大缓存长度</param>
        public void Reset(int newLength)
        {
            if (newLength <= 0)
            {
                //throw new ArgumentOutOfRangeException(nameof(newLength), "最大缓存长度必须大于0");
                return;
            }
            Volatile.Write(ref maxCacheLength, newLength);
            Volatile.Write(ref currentCacheCount, 0);
            _cacheAvailableEvent.Set(); // 重置后允许解码
        }

        protected override void Dispose(bool disposing)
        {
            _cacheAvailableEvent.Dispose();
        }
    }
}