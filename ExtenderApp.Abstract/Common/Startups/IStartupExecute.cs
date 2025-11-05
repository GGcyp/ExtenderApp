

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 启动执行接口。实现者用于在应用启动阶段执行一次性或可重复的异步初始化/预热逻辑（例如注册、资源加载、缓存预热等）。
    /// </summary>
    /// <remarks>
    /// 实现类应负责在 <see cref="ExecuteAsync"/> 中执行异步启动任务，并在不再需要时通过 <see cref="Dispose"/> 或 <see cref="DisposeAsync"/> 释放占用的资源。
    /// 若实现需要支持取消，请在内部使用取消令牌；若可能被多次调用，应保证幂等性或由调用方保证只调用一次。
    /// </remarks>
    public interface IStartupExecute : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// 执行启动任务的异步方法。调用方应等待返回的 <see cref="ValueTask"/> 以确保启动逻辑已完成。
        /// </summary>
        /// <returns>
        /// 表示异步启动操作完成的 <see cref="ValueTask"/>。对于短期或低分配场景，使用 <see cref="ValueTask"/> 有助于减少分配开销。
        /// </returns>
        ValueTask ExecuteAsync();
    }
}