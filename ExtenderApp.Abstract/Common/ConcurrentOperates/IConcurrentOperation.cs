
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
    /// 定义一个并发操作接口，用于执行并发操作。
    /// </summary>
    /// <typeparam name="T">操作对象的类型。</typeparam>
    public interface IConcurrentOperation<T> : IConcurrentOperation
    {
        /// <summary>
        /// 执行并发操作。
        /// </summary>
        /// <param name="item">要执行操作的对象。</param>
        void Execute(T item);
    }
}
