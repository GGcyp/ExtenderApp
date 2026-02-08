using ExtenderApp.Data.Buffer.MemoryBlocks;

namespace ExtenderApp.Data.Buffer.Sequence
{
    /// <summary>
    /// 为序列缓冲段提供创建/封装逻辑的抽象工厂。
    /// </summary>
    /// <remarks>
    /// 该工厂通过内部的 <see cref="BufferProvider{T,MemoryBlock}"/> 获取底层的 <see cref="MemoryBlock{T}"/> 并将其转换为 <see
    /// cref="SequenceBufferSegment{T}"/>。派生类负责实现具体的段包装策略（见 <see cref="GetSegmentProtected(MemoryBlock{T})"/>）。
    /// </remarks>
    /// <typeparam name="T">段中元素的类型。</typeparam>
    public abstract class SequenceBufferSegmentProvider<T>
    {
        public static SequenceBufferSegmentProvider<T> Shared = MemoryBlockSegmentProvider<T>.Default;

        /// <summary>
        /// 内存块提供者，用于获取底层的 <see cref="MemoryBlock{T}"/> 实例以供封装成序列段。
        /// </summary>
        private readonly MemoryBlockProvider<T> _provider;

        /// <summary>
        /// 使用指定的缓冲工厂创建 <see cref="SequenceBufferSegmentProvider{T}"/> 实例。
        /// </summary>
        /// <param name="provider">用于提供底层 <see cref="MemoryBlock{T}"/> 的缓冲工厂，不能为 null。</param>
        protected SequenceBufferSegmentProvider(MemoryBlockProvider<T> provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 获取一个可以承载至少 <paramref name="sizeHint"/> 个元素的序列缓冲段。
        /// </summary>
        /// <param name="sizeHint">期望的最小可写元素数，传 0 表示不限。</param>
        /// <returns>一个包装好的 <see cref="SequenceBufferSegment{T}"/> 实例，派生实现可决定是否复用或新建。 返回的段必须与其底层 <see cref="MemoryBlock{T}"/> 的生命周期策略一致（例如归还到提供者或由调用方负责释放）。</returns>
        /// <remarks>
        /// 本方法先从内部的 <see cref="MemoryBlockProvider{T}"/> 获取一个 <see cref="MemoryBlock{T}"/>，然后调用 <see cref="GetSegmentProtected(MemoryBlock{T})"/>
        /// 将其转换为序列段。实现应保证返回的段与提供的 block 一一对应或按实现策略正确管理其生命周期。
        /// </remarks>
        public SequenceBufferSegment<T> GetSegment(int sizeHint)
        {
            var buffer = _provider.GetBuffer(sizeHint);
            return GetSegmentProtected(buffer);
        }

        /// <summary>
        /// 获取指定内存块对应的序列缓冲段。
        /// </summary>
        /// <param name="block">指定内存块</param>
        /// <returns>对应的序列缓冲段实例</returns>
        public SequenceBufferSegment<T> GetSegment(MemoryBlock<T> block)
        {
            return GetSegmentProtected(block);
        }

        /// <summary>
        /// 将由内部提供者返回的 <see cref="MemoryBlock{T}"/> 包装为 <see cref="SequenceBufferSegment{T}"/>。
        /// </summary>
        /// <param name="block">由 <see cref="_provider"/> 提供的 <see cref="MemoryBlock{T}"/>，不可为 null。</param>
        /// <returns>与 <paramref name="block"/> 对应的 <see cref="SequenceBufferSegment{T}"/> 实例。实现应明确段与底层 block 的生命周期关系， 并在适当时机进行资源回收或归还。</returns>
        protected abstract SequenceBufferSegment<T> GetSegmentProtected(MemoryBlock<T> block);
    }
}