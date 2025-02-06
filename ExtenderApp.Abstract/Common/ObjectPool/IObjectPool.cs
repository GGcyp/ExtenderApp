
namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 对象池接口
    /// </summary>
    public interface IObjectPool
    {
        /// <summary>
        /// 对象池中还有多少对象
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 从池中获取一个对象（如果有），否则创建一个对象。
        /// </summary>
        /// <returns>返回对象</returns>
        object Get();

        /// <summary>
        /// 将对象返回到池中。
        /// </summary>
        /// <param name="obj">要返回的对象</param>
        void Release(object obj);
    }

    /// <summary>
    /// 泛型对象池接口
    /// </summary>
    /// <typeparam name="T">对象的类型</typeparam>
    public interface IObjectPool<T> : IObjectPool where T : class
    {
        /// <summary>
        /// 从池中获取一个对象（如果有），否则创建一个对象。
        /// </summary>
        /// <returns>返回对象</returns>
        new T Get();

        /// <summary>
        /// 将对象返回到池中。
        /// </summary>
        /// <param name="obj">要返回的对象</param>
        void Release(T obj);
    }


}
