namespace ExtenderApp.Data
{
    /// <summary>
    /// 可池化的对象类，用于管理对象的创建和释放，以便重用对象以减少内存分配和垃圾回收的开销。
    /// </summary>
    /// <typeparam name="T">要池化的对象类型，必须是类类型，并且必须有一个无参数的构造函数。</typeparam>
    public class ObjectPoolable<T> where T : class,new()
    {
        /// <summary>
        /// 对象池实例（私有静态字段）
        /// </summary>
        private static ObjectPool<T> objPool;

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <returns>返回从对象池中获取的对象，如果对象池为空则创建一个新的对象池</returns>
        public static T GetObjectForPool()
        {
            if(objPool == null)
            {
                objPool = ObjectPool.Create<T>();
            }
            return objPool.Get();
        }

        /// <summary>
        /// 将对象释放回对象池
        /// </summary>
        /// <param name="item">要释放回对象池的对象</param>
        public static void SetObjectToPool(T item)
        {
            if (objPool == null)
            {
                objPool = ObjectPool.Create<T>();
            }
            objPool.Release(item);
        }
    }
}
