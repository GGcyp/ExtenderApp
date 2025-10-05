namespace ExtenderApp.Data
{
    /// <summary>
    /// 自定义 Awaitable 结构体，用于支持 await 语法，实现线程切换或异步等待。
    /// 通常与 <see cref="ExtenderThreadSwitchAwaiter"/> 配合使用，封装异步调度、主线程切换等场景。
    /// 典型用法：await dispatcher.ToMainThreadAsync(); // 之后的代码在线程上下文（如 UI 线程）执行
    /// </summary>
    /// <seealso cref="ExtenderThreadSwitchAwaiter"/>
    public readonly struct ExtenderThreadSwitchAwaitable
    {
        /// <summary>
        /// 取消令牌，用于支持异步操作的取消，在 <see cref="ExtenderThreadSwitchAwaiter.GetResult"/> 处传播取消。
        /// </summary>
        private readonly CancellationToken _token;

        /// <summary>
        /// 标准回调，用于注册 await 的 continuation，并将其调度到目标上下文执行（如主线程）。
        /// </summary>
        private readonly Action<Action>? _callback;

        /// <summary>
        /// 非安全回调，用于注册 await 的 continuation，允许跳过部分安全检查。
        /// </summary>
        private readonly Action<Action>? _unsafeCallback;

        /// <summary>
        /// 动态完成态判断委托。返回 true 表示当前已在目标上下文且无需切换；
        /// 返回 false 表示需要通过调度器切换执行环境。若为 null，则退回到 <see cref="_isCompleted"/> 的固定值。
        /// </summary>
        private readonly Func<bool>? _isCompletedFunc;

        /// <summary>
        /// 固定完成态标记。当未提供 <see cref="_isCompletedFunc"/> 时使用。
        /// true 表示 await 将同步继续（无需切换）；false 表示需要通过回调调度 continuation。
        /// </summary>
        private readonly bool _isCompleted;

        /// <summary>
        /// 构造函数（固定完成态版本），初始化 Awaitable。
        /// 适合在创建时即可确定是否需要切换的简单场景。
        /// </summary>
        /// <param name="callback">标准 continuation 调度回调。</param>
        /// <param name="unsafeCallback">非安全 continuation 调度回调。</param>
        /// <param name="isCompleted">固定完成态；true 表示无需切换，await 同步继续。</param>
        /// <param name="token">取消令牌。</param>
        public ExtenderThreadSwitchAwaitable(Action<Action>? callback, Action<Action>? unsafeCallback, bool isCompleted, CancellationToken token = default)
        {
            _token = token;
            _callback = callback;
            _unsafeCallback = unsafeCallback;
            _isCompleted = isCompleted;
            _isCompletedFunc = null;
        }

        /// <summary>
        /// 构造函数（动态完成态版本），初始化 Awaitable。
        /// 适合需要基于当前线程/上下文实时判断是否切换的场景（如：() =&gt; dispatcher.CheckAccess()）。
        /// </summary>
        /// <param name="callback">标准 continuation 调度回调。</param>
        /// <param name="unsafeCallback">非安全 continuation 调度回调。</param>
        /// <param name="isCompletedFunc">动态判断是否已在目标上下文的委托；true 表示无需切换。</param>
        /// <param name="token">取消令牌。</param>
        public ExtenderThreadSwitchAwaitable(Action<Action>? callback, Action<Action>? unsafeCallback, Func<bool>? isCompletedFunc, CancellationToken token = default)
        {
            _token = token;
            _callback = callback;
            _unsafeCallback = unsafeCallback;
            _isCompleted = false;
            _isCompletedFunc = isCompletedFunc;
        }

        /// <summary>
        /// 获取 Awaiter 实例，供 await 语法使用。
        /// </summary>
        /// <returns><see cref="ExtenderThreadSwitchAwaiter"/> 实例。</returns>
        /// <remarks>
        /// 若提供了 <see cref="_isCompletedFunc"/>，则走“动态判断”路径；否则使用固定完成态 <see cref="_isCompleted"/>。
        /// </remarks>
        public ExtenderThreadSwitchAwaiter GetAwaiter()
        {
            if (_isCompletedFunc != null)
                return new ExtenderThreadSwitchAwaiter(_callback, _unsafeCallback, _isCompletedFunc, _token);

            return new ExtenderThreadSwitchAwaiter(_callback, _unsafeCallback, _isCompleted, _token);
        }
    }
}
