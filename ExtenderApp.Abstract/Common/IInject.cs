namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义对象注入能力的契约。
    /// </summary>
    public interface IInject
    {
        /// <summary>
        /// 将依赖/数据注入到指定对象实例中。
        /// </summary>
        /// <param name="obj">要被注入的目标对象实例。</param>
        void Inject(object obj);
    }

    /// <summary>
    /// 定义针对指定类型 <typeparamref name="T"/> 的注入能力契约。
    /// </summary>
    /// <typeparam name="T">要被注入的目标对象类型。</typeparam>
    public interface IInject<T> : IInject
    {
        /// <summary>
        /// 将依赖/数据注入到指定类型的对象实例中。
        /// </summary>
        /// <param name="obj">要被注入的目标对象实例。</param>
        void Inject(T obj);
    }
}