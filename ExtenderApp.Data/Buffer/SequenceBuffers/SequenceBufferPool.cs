using ExtenderApp.Data.Buffer;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 可重用的序列缓冲区池，用于管理 <see cref="SequenceBuffer{T}"/> 实例的创建与回收。 使用内部的对象池（ <see cref="ObjectPool{T}"/>）避免频繁分配/释放，适合短生命周期或高并发重用场景。
    /// </summary>
    /// <typeparam name="T">序列中元素的类型。</typeparam>
    public class SequenceBufferPool<T>
    {
        private static readonly Lazy<SequenceBufferPool<T>> _shared = new(() => new());

        /// <summary>
        /// 全局共享的序列池单例，便于在应用中复用同一池实例。
        /// </summary>
        public static readonly SequenceBufferPool<T> Shared = _shared.Value;

        private readonly ObjectPool<SequenceBuffer<T>> _objectPool =
            ObjectPool.Create<SequenceBuffer<T>>();

        /// <summary>
        /// 从池中租用一个序列缓冲区的租约。调用方应通过返回的 <see cref="SequenceRental.Dispose"/> 来归还缓冲区。
        /// </summary>
        /// <param name="minimumLength">建议的最小容量（保留接口兼容，当前实现会从池中获取默认实例）。</param>
        /// <returns>表示租用关系的 <see cref="SequenceRental"/> 结构体，使用完毕后应 <c>Dispose()</c> 以归还实例。</returns>
        public SequenceBuffer<T> Rent()
        {
            var buffer = _objectPool.Get();
            buffer.OwnerPool = this; // 关联租用的缓冲区与当前池，方便归还时识别来源
            return buffer;
        }

        /// <summary>
        /// 将序列缓冲区归还到对象池。此方法由 <see cref="SequenceRental.Dispose"/> 调用。
        /// </summary>
        /// <param name="buffer">要归还的 <see cref="SequenceBuffer{T}"/> 实例（不能为 <c>null</c>）。</param>
        public void Release(SequenceBuffer<T> buffer)
        {
            buffer.OwnerPool = default!;
            _objectPool.Release(buffer);
        }
    }
}