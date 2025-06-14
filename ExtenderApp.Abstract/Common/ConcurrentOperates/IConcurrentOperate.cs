

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
        /// 判断是否可以执行某个操作。
        /// </summary>
        /// <returns>如果可以执行，则返回true；否则返回false。</returns>
        bool CanOperate { get; }

        /// <summary>
        /// 开始方法。
        /// </summary>
        void Start();
    }

    /// <summary>
    /// 表示一个并发操作的接口，它继承自 <see cref="IConcurrentOperate"/> 接口，并添加了两个泛型参数。
    /// </summary>
    /// <typeparam name="TData">表示操作类型的泛型参数，必须是一个类。</typeparam>
    /// <typeparam name="TData">表示数据类型的泛型参数，必须是一个类。</typeparam>
    public interface IConcurrentOperate<TData> : IConcurrentOperate
        where TData : class
    {
        /// <summary>
        /// 获取当前并发操作的数据。
        /// </summary>
        /// <returns>返回当前并发操作的数据。</returns>
        TData Data { get; }

        /// <summary>
        /// 开始执行操作
        /// </summary>
        /// <param name="data">要执行的操作</param>
        void Start(TData data);

        /// <summary>
        /// 执行一个并发操作。
        /// </summary>
        /// <param name="operation">要执行的并发操作对象。</param>
        void ExecuteOperation(IConcurrentOperation<TData> operation);

        /// <summary>
        /// 执行多个并发操作。
        /// </summary>
        /// <param name="operations">要执行的并发操作对象的集合。</param>
        void ExecuteOperation(IEnumerable<IConcurrentOperation<TData>> operations);

        /// <summary>
        /// 将一个并发操作加入队列中。
        /// </summary>
        /// <param name="operation">要加入队列的并发操作对象。</param>
        void QueueOperation(IConcurrentOperation<TData> operation);

        /// <summary>
        /// 将多个并发操作加入队列中。
        /// </summary>
        /// <param name="operations">要加入队列的并发操作对象的集合。</param>
        void QueueOperation(IEnumerable<IConcurrentOperation<TData>> operations);
    }
}
