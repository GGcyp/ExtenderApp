

namespace ExtenderApp.Common.ObjectPools
{
    /// <summary>
    /// 公共层,对象池策略
    /// </summary>
    public interface IPooledObjectPolicy<T> where T : notnull
    {
        /// <summary>
        /// 创建对象。
        /// </summary>
        /// <returns><see cref="{T}"/></returns>
        T Create();

        /// <summary>
        /// 将对象返回到池时运行一些处理。可用于重置对象的状态，并指示是否应将对象返回到池中。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Release(T obj);
    }
}
