using System.Buffers;
using System.Diagnostics;
using ExtenderApp.Buffer.MemoryBlocks;
using ExtenderApp.Buffer.SequenceBuffers;

namespace ExtenderApp.Buffer
{
    /// <summary>
    /// 序列缓冲区提供者的抽象基类，定义了创建和回收 <see cref="SequenceBuffer{T}"/> 实例的基本接口和流程。 具体的缓冲区池实现（如 <see cref="DefaultSequenceBufferProvider{T}"/>）应继承此类并实现相关方法以管理缓冲区的生命周期。 该设计允许灵活替换不同的缓冲区池实现，以适应不同的性能需求和使用场景。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SequenceBufferProvider<T> : AbstractBufferProvider<T, SequenceBuffer<T>>
    {
        private static readonly Lazy<SequenceBufferProvider<T>> _shared = new(() => new());

        public static SequenceBufferProvider<T> Shared => _shared.Value;

        private readonly ObjectPool<SequenceBuffer<T>> _objectPool =
            ObjectPool.Create(static () => new SequenceBuffer<T>());

        /// <summary>
        /// 根据给定的 <see cref="ReadOnlySequence{T}"/> 构建并返回一个包含对应段的 <see cref="SequenceBuffer{T}"/>。
        /// </summary>
        /// <param name="sequence">要转换为序列缓冲区的只读序列，可能由多段组成。</param>
        /// <returns>已初始化并包含输入各段数据的 <see cref="SequenceBuffer{T}"/> 实例。 返回的缓冲区来自内部对象池；调用方在不再使用时应通过相应的释放流程将其归还（例如调用提供者的释放方法或使用 <see cref="AbstractBuffer{T}.PrepareForRelease"/>）。</returns>
        /// <remarks>
        /// 本方法会：
        /// - 从对象池获取一个 <see cref="SequenceBuffer{T}"/> 并初始化；
        /// - 遍历 <paramref name="sequence"/> 的每一段，将每段包装为固定内存块（使用 <see cref="FixedMemoryBlockProvider{T}"/>）， 再使用 <see
        ///   cref="MemoryBlockSegmentProvider{T}"/> 获取段对象并追加到缓冲区中。 注意：方法内部未对遍历过程中可能抛出的异常执行回收操作；若需要更强的异常安全性，可在调用方捕获异常后负责回收返回的缓冲区或修改本方法以在异常时释放已分配资源。
        /// </remarks>
        public SequenceBuffer<T> GetBuffer(ReadOnlySequence<T> sequence)
        {
            var buffer = _objectPool.Get();
            buffer.Initialize(this);

            var segmentProvider = SequenceBufferSegmentProvider<T>.Shared;
            var blockProvider = FixedMemoryBlockProvider<T>.Default;

            SequencePosition position = sequence.Start;
            while (sequence.TryGet(ref position, out var memory))
            {
                var block = blockProvider.GetBuffer(memory);
                var segment = segmentProvider.GetSegment(block);
                buffer.Append(segment);
            }
            return buffer;
        }

        protected override SequenceBuffer<T> CreateBufferProtected(int sizeHint)
        {
            var buffer = _objectPool.Get();
            buffer.Initialize(this);
            return buffer;
        }

        protected override void ReleaseProtected(SequenceBuffer<T> buffer)
        {
            _objectPool.Release(buffer);
        }
    }
}