namespace ExtenderApp.Common
{
    /// <summary>
    /// 提供对 <see cref="IDisposable"/> 对象的扩展方法。
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
    }
}