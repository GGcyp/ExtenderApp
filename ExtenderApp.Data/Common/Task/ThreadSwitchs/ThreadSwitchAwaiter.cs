using System.Runtime.CompilerServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 自定义 Awaiter，用于支持 await 语法，实现线程切换或异步等待。
    /// 通过 ICriticalNotifyCompletion 接口，配合 ExtenderAwaitable 实现主线程/调度器切换。
    /// </summary>
    public readonly struct ThreadSwitchAwaiter : ICriticalNotifyCompletion
    {
        /// <summary>
        /// 取消令牌，用于支持异步操作的取消。
        /// </summary>
        private readonly CancellationToken _token;

        /// <summary>
        /// 标准回调，用于注册 await 后续操作（continuation）。
        /// </summary>
        private readonly Action<Action>? _callback;

        /// <summary>
        /// 非安全回调，用于注册 await 后续操作（continuation），跳过部分安全检查。
        /// </summary>
        private readonly Action<Action>? _unsafeCallback;

        /// <summary>
        /// 用于惰性评估 <see cref="IsCompleted"/> 的委托。
        /// 返回 true 表示当前已在目标上下文（如 UI 线程）且无需切换；
        /// 返回 false 表示需要通过调度器切换执行环境。
        /// 若为 null，则视为“未知/未完成”，<see cref="IsCompleted"/> 将返回 <see cref="_isCompleted"/>。
        /// </summary>
        private readonly Func<bool>? _isCompletedFunc;

        /// <summary>
        /// 静态判定完成标记。当未提供 <see cref="_isCompletedFunc"/> 时使用。
        /// true 表示 await 将同步继续（无需切换）；false 表示需要通过回调调度 continuation。
        /// </summary>
        private readonly bool _isCompleted;

        /// <summary>
        /// 是否已完成。true 表示无需挂起，await 后续代码可直接执行。
        /// 通常用于判断当前线程是否已在目标上下文（如主线程）。
        /// </summary>
        /// <remarks>
        /// 优先调用 <see cref="_isCompletedFunc"/> 动态判断；若其为 null，则返回 <see cref="_isCompleted"/> 的固定值。
        /// 请确保该属性的计算开销极低、且不抛出异常（编译器会在 await 热路径频繁访问）。
        /// </remarks>
        public bool IsCompleted => _isCompletedFunc == null ? _isCompleted : _isCompletedFunc.Invoke();

        /// <summary>
        /// 构造函数：使用动态判定委托决定是否已在目标上下文。
        /// 适合基于线程/调度器上下文进行实时判断的场景（例如：() =&gt; dispatcher.CheckAccess()）。
        /// </summary>
        /// <param name="callback">标准 continuation 调度回调（通常用于切换到目标线程/上下文）。</param>
        /// <param name="unsafeCallback">非安全 continuation 调度回调（可跳过部分安全检查）。</param>
        /// <param name="isCompletedFunc">动态判断是否已完成/无需切换的委托，返回 true 表示 await 同步继续。</param>
        /// <param name="token">取消令牌，将在 <see cref="GetResult"/> 处传播取消。</param>
        public ThreadSwitchAwaiter(Action<Action>? callback, Action<Action>? unsafeCallback, Func<bool>? isCompletedFunc, CancellationToken token)
        {
            _token = token;
            _callback = callback;
            _unsafeCallback = unsafeCallback;
            _isCompletedFunc = isCompletedFunc;
            _isCompleted = false;
        }

        /// <summary>
        /// 构造函数：使用固定布尔值决定是否已完成。
        /// 适合在创建时即可确定是否需要切换的简单场景。
        /// </summary>
        /// <param name="callback">标准 continuation 调度回调。</param>
        /// <param name="unsafeCallback">非安全 continuation 调度回调。</param>
        /// <param name="isCompleted">固定的完成状态；true 表示无需切换，await 同步继续。</param>
        /// <param name="token">取消令牌，将在 <see cref="GetResult"/> 处传播取消。</param>
        public ThreadSwitchAwaiter(Action<Action>? callback, Action<Action>? unsafeCallback, bool isCompleted, CancellationToken token)
        {
            _token = token;
            _callback = callback;
            _unsafeCallback = unsafeCallback;
            _isCompletedFunc = null;
            _isCompleted = isCompleted;
        }

        /// <summary>
        /// 获取异步操作结果。若取消则抛出异常。
        /// await 结束时自动调用。
        /// </summary>
        public void GetResult()
        {
            _token.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// 注册 continuation。即使已取消也要触发 continuation，
        /// 以便 await 恢复并在 GetResult 中抛出 OperationCanceledException。
        /// </summary>
        /// <param name="continuation">await 之后要继续执行的委托。</param>
        public void OnCompleted(Action continuation)
        {
            if (continuation is null) return;

            if (_token.IsCancellationRequested)
            {
                // 直接同步触发，让 await 立刻恢复到 GetResult 并抛出取消
                continuation();
                return;
            }

            if (_callback != null)
                _callback.Invoke(continuation);
            else
                continuation(); // 无调度器时退回同步执行，避免丢失 continuation
        }

        /// <summary>
        /// 注册 continuation（不安全版本）。逻辑与 <see cref="OnCompleted"/> 一致，
        /// 仅当提供了 <see cref="_unsafeCallback"/> 时使用之，否则退回到 <see cref="OnCompleted(Action)"/>。
        /// </summary>
        /// <param name="continuation">await 之后要继续执行的委托。</param>
        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation is null) return;

            if (_token.IsCancellationRequested)
            {
                continuation();
                return;
            }

            if (_unsafeCallback != null)
                _unsafeCallback.Invoke(continuation);
            else
                OnCompleted(continuation);
        }
    }
}
