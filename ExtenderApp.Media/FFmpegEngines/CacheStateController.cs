using ExtenderApp.Data;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 泛型状态控制器，用于管理状态的变更与同步。
    /// <para>提供了一种机制来监视状态变化，并阻塞当前线程直到状态达到预期的目标值。</para>
    /// <para>支持通过轮询（Polling）配合委托更新状态，适用于多线程环境下等待特定条件（如缓冲区可用）的场景。</para>
    /// </summary>
    /// <typeparam name="T">状态类型，需支持相等性比较。</typeparam>
    public class CacheStateController<T> : DisposableObject
    {
        /// <summary>
        /// 内部使用的等待句柄，用于线程同步信号（当前实现主要依赖轮询，此事件用于辅助通知）。
        /// </summary>
        private readonly AutoResetEvent _cacheAvailableEvent;

        /// <summary>
        /// 等待句柄数组，复用以减少分配。
        /// </summary>
        private WaitHandle[] waitHandles;

        /// <summary>
        /// 标志位，指示是否正在进行刷新/重置操作（此时应中断等待）。
        /// </summary>
        private volatile bool isFlushing;

        /// <summary>
        /// 获取当前的实际状态值。
        /// </summary>
        public T? State { get; private set; }

        /// <summary>
        /// 获取期望达到的目标状态值。
        /// </summary>
        public T? TargetState { get; private set; }

        /// <summary>
        /// 获取状态更新委托。
        /// <para>如果设置了此委托，控制器在等待过程中会定期调用它来获取最新状态并更新 <see cref="State"/>。</para>
        /// </summary>
        public Func<T>? UpdateStateFunc { get; private set; }

        /// <summary>
        /// 获取一个值，指示当前 <see cref="State"/> 是否已等于 <see cref="TargetState"/>。
        /// </summary>
        public bool IsInTargetState => EqualityComparer<T>.Default.Equals(State, TargetState);

        /// <summary>
        /// 初始化 <see cref="CacheStateController{T}"/> 类的新实例，状态和目标状态均为默认值。
        /// </summary>
        public CacheStateController() : this(default, default, default)
        {
        }

        /// <summary>
        /// 初始化 <see cref="CacheStateController{T}"/> 类的新实例，指定初始状态和目标状态。
        /// </summary>
        /// <param name="initialState">初始状态。</param>
        /// <param name="targetState">目标状态。</param>
        public CacheStateController(T initialState, T targetState) : this(initialState, targetState, null)
        {
        }

        /// <summary>
        /// 初始化 <see cref="CacheStateController{T}"/> 类的新实例，指定状态更新委托。
        /// </summary>
        /// <param name="updateFunc">用于获取最新状态的委托。</param>
        public CacheStateController(Func<T> updateFunc) : this(default, default, updateFunc)
        {
        }

        /// <summary>
        /// 初始化 <see cref="CacheStateController{T}"/> 类的新实例，指定目标状态和状态更新委托。
        /// </summary>
        /// <param name="targetState">目标状态。</param>
        /// <param name="updateFunc">用于获取最新状态的委托。</param>
        public CacheStateController(T? targetState, Func<T> updateFunc) : this(default, targetState, updateFunc)
        {
        }

        /// <summary>
        /// 初始化 <see cref="CacheStateController{T}"/> 类的新实例，指定所有参数。
        /// </summary>
        /// <param name="initialState">初始状态。</param>
        /// <param name="targetState">目标状态。</param>
        /// <param name="updateFunc">用于获取最新状态的委托。</param>
        public CacheStateController(T? initialState, T? targetState, Func<T>? updateFunc)
        {
            State = initialState;
            TargetState = targetState;
            UpdateStateFunc = updateFunc;
            _cacheAvailableEvent = new(false);
            waitHandles = new WaitHandle[2];
        }

        /// <summary>
        /// 阻塞当前线程，直到状态达到目标状态或操作被取消。
        /// <para>此方法采用轮询机制：定期调用 <see cref="Update"/> 刷新状态，并检查是否满足 <see cref="IsInTargetState"/>。</para>
        /// </summary>
        /// <param name="token">用于取消等待操作的取消令牌。</param>
        /// <param name="timeoutMs">轮询间隔时间（毫秒），即每次检查状态失败后的等待时间。</param>
        /// <returns>如果成功达到目标状态，则返回 <see langword="true"/>；如果操作被取消或需要刷新，则返回 <see langword="false"/>。</returns>
        public bool WaitForTargetState(CancellationToken token, int timeoutMs)
        {
            Update();

            while (!IsInTargetState &&
                !token.IsCancellationRequested &&
                !isFlushing)
            {
                waitHandles[0] = token.WaitHandle;
                waitHandles[1] = _cacheAvailableEvent;

                WaitHandle.WaitAny(waitHandles, timeoutMs);
                Update();
            }
            return !token.IsCancellationRequested && !isFlushing;
        }

        /// <summary>
        /// 手动触发状态刷新。
        /// <para>如果配置了 <see cref="UpdateStateFunc"/>，则调用该委托并将结果赋值给 <see cref="State"/>；否则保持当前状态不变。</para>
        /// </summary>
        public void Update()
        {
            State = UpdateStateFunc is null ? State : UpdateStateFunc.Invoke();
        }

        /// <summary>
        /// 直接更新当前的 <see cref="State"/> 为指定的新值。
        /// <para>如果更新后的状态达到了 <see cref="TargetState"/>，将触发内部信号唤醒等待线程。</para>
        /// </summary>
        /// <param name="newState">要设置的新状态值。</param>
        public void UpdateTargetState(T newState)
        {
            State = newState;
            if (IsInTargetState)
            {
                _cacheAvailableEvent.Set();
            }
        }

        /// <summary>
        /// 释放等待状态，通常用于中断阻塞（如停止解码时）。
        /// </summary>
        public void ReleaseWait()
        {
            isFlushing = true;
            _cacheAvailableEvent.Set();
        }

        /// <summary>
        /// 重置控制器状态，允许重新开始等待。
        /// </summary>
        public void Reset()
        {
            isFlushing = false;
            // 设置事件以确保任何滞留的等待线程能被唤醒并重新检查状态
            _cacheAvailableEvent.Set();
        }

        /// <summary>
        /// 释放托管资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            _cacheAvailableEvent.Dispose();
        }
    }
}