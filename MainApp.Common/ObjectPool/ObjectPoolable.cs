namespace MainApp.Common.ObjectPool
{
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
