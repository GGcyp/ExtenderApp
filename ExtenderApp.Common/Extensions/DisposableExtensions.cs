namespace ExtenderApp.Common
{
    /// <summary>
    /// 提供对 <see cref="IDisposable"/> 与 <see cref="IAsyncDisposable"/> 对象的安全释放扩展方法。 这些方法在释放过程中会捕获并忽略异常，便于调用方在清理资源时不必重复编写 try/catch。
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// 安全地释放一个 <see cref="IDisposable"/> 对象，并忽略在释放过程中可能抛出的任何异常。
        /// </summary>
        /// <param name="disposable">要安全释放的可空 <see cref="IDisposable"/> 对象。</param>
        public static void DisposeSafe(this IDisposable? disposable)
        {
            try
            {
                disposable?.Dispose();
            }
            catch
            {
                // 忽略 Dispose 过程中发生的异常
            }
        }

        /// <summary>
        /// 异步地安全释放一个 <see cref="IDisposable"/> 对象。
        /// </summary>
        /// <param name="disposable">要安全释放的可空 <see cref="IDisposable"/> 对象。</param>
        /// <returns>
        /// 一个表示释放操作的 <see cref="ValueTask"/>。若 <paramref name="disposable"/> 为 <c>null</c>， 返回已完成的 <see cref="ValueTask"/>；否则在后台线程调用 <see
        /// cref="IDisposable.Dispose"/> 并以 <see cref="Task"/> 包装返回一个 <see cref="ValueTask"/>。
        /// </returns>
        /// <remarks>本方法不会抛出异常；如果释放过程中出现异常，会被捕获并忽略。 使用场景：当需要在异步流程中以非阻塞方式释放同步可释放对象时可使用此方法。</remarks>
        public static ValueTask DisposeSafeAsync(this IDisposable? disposable)
        {
            try
            {
                if (disposable == null)
                    return ValueTask.CompletedTask;

                return new(Task.Run(disposable.Dispose));
            }
            catch
            {
                // 忽略 Dispose 过程中发生的异常
                return ValueTask.CompletedTask;
            }
        }

        /// <summary>
        /// 异步地安全释放一个 <see cref="IAsyncDisposable"/> 对象，并忽略释放过程中可能抛出的异常。
        /// </summary>
        /// <param name="asyncDisposable">要安全释放的可空 <see cref="IAsyncDisposable"/> 对象。</param>
        /// <returns>一个表示异步释放操作的 <see cref="ValueTask"/>。若 <paramref name="asyncDisposable"/> 为 <c>null</c>， 返回已完成的 <see cref="ValueTask"/>。</returns>
        /// <remarks>若目标对象支持 <see cref="IAsyncDisposable.DisposeAsync"/>, 本方法将直接调用该方法并返回其结果。 本方法会捕获并忽略在释放过程中抛出的异常，保证调用方不必额外处理释放异常。</remarks>
        public static ValueTask DisposeSafeAsync(this IAsyncDisposable? asyncDisposable)
        {
            try
            {
                return asyncDisposable?.DisposeAsync() ?? ValueTask.CompletedTask;
            }
            catch
            {
                // 忽略 Dispose 过程中发生的异常
                return ValueTask.CompletedTask;
            }
        }
    }
}