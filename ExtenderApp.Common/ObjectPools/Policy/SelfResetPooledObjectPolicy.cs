using ExtenderApp.Abstract;

namespace ExtenderApp.Common.ObjectPools
{
    /// <summary>
    /// 自重置对象池策略类
    /// </summary>
    /// <typeparam name="T">需要池化的对象类型，必须是实现了 ISelfReset 接口的类</typeparam>
    public class SelfResetPooledObjectPolicy<T> : PooledObjectPolicy<T> where T : class, ISelfReset, new()
    {
        /// <summary>
        /// 创建一个新的对象实例
        /// </summary>
        /// <returns>返回创建的对象实例</returns>
        public override T Create()
        {
            var result = new T();
            result.SetReset(o => releaseAction.Invoke((T)o));
            return result;
        }

        /// <summary>
        /// 释放对象到对象池中
        /// </summary>
        /// <param name="obj">需要释放的对象</param>
        /// <returns>如果对象成功重置则返回 true，否则返回 false</returns>
        public override bool Release(T obj)
        {
            return obj.TryReset();
        }
    }
}
