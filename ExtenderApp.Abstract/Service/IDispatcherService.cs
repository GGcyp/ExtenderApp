namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 调度器接口
    /// </summary>
    public interface IDispatcherService
    {
        /// <summary>
        /// 同步调用指定操作。
        /// </summary>
        /// <param name="action">要执行的操作。</param>
        void Invoke(Action action);

        /// <summary>
        /// 异步调用指定操作。
        /// </summary>
        /// <param name="action">要执行的操作。</param>
        void BeginInvoke(Action action);

        /// <summary>
        /// 异步调度指定函数并在目标上下文中执行，返回表示操作的任务。
        /// </summary>
        /// <typeparam name="TResult">回调函数的返回类型。</typeparam>
        /// <param name="callback">要在调度器上下文中异步执行的函数。</param>
        /// <returns>
        /// 返回 <see cref="Task{TResult}"/>，其结果为回调函数的返回值。
        /// 如果回调抛出异常，任务状态将变为 Faulted。
        /// </returns>
        Task<TResult> InvokeAsync<TResult>(Func<TResult> callback);

        /// <summary>
        /// 检查当前线程是否具有访问调度器上下文的权限。
        /// </summary>
        /// <returns>
        /// 如果当前线程已关联到调度器上下文（如 UI 线程）则返回 true，
        /// 否则返回 false。
        /// </returns>
        bool CheckAccess();
    }
}
