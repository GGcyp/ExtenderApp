namespace ExtenderApp.Common.ObjectPool
{
    /// <summary>
    /// 通用层,定义一个可以重置类的接口
    /// </summary>
    public interface IResettable
    {
        /// <summary>
        /// 将对象重置
        /// </summary>
        /// <returns>如果对象可以重置自身为<see langword="true" />，否则<see langword="false" />.</returns>
        /// <remarks>
        /// 可能会有线程安全问题
        /// </remarks>
        bool TryReset();
    }
}
