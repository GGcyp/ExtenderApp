using System.Buffers;
using ExtenderApp.Buffer.MemoryBlocks;
using ExtenderApp.Buffer.Sequence;

namespace ExtenderApp.Buffer
{
    /// <summary>
    /// 可重用的序列缓冲区池，用于管理 <see cref="SequenceBuffer{T}"/> 实例的创建与回收。 使用内部的对象池（ <see cref="ObjectPool{T}"/>）避免频繁分配/释放，适合短生命周期或高并发重用场景。
    /// </summary>
    /// <typeparam name="T">序列中元素的类型。</typeparam>
    public class SequenceBufferProvider<T> : AbstractBufferProvider<T, SequenceBuffer<T>>
    {
        private static readonly Lazy<SequenceBufferProvider<T>> _shared = new(() => new());

        /// <summary>
        /// 全局共享的序列池单例，便于在应用中复用同一池实例。
        /// </summary>
        public static readonly SequenceBufferProvider<T> Shared = _shared.Value;

        private readonly ObjectPool<SequenceBuffer<T>> _objectPool =
            ObjectPool.Create<SequenceBuffer<T>>();

        protected override SequenceBuffer<T> CreateBufferProtected(int sizeHint)
        {
            var buffer = _objectPool.Get();
            buffer.Initialize(this);
            return buffer;
        }

        protected override void ReleaseProtected(SequenceBuffer<T> buffer)
        {
            buffer.PrepareForRelease();
            _objectPool.Release(buffer);
        }

        /// <summary>
        /// 根据给定的 <see cref="ReadOnlySequence{T}"/> 构建并返回一个包含对应段的 <see cref="SequenceBuffer{T}"/>。
        /// </summary>
        /// <param name="sequence">要转换为序列缓冲区的只读序列，可能由多段组成。</param>
        /// <returns>
        /// 已初始化并包含输入各段数据的 <see cref="SequenceBuffer{T}"/> 实例。
        /// 返回的缓冲区来自内部对象池；调用方在不再使用时应通过相应的释放流程将其归还（例如调用提供者的释放方法或使用 <see cref="AbstractBuffer{T}.PrepareForRelease"/>）。
        /// </returns>
        /// <remarks>
        /// 本方法会：
        /// - 从对象池获取一个 <see cref="SequenceBuffer{T}"/> 并初始化；
        /// - 遍历 <paramref name="sequence"/> 的每一段，将每段包装为固定内存块（使用 <see cref="FixedMemoryBlockProvider{T}"/>），
        ///   再使用 <see cref="MemoryBlockSegmentProvider{T}"/> 获取段对象并追加到缓冲区中。
        /// 注意：方法内部未对遍历过程中可能抛出的异常执行回收操作；若需要更强的异常安全性，可在调用方捕获异常后负责回收返回的缓冲区或修改本方法以在异常时释放已分配资源。
        /// </remarks>
        public SequenceBuffer<T> GetBuffer(ReadOnlySequence<T> sequence)
        {
            var buffer = _objectPool.Get();
            buffer.Initialize(this);

            var segmentProvider = MemoryBlockSegmentProvider<T>.Shared;
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
    }
}