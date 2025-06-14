
namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个并发操作接口，用于执行并发操作。
    /// </summary>
    public interface IConcurrentOperation : IResettable, IDisposable
    {
        /// <summary>
        /// 释放资源
        /// </summary>
        void Release();
    }

    /// <summary>
    /// 表示一个并发操作接口，该接口支持泛型数据参数。
    /// </summary>
    /// <typeparam name="TData">并发操作所需的数据类型。</typeparam>
    public interface IConcurrentOperation<TData> : IConcurrentOperation
    {
        /// <summary>
        /// 执行并发操作。
        /// </summary>
        /// <param name="data">执行操作所需的数据。</param>
        void Execute(TData data);
    }
}
