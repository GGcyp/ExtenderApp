using ExtenderApp.Abstract;
using ExtenderApp.Common.Hash;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 客户端通用格式化器基类。
    /// 负责把 <typeparamref name="T"/> 与传输字节之间进行序列化/反序列化，并在反序列化成功时触发事件分发。
    /// </summary>
    /// <typeparam name="T">要序列化/反序列化的强类型。</typeparam>
    /// <remarks>
    /// 约定与行为：
    /// - 缓冲来源：通过 <see cref="IByteBufferFactory"/> 创建 <see cref="ByteBuffer"/>；
    /// - 类型标识：<see cref="MessageType"/> 基于 <typeparamref name="T"/> 的类型名（<c>typeof(TLinkClient).Name</c>）使用 FNV-1a 计算，需与对端保持一致用于快速路由；
    /// - 分发机制：反序列化成功后触发 <see cref="Receive"/> 事件；
    /// - 线程安全：本类型本身无状态且通常可在多线程并发调用，但具体实现需保证对 <paramref name="buffer"/> 的读写只发生在调用栈内；
    /// - 资源管理：若工厂返回的是池化可写缓冲，调用方在使用完 <see cref="Serialize(T)"/> 的返回值后应调用 <see cref="ByteBuffer.Dispose"/> 归还资源。
    /// </remarks>
    public abstract class LinkClientFormatter<T> : IClientFormatter<T>
    {
        /// <summary>
        /// 与业务数据类型 <typeparamref name="T"/> 关联的稳定哈希。
        /// </summary>
        /// <remarks>
        /// - 计算规则：<c>FNV-1a(nameof(TLinkClient))</c>；
        /// - 注意：若存在跨命名空间同名类型，建议统一迁移到使用 FullName 的策略（需通讯两端同时调整）；
        /// - 用途：在消息头中用于快速定位对应的格式化器或处理管道。
        /// </remarks>
        public int MessageType { get; private set; }

        /// <summary>
        /// 当成功反序列化得到一个 <typeparamref name="T"/> 实例时触发。
        /// </summary>
        /// <remarks>
        /// - 事件可能在网络 IO 回调线程触发，订阅方需自行保证线程上下文与异常处理；
        /// - 订阅/反订阅请在对象生命周期内进行，避免潜在的内存泄漏。
        /// </remarks>
        public event Action<T>? Receive;

        public LinkClientFormatter()
        {
            MessageType = typeof(T).ComputeHash_FNV_1a();
        }

        /// <summary>
        /// 从输入缓冲中反序列化并分发（触发 <see cref="Receive"/>）。
        /// </summary>
        /// <param name="buffer">输入缓冲；实现应仅消费本次消息所需的字节，不得越界读取。</param>
        /// <remarks>
        /// - 若反序列化失败，应抛出明确的异常或由上层捕获处理；
        /// - 成功解析后会立即触发 <see cref="Receive"/> 事件。
        /// </remarks>
        public void DeserializeAndInvoke(ByteBuffer buffer)
        {
            T value = Deserialize(buffer);
            Receive?.Invoke(value);
        }

        /// <summary>
        /// 执行具体的反序列化实现。
        /// </summary>
        /// <param name="buffer">输入缓冲，读取位置应在实现内正确推进。</param>
        /// <returns>解析得到的 <typeparamref name="T"/> 实例。</returns>
        /// <exception cref="System.IO.EndOfStreamException">数据不足。</exception>
        /// <exception cref="FormatException">格式不符合预期。</exception>
        protected abstract T Deserialize(ByteBuffer buffer);

        /// <summary>
        /// 将值序列化为可发送的缓冲。
        /// </summary>
        /// <param name="value">要序列化的值。</param>
        /// <returns>承载序列化结果的 <see cref="ByteBuffer"/>。</returns>
        /// <remarks>
        /// - 缓冲由工厂创建并传入 <see cref="Serialize(T, ref ByteBuffer)"/> 填充；
        /// - 若返回的是池化可写缓冲，调用方在发送/使用完成后应调用 <see cref="ByteBuffer.Dispose"/> 归还资源。
        /// </remarks>
        public ByteBuffer Serialize(T value)
        {
            var buffer = ByteBuffer.CreateBuffer();
            Serialize(value, ref buffer);
            return buffer;
        }

        /// <summary>
        /// 执行具体的序列化实现，将 <paramref name="value"/> 写入到 <paramref name="buffer"/>。
        /// </summary>
        /// <param name="value">要序列化的值。</param>
        /// <param name="buffer">目标缓冲（可写），实现应正确申请写入空间并调用 <see cref="ByteBuffer.WriteAdvance(int)"/> 提交。</param>
        /// <remarks>
        /// - 实现不应在此处释放 <paramref name="buffer"/>；资源由调用方管理；
        /// - 请确保写入后的读取视图可被对端按协议正确解析。
        /// </remarks>
        protected abstract void Serialize(T value, ref ByteBuffer buffer);
    }
}