using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    public abstract class ConcurrentOperation : DisposableObject, IConcurrentOperation, ISelfReset
    {
        private Action<object> releaseAction;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release()
        {
            releaseAction?.Invoke(this);
        }

        public void SetReset(Action<object> action)
        {
            releaseAction = action;
        }

        public abstract bool TryReset();
    }

    /// <summary>
    /// 一个抽象的并发操作类，用于处理并发操作。
    /// </summary>
    /// <typeparam name="TData">并发操作处理的数据类型。</typeparam>
    public abstract class ConcurrentOperation<TData> : ConcurrentOperation, IConcurrentOperation<TData> where TData : class
    {
        /// <summary>
        /// 执行并发操作。
        /// </summary>
        /// <param name="item">需要处理的数据项。</param>
        public abstract void Execute(TData item);
    }
}
