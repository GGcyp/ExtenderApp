using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.ConcurrentOperates
{
    /// <summary>
    /// 并发操作池策略类，用于管理并发操作的池化。
    /// </summary>
    /// <typeparam name="T">并发操作的类型，必须实现ConcurrentOperation和IConcurrentOperation接口。</typeparam>
    internal class ConcurrentOperationPoolPolicy<T> : PooledObjectPolicy<T>
        where T : ConcurrentOperation, IConcurrentOperation
    {
        /// <summary>
        /// 创建并发操作的委托。
        /// </summary>
        private readonly Func<Action<IConcurrentOperation>, T> _createFunc;

        /// <summary>
        /// 初始化一个ConcurrentOperationPoolPolicy实例。
        /// </summary>
        /// <param name="createFunc">创建并发操作的委托。</param>
        public ConcurrentOperationPoolPolicy(Func<Action<IConcurrentOperation>, T> createFunc)
        {
            _createFunc = createFunc;
        }

        /// <summary>
        /// 创建一个新的并发操作实例。
        /// </summary>
        /// <returns>返回新创建的并发操作实例。</returns>
        public override T Create()
        {
            return _createFunc.Invoke(o => releaseAction?.Invoke((T)o));
        }

        /// <summary>
        /// 释放一个并发操作实例到池中。
        /// </summary>
        /// <param name="obj">要释放的并发操作实例。</param>
        /// <returns>如果成功释放对象，则返回true；否则返回false。</returns>
        public override bool Release(T obj)
        {
            return obj.TryReset();
        }
    }
}
