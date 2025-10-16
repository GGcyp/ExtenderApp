using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Buffers
{
    /// <summary>
    /// <see cref="IByteBufferFactory"/> 的默认实现。
    /// 通过 <see cref="SequencePool{T}"/>（T=byte）为 <see cref="ByteBuffer"/> 提供可写序列的租约。
    /// </summary>
    /// <remarks>
    /// - 工厂本身线程安全，可在多线程环境下复用。<br/>
    /// - 由本工厂创建的 <see cref="ByteBuffer"/> 为 ref struct，实例非线程安全；使用完毕请调用 <c>Dispose()</c> 归还底层租约。
    /// </remarks>
    internal class ByteBufferFactory : IByteBufferFactory
    {
        /// <summary>
        /// 序列池，用于分配/回收底层可写序列。
        /// </summary>
        private readonly SequencePool<byte> _pool;

        /// <summary>
        /// 使用给定的序列池初始化工厂。
        /// </summary>
        /// <param name="pool">用于分配与回收序列的 <see cref="SequencePool{T}"/>。</param>
        public ByteBufferFactory(SequencePool<byte> pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// 创建一个新的 <see cref="ByteBuffer"/>，并从池中获取可写序列的租约。
        /// </summary>
        /// <returns>新建的 <see cref="ByteBuffer"/> 实例；使用完毕需调用 <c>Dispose()</c> 归还租约。</returns>
        public ByteBuffer Create()
        {
            return new ByteBuffer(_pool);
        }
    }
}
