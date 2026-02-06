namespace ExtenderApp.Data
{
    /// <summary>
    /// 为 <see cref="MemoryBlock{T}"/> 提供获取与释放的抽象基类。
    /// </summary>
    /// <typeparam name="T">序列段中元素的类型。</typeparam>
    public abstract class MemoryBlockProviderBase<T> : DisposableObject
    {
        /// <summary>
        /// 获取一个序列段实例，提供至少 <paramref name="sizeHint"/> 个可用元素的空间（实现可选择遵守或作为建议）。
        /// 实现通常会从池中租用或新建一个 <see cref="MemoryBlock{T}"/>。
        /// </summary>
        /// <param name="sizeHint">所需的最小可用元素数（可为 0，表示无特定大小要求）。</param>
        /// <returns>一个可用的 <see cref="MemoryBlock{T}"/> 实例，不能为 <c>null</c>。</returns>
        public MemoryBlock<T> GetBlock(int sizeHint)
        {
            if (sizeHint < 0)
                throw new ArgumentOutOfRangeException(nameof(sizeHint), "获取序列段时，sizeHint 参数不能为负数。");
            return GetSegmentProtected(sizeHint);
        }

        /// <summary>
        /// 获取一个序列段实例的受保护方法，供派生类实现具体的获取逻辑。
        /// </summary>
        /// <param name="sizeHint">所需的最小可用元素数（可为 0，表示无特定大小要求）。</param>
        /// <returns>一个可用的 <see cref="MemoryBlock{T}"/> 实例，不能为 <c>null</c>。</returns>
        protected abstract MemoryBlock<T> GetSegmentProtected(int sizeHint);

        ///// <summary>
        ///// 释放先前由 <see cref="GetBlock"/> 获取的序列段，供实现将其返回池中或释放底层资源。
        ///// </summary>
        ///// <param name="sequenceSegment">要释放的序列段实例（不得为 <c>null</c>）。</param>
        //public void ReleaseSegment(SequenceSegmentBase<T> sequenceSegment)
        //{
        //    sequenceSegment.Reset();
        //    ReleaseSegmentProtected(sequenceSegment);
        //}

        ///// <summary>
        ///// 释放先前由 <see cref="GetBlock"/> 获取的序列段的受保护方法，供派生类实现具体的释放逻辑。
        ///// </summary>
        ///// <param name="sequenceSegment">需要被释放的序列段实例。</param>
        //protected abstract void ReleaseSegmentProtected(SequenceSegmentBase<T> sequenceSegment);
    }
}