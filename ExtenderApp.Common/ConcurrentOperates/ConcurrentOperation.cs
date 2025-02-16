using ExtenderApp.Abstract;

namespace ExtenderApp.Common
{
    public abstract class ConcurrentOperation : DisposableObject, IConcurrentOperation
    {
        private readonly Action<IConcurrentOperation> releaseAction;

        public ConcurrentOperation(Action<IConcurrentOperation> releaseAction)
        {
            this.releaseAction = releaseAction;
        }

        public void Release()
        {
            releaseAction?.Invoke(this);
        }

        public abstract bool TryReset();
    }

    /// <summary>
    /// 一个抽象的并发操作类，用于处理并发操作。
    /// </summary>
    /// <typeparam name="T">并发操作处理的数据类型。</typeparam>
    public abstract class ConcurrentOperation<T> : ConcurrentOperation, IConcurrentOperation<T> where T : class
    {
        protected ConcurrentOperation(Action<IConcurrentOperation> releaseAction) : base(releaseAction)
        {
        }

        /// <summary>
        /// 执行并发操作。
        /// </summary>
        /// <param name="item">需要处理的数据项。</param>
        public abstract void Execute(T item);
    }
}
