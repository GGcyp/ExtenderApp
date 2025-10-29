using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 线程调度服务：封装跨线程（UI/后台）同步与异步调用，以及基于 await 的线程切换。
    /// 典型场景：从后台线程切回 UI 线程更新界面，或从 UI 线程切到后台执行密集型任务。
    /// </summary>
    public interface IDispatcherService
    {
        /// <summary>
        /// 在目标线程上“同步”执行委托，调用方会阻塞直至执行完成。
        /// 适合需要立即完成并获取副作用结果的场景。
        /// </summary>
        /// <param name="action">要执行的委托。</param>
        void Invoke(Action action);

        /// <summary>
        /// 在目标线程上“同步”执行带参数的委托，调用方会阻塞直至执行完成。
        /// </summary>
        /// <typeparam name="T">参数类型。</typeparam>
        /// <param name="action">要执行的委托。</param>
        /// <param name="send">传递给委托的参数。</param>
        void Invoke<T>(Action<T> action, T send);

        /// <summary>
        /// 使用 SynchronizationContext.Send 在目标线程上“同步”执行回调。
        /// </summary>
        /// <param name="callback">回调。</param>
        /// <param name="obj">传递给回调的状态对象。</param>
        void Invoke(SendOrPostCallback callback, object? obj);

        /// <summary>
        /// 在目标线程上“异步”执行委托，立即返回，不阻塞调用方。
        /// 适合更新 UI 等无需等待完成的场景。
        /// </summary>
        /// <param name="action">要执行的委托。</param>
        void BeginInvoke(Action action);

        /// <summary>
        /// 在目标线程上“异步”执行带参数的委托，立即返回，不阻塞调用方。
        /// </summary>
        /// <typeparam name="T">参数类型。</typeparam>
        /// <param name="action">要执行的委托。</param>
        /// <param name="send">传递给委托的参数。</param>
        void BeginInvoke<T>(Action<T> action, T send);

        /// <summary>
        /// 使用 SynchronizationContext.Post 在目标线程上“异步”执行回调，立即返回。
        /// </summary>
        /// <param name="callback">回调。</param>
        /// <param name="obj">传递给回调的状态对象。</param>
        void BeginInvoke(SendOrPostCallback callback, object? obj);

        /// <summary>
        /// 在目标线程上执行委托并返回结果的任务形式。
        /// 内部通常先切换到目标线程再执行 <paramref name="callback"/>。
        /// </summary>
        /// <typeparam name="TResult">返回结果类型。</typeparam>
        /// <param name="callback">要执行的委托。</param>
        /// <returns>包装结果的任务；若 <paramref name="callback"/> 为空则返回默认值。</returns>
        Task<TResult?> InvokeAsync<TResult>(Func<TResult> callback);

        /// <summary>
        /// 判断当前线程是否有权访问目标线程关联的资源（例如 UI 线程）。
        /// </summary>
        /// <returns>当前线程可访问则为 true，否则为 false。</returns>
        bool CheckAccess();

        /// <summary>
        /// 生成一个可等待对象，用于将后续代码切换到主线程（例如 UI 线程）执行。
        /// 用法：await ToMainThreadAsync(); // 之后的代码在主线程运行
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>可与 await 配合使用的可等待对象。</returns>
        ThreadSwitchAwaitable ToMainThreadAsync(CancellationToken token = default);

        /// <summary>
        /// 生成一个可等待对象，用于将后续代码切换到非主线程（后台线程/线程池）执行。
        /// 用法：await AwayMainThreadAsync(); // 之后的代码在后台线程运行
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>可与 await 配合使用的可等待对象。</returns>
        ThreadSwitchAwaitable AwayMainThreadAsync(CancellationToken token = default);
    }
}
