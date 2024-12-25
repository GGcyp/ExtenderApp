namespace ExtenderApp.Common.ObjectPools
{
    /// <summary>
    /// 通用层,默认对象池提供器。
    /// </summary>
    public class DefaultObjectPoolProvider : ObjectPoolProvider
    {
        /// <summary>
        /// 要保留在池中的最大对象数。
        /// </summary>
        public int MaximumRetained { get; set; }

        public DefaultObjectPoolProvider() : this(Environment.ProcessorCount * 2)
        {

        }

        public DefaultObjectPoolProvider(int maximumRetained)
        {
            MaximumRetained = maximumRetained;
        }

        public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy)
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            {
                return new DisposableObjectPool<T>(policy, MaximumRetained);
            }

            return new DefaultObjectPool<T>(policy, MaximumRetained);
        }
    }
}
