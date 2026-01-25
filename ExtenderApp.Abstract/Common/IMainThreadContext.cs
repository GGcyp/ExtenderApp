namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供主线程及其同步上下文的只读访问接口。 常用于跨组件获取 UI/宿主线程信息，以便进行线程检查与任务调度。
    /// </summary>
    /// <remarks>
    /// 在 WPF 中， <see cref="SynchronizationContext.Current"/> 一般会在建立消息循环后 （例如 Application.Run/OnStartup 阶段）被设置为
    /// <c>DispatcherSynchronizationContext</c>。 因此在应用启动的早期阶段，该上下文可能为 null，需在 UI 初始化后再获取。
    /// </remarks>
    public interface IMainThreadContext
    {
        /// <summary>
        /// 主线程对象（通常为 UI/Dispatcher 线程或宿主线程）。 可用于执行线程访问校验（例如与 <see cref="Thread.CurrentThread"/> 比较）。
        /// </summary>
        Thread? MainThread { get; }

        /// <summary>
        /// 主线程的同步上下文。用于将回调（continuation）投递到主线程执行， 可通过 <see cref="SynchronizationContext.Post(SendOrPostCallback, object)"/> 或 <see
        /// cref="SynchronizationContext.Send(SendOrPostCallback, object)"/> 进行异步/同步调度。 在 UI 初始化前可能为 null。
        /// </summary>
        SynchronizationContext? Context { get; }

        /// <summary>
        /// 初始化并捕获主线程与其 <see cref="SynchronizationContext"/>。
        /// </summary>
        /// <remarks>
        /// - 典型调用时机：在 WPF 的 <c>App.OnStartup</c> 或 UI 线程已建立 Dispatcher 之后调用。 <br/>
        /// - 期望行为：实现方应将 <see cref="Thread.CurrentThread"/> 作为 <see cref="MainThread"/>， 并读取 <see cref="SynchronizationContext.Current"/> 作为 <see
        /// cref="Context"/>。 <br/>
        /// - 幂等性：应设计为可重复调用且安全，重复调用不会破坏已捕获的上下文。 <br/>
        /// - 线程安全：应仅在目标主线程上调用；必要时实现方可忽略非主线程调用或抛出异常（由实现决定）。
        /// </remarks>
        void InitMainThreadContext();
    }
}