using System.Buffers;

namespace ExtenderApp.Data.Buffer.MemoryBlocks
{
    /// <summary>
    /// 基于数组池的内存块提供者，用于创建或复用 <see cref="MemoryBlock{T}"/> 实例。
    /// </summary>
    /// <typeparam name="T">内存块中元素的类型。</typeparam>
    internal sealed class ArrayPoolBlockProvider<T> : MemoryBlockProvider<T>
    {
        private static readonly Lazy<ArrayPoolBlockProvider<T>> _default = new(static () => new());
        public static ArrayPoolBlockProvider<T> Default = _default.Value;

        private readonly ObjectPool<ArrayPoolMemoryBlock> _blockPool = ObjectPool.Create<ArrayPoolMemoryBlock>();

        private readonly ArrayPool<T> _arrayPool;

        /// <summary>
        /// 使用共享的 <see cref="ArrayPool{T}"/> 创建 <see cref="ArrayPoolBlockProvider{T}"/> 实例。
        /// </summary>
        public ArrayPoolBlockProvider() : this(ArrayPool<T>.Shared)
        {
        }

        /// <summary>
        /// 使用指定的 <see cref="ArrayPool{T}"/> 创建 <see cref="ArrayPoolBlockProvider{T}"/> 实例。
        /// </summary>
        /// <param name="arrayPool">用于租用/归还数组的数组池，不能为空。</param>
        public ArrayPoolBlockProvider(ArrayPool<T> arrayPool)
        {
            _arrayPool = arrayPool;
        }

        /// <summary>
        /// 从内部对象池获取一个可写的 <see cref="MemoryBlock{T}"/> 实例并初始化为可从数组池租用。
        /// </summary>
        /// <param name="sizeHint">建议的最小容量（元素数）。实现可以忽略或用于选择合适大小的数组。</param>
        /// <returns>一个来自对象池且已配置为从数组池租用底层数组的 <see cref="MemoryBlock{T}"/> 实例。</returns>
        protected override MemoryBlock<T> CreateBufferProtected(int sizeHint)
        {
            var block = _blockPool.Get();
            block.arrayPool = _arrayPool;
            block.BlockPool = _blockPool;
            return block;
        }

        /// <summary>
        /// 数组池内存块的具体实现：它维护一个来自数组池的底层数组，并在释放时根据是否使用池进行适当的清理和归还。
        /// </summary>
        private sealed class ArrayPoolMemoryBlock : MemoryBlock<T>
        {
            public ObjectPool<ArrayPoolMemoryBlock> BlockPool;

            // 以下字段由提供者/池初始化或在释放时清理
            public ArrayPool<T> arrayPool;

            public T[] buffer;

            /// <summary>
            /// 返回当前内存块的底层可用内存（包含已提交与未提交部分）。
            /// </summary>
            protected override Memory<T> AvailableMemory => buffer;

            /// <summary>
            /// 构造函数：对象从池中获取后，字段由提供者初始化。
            /// </summary>
            public ArrayPoolMemoryBlock()
            {
                arrayPool = default!;
                buffer = default!;
                BlockPool = default!;
            }

            /// <summary>
            /// 释放内存块时的保护性实现：在基类清理已写入索引后，若底层数组来自池则将其归还并置空引用。
            /// </summary>
            protected override void ReleaseProtected()
            {
                arrayPool.Return(buffer);
                buffer = default!;
                arrayPool = default!;
                BlockPool.Release(this);
            }

            protected override void EnsureCapacityProtected(int sizeHint)
            {
                if (buffer == null)
                {
                    buffer = arrayPool.Rent(sizeHint);
                    return;
                }

                var newBuffer = arrayPool.Rent((int)(Committed + sizeHint));
                buffer.AsSpan(0, (int)Committed).CopyTo(newBuffer);
                arrayPool.Return(buffer);
                buffer = newBuffer;
            }
        }
    }
}