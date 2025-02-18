

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 并发操作接口
    /// </summary>
    public interface IConcurrentOperate : IDisposable, IResettable
    {
        /// <summary>
        /// 获取一个布尔值，指示当前是否正在执行操作。
        /// </summary>
        /// <value>
        /// 如果当前正在执行操作，则为 true；否则为 false。
        /// </value>
        bool IsExecuting { get; }

        /// <summary>
        /// 释放资源。
        /// </summary>
        void Release();
    }

    /// <summary>
    /// 表示一个并发操作的接口，它继承自 <see cref="IConcurrentOperate"/> 接口，并添加了两个泛型参数。
    /// </summary>
    /// <typeparam name="TOperate">表示操作类型的泛型参数，必须是一个类。</typeparam>
    /// <typeparam name="TData">表示数据类型的泛型参数，必须是一个类。</typeparam>
    public interface IConcurrentOperate<TOperate, TData> : IConcurrentOperate
        where TOperate : class
        where TData : class
    {
        /// <summary>
        /// 获取当前并发操作关联的数据对象。
        /// </summary>
        /// <returns>与当前并发操作关联的数据对象。</returns>
        TData Data { get; }

        /// <summary>
        /// 设置操作策略和数据
        /// </summary>
        /// <param name="policy">操作策略接口，用于定义并发操作的策略</param>
        /// <param name="data">要设置的数据</param>
        void SetPolicyAndData(IConcurrentOperatePolicy<TOperate, TData> policy, TData data);

        /// <summary>
        /// 执行一个并发操作。
        /// </summary>
        /// <param name="operation">要执行的并发操作对象。</param>
        void ExecuteOperation(IConcurrentOperation<TOperate> operation);

        /// <summary>
        /// 执行多个并发操作。
        /// </summary>
        /// <param name="operations">要执行的并发操作对象的集合。</param>
        void ExecuteOperation(IEnumerable<IConcurrentOperation<TOperate>> operations);

        /// <summary>
        /// 将一个并发操作加入队列中。
        /// </summary>
        /// <param name="operation">要加入队列的并发操作对象。</param>
        void QueueOperation(IConcurrentOperation<TOperate> operation);

        /// <summary>
        /// 将多个并发操作加入队列中。
        /// </summary>
        /// <param name="operations">要加入队列的并发操作对象的集合。</param>
        void QueueOperation(IEnumerable<IConcurrentOperation<TOperate>> operations);
    }
}
