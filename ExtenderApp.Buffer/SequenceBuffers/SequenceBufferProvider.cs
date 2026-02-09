using ExtenderApp.Buffer;

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
            buffer.OwnerProvider = this; // 关联租用的缓冲区与当前池，方便归还时识别来源
            return buffer;
        }

        protected override void ReleaseProtected(SequenceBuffer<T> buffer)
        {
            buffer.OwnerProvider = default!;
            _objectPool.Release(buffer);
        }
    }
}