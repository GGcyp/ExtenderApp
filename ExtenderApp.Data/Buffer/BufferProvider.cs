namespace ExtenderApp.Data.Buffer
{
    /// <summary>
    /// 抽象缓冲区提供者：按需创建或提供泛型缓冲实例。
    /// </summary>
    /// <typeparam name="T">缓冲中元素的类型。</typeparam>
    /// <typeparam name="TBuffer">缓冲实例类型，必须继承自 <see cref="BufferBase{T}"/>。</typeparam>
    public abstract class BufferProvider<T, TBuffer> : DisposableObject
        where TBuffer : BufferBase<T>
    {
        /// <summary>
        /// 获取一个可用于写入的缓冲实例。
        /// </summary>
        /// <param name="sizeHint">期望的最小容量（以元素数计）。传 0 表示不作特殊建议。</param>
        /// <returns>
        /// 一个满足或尽量满足 <paramref name="sizeHint"/> 要求的 <typeparamref name="TBuffer"/> 实例。
        /// 实现可以返回新建实例或池中复用的实例，调用者应按实现约定负责后续的释放/回收操作。
        /// </returns>
        public TBuffer GetBuffer(int sizeHint = 0)
        {
            return CreateBufferProtected(sizeHint);
        }

        /// <summary>
        /// 派生类实现：创建或提供一个满足 <paramref name="sizeHint"/> 要求的缓冲实例。
        /// </summary>
        /// <param name="sizeHint">期望的最小容量（以元素数计）。传 0 表示不作特殊建议。</param>
        /// <returns>
        /// 一个 <typeparamref name="TBuffer"/> 实例，调用者将使用该实例进行写入并负责遵循库中关于生命周期的约定。
        /// </returns>
        protected abstract TBuffer CreateBufferProtected(int sizeHint);
    }
}