

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个可释放资源的对象，该对象实现了IDisposable接口，并提供了一个属性来判断对象是否已经被释放。
    /// </summary>
    public interface IDisposableObject : IDisposable
    {
        /// <summary>
        /// 获取一个值，指示该对象是否已被释放。
        /// </summary>
        /// <value>
        /// 如果对象已被释放，则为 true；否则为 false。
        /// </value>
        public bool IsDisposed { get; }
    }
}
