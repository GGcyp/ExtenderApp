namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 调度器接口
    /// </summary>
    public interface IDispatcher
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
    }
}
