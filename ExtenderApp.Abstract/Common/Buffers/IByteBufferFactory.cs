using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// ByteBuffer 工厂接口，用于创建面向 byte 的顺序读写缓冲。
    /// </summary>
    /// <remarks>
    /// - 返回的 <see cref="ByteBuffer"/> 为 ref struct，实例本身非线程安全，请勿在多线程间并发读写同一实例。<br/>
    /// - 具体实现可选择从序列池租用可写序列，或返回只读视图；若为池化可写缓冲，使用完毕应调用 <c>Dispose()</c> 归还。
    /// </remarks>
    public interface IByteBufferFactory
    {
        /// <summary>
        /// 创建一个新的 <see cref="ByteBuffer"/> 实例。
        /// </summary>
        /// <returns>新创建的 <see cref="ByteBuffer"/>；其可写/只读能力由具体实现决定。</returns>
        /// <remarks>
        /// - 若返回的是池化的可写缓冲，请在使用结束后调用 <c>Dispose()</c> 以释放底层租约。<br/>
        /// - 生成的实例不可装箱、不可捕获到闭包、不可跨异步方法传递（因其为 ref struct）。
        /// </remarks>
        public ByteBuffer Create();
    }
}
