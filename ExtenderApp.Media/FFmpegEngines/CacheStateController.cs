using ExtenderApp.Common.Threads;
using ExtenderApp.Data;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 线程安全的缓存状态控制器（用于手动控制解码节奏）
    /// </summary>
    public class CacheStateController : DisposableObject
    {
        /// <summary>
        /// 默认等待超时时间（毫秒）
        /// </summary>
        private const int DefaultWaitTimeoutMs = 10;

        private readonly AutoResetEvent _cacheAvailableEvent; // 缓存可用信号（用于唤醒等待的解码线程）
        private readonly Action<int, CancellationToken> _waitForCacheSpaceAction;// 异步等待委托
        private int maxCacheLength; // 最大缓存长度（如100帧）
        private int currentCacheCount; // 当前缓存帧数量（线程安全访问）

        public CacheStateController(int maxCacheLength)
        {
            this.maxCacheLength = maxCacheLength;
            currentCacheCount = 0;
            _cacheAvailableEvent = new(true); // 初始状态：缓存可用
            _waitForCacheSpaceAction = WaitForCacheSpace;
        }

        /// <summary>
        /// 检查缓存是否有剩余空间（可继续解码）
        /// </summary>
        public bool HasCacheSpace => Volatile.Read(ref currentCacheCount) < maxCacheLength;

        /// <summary>
        /// 同步等待，直到缓存中有可用空间。 此方法会阻塞当前线程，直到 <see cref="HasCacheSpace"/> 为 true 或取消请求被触发。
        /// </summary>
        /// <remarks>此方法适用于可以安全阻塞的后台解码线程。它会循环调用 <c>WaitOne</c>， 直到有信号通知缓存空间已释放或操作被取消。</remarks>
        /// <param name="timeoutMs">单次等待的超时时间（毫秒）。</param>
        /// <param name="cancellationToken">用于取消等待操作的令牌。</param>
        public void WaitForCacheSpace(int timeoutMs = DefaultWaitTimeoutMs, CancellationToken cancellationToken = default)
        {
            while (!HasCacheSpace && !cancellationToken.IsCancellationRequested)
            {
                // 等待信号，如果超时则继续循环检查
                _cacheAvailableEvent.WaitOne(timeoutMs, !cancellationToken.IsCancellationRequested);
            }
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

        protected override void DisposeManagedResources()
        {
            _cacheAvailableEvent.Dispose();
        }
    }
}