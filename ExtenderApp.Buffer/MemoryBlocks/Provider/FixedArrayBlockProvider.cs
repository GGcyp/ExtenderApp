namespace ExtenderApp.Buffer.MemoryBlocks
{
    /// <summary>
    /// 提供基于固定托管数组的 <see cref="MemoryBlock{T}"/> 实例。 与 <see cref="ArrayPoolBlockProvider{T}"/> 不同，此提供者产生的内存块使用的数组不是来自 ArrayPool， 且内存块在运行时不可扩容（固定大小）。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    internal sealed class FixedArrayBlockProvider<T> : MemoryBlockProvider<T>
    {
        private static readonly Lazy<FixedArrayBlockProvider<T>> _default = new(static () => new());

        /// <summary>
        /// 获取默认的 <see cref="FixedArrayBlockProvider{T}"/> 单例实例（惰性初始化）。
        /// </summary>
        public static FixedArrayBlockProvider<T> Default => _default.Value;

        private readonly ObjectPool<FixedArrayMemoryBlock> _blockPool = ObjectPool.Create<FixedArrayMemoryBlock>();

        /// <summary>
        /// 获取或创建一个能够承载至少 <paramref name="sizeHint"/> 个元素的内存块。 如果 <paramref name="sizeHint"/> &gt; 0，会分配一个新的托管数组（非 ArrayPool）并包装为内存块；该内存块不可扩容。
        /// </summary>
        /// <param name="sizeHint">建议的最小容量（元素数）。为 0 表示可以返回空数组封装的内存块。</param>
        /// <returns>一个可写的 <see cref="MemoryBlock{T}"/> 实例（来自内部对象池）。</returns>
        protected override MemoryBlock<T> CreateBufferProtected(int sizeHint)
        {
            return GetBuffer(new T[sizeHint]);
        }

        /// <summary>
        /// 使用已有的托管数组创建一个包裹该数组但不可扩容的内存块。调用方负责数组的拥有权语义； 释放内存块不会归还或修改该数组。
        /// </summary>
        /// <param name="array">要包装的数组（不能为空）。</param>
        /// <returns>一个包裹指定数组的 <see cref="MemoryBlock{T}"/>（来自内部对象池）。</returns>
        public MemoryBlock<T> GetBuffer(T[] array)
        {
            return GetBuffer(new ArraySegment<T>(array));
        }

        /// <summary>
        /// 使用已有的托管数组的一部分创建一个包裹该数组分段但不可扩容的内存块。
        /// </summary>
        /// <param name="array">要包装的数组（不能为空）。</param>
        /// <param name="start">起始索引（从 0 开始）。</param>
        /// <param name="length">分段长度。</param>
        /// <returns>一个包裹指定数组分段的 <see cref="MemoryBlock{T}"/>（来自内部对象池）。</returns>
        public MemoryBlock<T> GetBuffer(T[] array, int start, int length)
        {
            return GetBuffer(new ArraySegment<T>(array, start, length));
        }

        /// <summary>
        /// 使用指定的 <see cref="ArraySegment{T}"/> 创建一个包裹该分段的不可扩容内存块。
        /// </summary>
        /// <param name="segment">要包装的数组分段（不能为空）。</param>
        /// <returns>一个包裹指定数组分段的 <see cref="MemoryBlock{T}"/>（来自内部对象池）。</returns>
        public MemoryBlock<T> GetBuffer(ArraySegment<T> segment)
        {
            var block = _blockPool.Get();
            block.Segment = segment;
            return block;
        }

        protected override sealed void ReleaseProtected(MemoryBlock<T> buffer)
        {
            if (buffer is FixedArrayMemoryBlock fixedBlock)
            {
                fixedBlock.Segment = default;
                _blockPool.Release(fixedBlock);
            }
            else
            {
                buffer.Dispose();
            }
        }

        /// <summary>
        /// 内部固定数组内存块实现：包装固定大小的托管数组，禁止扩容。 该类型实例由父提供者的对象池管理，不应从外部直接 new 用于长期持有。
        /// </summary>
        private sealed class FixedArrayMemoryBlock : MemoryBlock<T>
        {
            /// <summary>
            /// 被包装的数组分段（背后托管数组与有效范围）。
            /// </summary>
            public ArraySegment<T> Segment;

            /// <summary>
            /// 创建一个新的 <see cref="FixedArrayMemoryBlock"/> 实例（初始时未绑定任何数组分段）。
            /// </summary>
            public FixedArrayMemoryBlock()
            {
                Segment = default!;
            }

            /// <summary>
            /// 返回底层可用内存（固定不变），映射到当前 <see cref="Segment"/>。
            /// </summary>
            protected override sealed Memory<T> AvailableMemory => Segment;

            /// <summary>
            /// 固定数组内存块无法扩容：若请求的 <paramref name="sizeHint"/> 超过剩余可写空间，则抛出 <see cref="InvalidOperationException"/>。
            /// </summary>
            /// <param name="sizeHint">期望的最小可写元素数。</param>
            /// <exception cref="InvalidOperationException">始终在尝试扩容时抛出，因为底层数组为固定大小。</exception>
            protected override sealed void EnsureCapacityProtected(int sizeHint)
            {
                throw new InvalidOperationException("底层数组为固定大小，无法扩容以满足请求的写入空间。");
            }
        }
    }
}