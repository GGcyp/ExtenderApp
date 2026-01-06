using ExtenderApp.Data;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 线程安全的数量缓存状态控制器（用于手动控制解码节奏）
    /// </summary>
    public class CacheCountStateController : DisposableObject
    {
        /// <summary>
        /// 默认等待超时时间（毫秒）
        /// </summary>
        private const int DefaultWaitTimeoutMs = 10;

        private readonly AutoResetEvent _cacheAvailableEvent; // 缓存可用信号（用于唤醒等待的解码线程）
        private WaitHandle[] waitHandles;//服用等待句柄数组，减少分配
        private volatile int maxCacheLength; // 最大缓存长度（如100帧）
        private volatile int currentCacheCount; // 当前缓存帧数量（线程安全访问）

        private volatile bool isFlushing;// 是否正在刷新缓存（如Seek时）

        /// <summary>
        /// 是否正在刷新缓存（如Seek时）
        /// </summary>
        public bool IsFlushing => isFlushing;

        public CacheCountStateController(int maxCacheLength)
        {
            this.maxCacheLength = maxCacheLength;
            currentCacheCount = 0;
            waitHandles = new WaitHandle[2];
            _cacheAvailableEvent = new(true); // 初始状态：缓存可用
        }

        /// <summary>
        /// 检查缓存是否有剩余空间（可继续解码）
        /// </summary>
        public bool HasCacheSpace => currentCacheCount < maxCacheLength;

        /// <summary>
        /// 阻塞当前线程，直到缓存中有可用空间或操作被取消。
        /// </summary>
        /// <param name="token">用于取消等待操作的令牌。</param>
        /// <param name="timeoutMs">单次等待的超时时间（毫秒）。</param>
        /// <returns>如果等待因取消请求而退出则返回 false，否则返回 true。</returns>
        public bool WaitForCacheSpace(CancellationToken token, int timeoutMs = DefaultWaitTimeoutMs)
        {
            if (IsFlushing)
                return true;

            // 当缓存已满且未请求取消时，进入等待循环
            while (!HasCacheSpace && !token.IsCancellationRequested)
            {
                // 更新等待句柄数组
                // waitHandles[0]: 缓存可用信号
                // waitHandles[1]: 取消信号
                waitHandles[0] = _cacheAvailableEvent;
                waitHandles[1] = token.WaitHandle;

                // 等待任意一个信号触发（缓存可用或取消），或者超时继续检查
                WaitHandle.WaitAny(waitHandles, timeoutMs);
            }

            // 返回是否因取消请求而退出（true表示已取消，false表示有缓存空间）
            return !token.IsCancellationRequested && !IsFlushing;
        }

        /// <summary>
        /// 缓存添加帧（更新计数并唤醒等待的解码线程）
        /// </summary>
        public void OnFrameAdded()
        {
            OnFrameAdded(1);
        }

        /// <summary>
        /// 缓存添加指定数量的帧
        /// </summary>
        /// <param name="count">添加的帧数</param>
        public void OnFrameAdded(int count)
        {
            // 线程安全地增加当前缓存计数
            Interlocked.Add(ref currentCacheCount, count);

            // 如果缓存仍有空间，设置信号量以允许生产者继续工作
            // 这确保了只要未满，WaitHandle就不会阻塞
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
            OnFrameRemoved(1);
        }

        /// <summary>
        /// 缓存移除指定数量的帧
        /// </summary>
        /// <param name="count">移除的帧数</param>
        public void OnFrameRemoved(int count)
        {
            // 检查并更新计数，防止减到负数
            if (currentCacheCount <= 0)
            {
                Interlocked.Exchange(ref currentCacheCount, 0);
            }
            else
            {
                Interlocked.Add(ref currentCacheCount, -count);
            }

            // 移除帧后空间释放，必然需要通知等待的生产者线程（如果有的话）
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
            maxCacheLength = newLength;
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
            maxCacheLength = newLength;
            currentCacheCount = 0;
            _cacheAvailableEvent.Set(); // 重置后允许解码
            isFlushing = false;
        }

        /// <summary>
        /// 释放当前等待
        /// </summary>
        public void ReleaseWait()
        {
            isFlushing = true;
            _cacheAvailableEvent.Set(); // 释放等待
        }

        protected override void DisposeManagedResources()
        {
            _cacheAvailableEvent.Dispose();
        }
    }
}