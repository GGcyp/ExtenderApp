using System.Buffers;
using ExtenderApp.Buffer.MemoryBlocks;

namespace ExtenderApp.Buffer.Sequence
{
    /// <summary>
    /// 内存块序列段提供者，基于 <see cref="MemoryBlockProvider{T}"/> 获取底层缓冲区并将其封装为 <see cref="SequenceBufferSegment{T}"/> 实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class MemoryBlockSegmentProvider<T> : SequenceBufferSegmentProvider<T>
    {
        private static readonly Lazy<MemoryBlockSegmentProvider<T>> _default = new(static () => new());
        public static MemoryBlockSegmentProvider<T> Default = _default.Value;

        private readonly ObjectPool<MemoryBlockSequenceBufferSegment> _pool =
            ObjectPool.Create<MemoryBlockSequenceBufferSegment>();

        public MemoryBlockSegmentProvider() : this(MemoryBlockProvider<T>.Shared)
        {
        }

        public MemoryBlockSegmentProvider(MemoryBlockProvider<T> provider) : base(provider)
        {
        }

        protected override SequenceBufferSegment<T> GetSegmentProtected(MemoryBlock<T> buffer)
        {
            var segment = _pool.Get();
            buffer.Freeze();
            segment.Block = buffer;
            buffer.CommittedChanged += segment.UpdateRunningIndex;
            return segment;
        }

        protected override void ReleaseSegmentProtected(SequenceBufferSegment<T> segment)
        {
            if (segment is MemoryBlockSequenceBufferSegment blockSegment)
            {
                blockSegment.Block.CommittedChanged -= blockSegment.UpdateRunningIndex;
                blockSegment.Block.TryRelease();
                blockSegment.Block = default!;
                _pool.Release(blockSegment);
            }
        }

        /// <summary>
        /// 内存块序列段，封装了一个 <see cref="MemoryBlock{T}"/> 实例作为底层缓冲区，并通过事件监听机制自动更新段的已提交长度以维护正确的 RunningIndex。 该类通过对象池进行实例管理以优化性能和内存使用，适用于基于段的序列实现中需要动态管理内存块的场景。
        /// </summary>
        /// <typeparam name="T">序列段中元素的类型。</typeparam>
        private sealed class MemoryBlockSequenceBufferSegment : SequenceBufferSegment<T>
        {
            public MemoryBlock<T> Block;

            public MemoryBlockSequenceBufferSegment()
            {
                Block = default!;
            }

            protected internal override Memory<T> Memory => Block.Memory;

            protected internal override long Committed => Block.Committed;

            protected internal override int Available => Block.Available;

            protected internal override ReadOnlyMemory<T> CommittedMemory => Block.CommittedMemory;

            protected override Memory<T> GetMemotyProtected(int sizeHint = 0) => Block.GetMemory(sizeHint);

            protected override Span<T> GetSpanProtected(int sizeHint = 0) => Block.GetSpan(sizeHint);

            protected override void AdvanceProtected(int count) => Block.Advance(count);

            public override MemoryHandle Pin(int elementIndex) => Block.Pin(elementIndex);

            public override void Unpin() => Block.Unpin();

            public override SequenceBufferSegment<T> Slice(int start, int length) 
                => SegmentProvider?.GetSegment(Block.Slice(start, length)) ?? SequenceBufferSegment<T>.Empty;
        }
    }
}