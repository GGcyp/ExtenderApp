using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端侧的序列化/反序列化器最小契约。
    /// </summary>
    /// <remarks>
    /// 职责：
    /// - 将原始字节从 <see cref="ByteBuffer"/> 中反序列化为具体的数据类型；
    /// - 提供稳定的数据类型标识与格式化器类型标识，便于快速路由与协议协商；
    /// - 反序列化成功后由泛型接口 <see cref="IClientFormatter{T}"/> 触发类型安全的事件。
    /// </remarks>
    public interface ILinkClientFormatter
    {
        /// <summary>
        /// 与“业务数据类型”关联的稳定哈希。
        /// </summary>
        /// <remarks>
        /// - 应与序列化/反序列化的目标类型一一对应，跨进程/跨语言保持一致；<br/>
        /// - 常见做法是使用类型名或自定义标识计算哈希（例如 FNV-1a）；<br/>
        /// - 用于在反序列化前的快速分发与匹配。
        /// </remarks>
        int DataType { get; }

        /// <summary>
        /// 从输入缓冲中反序列化并进行分发（触发对应的类型事件）。
        /// </summary>
        /// <param name="buffer">输入缓冲；从其未读序列中解析当前消息。</param>
        /// <remarks>
        /// - 实现应仅消费当前消息所需的字节，超读会破坏后续解析；<br/>
        /// - 反序列化成功后应调用 <see cref="IClientFormatter{T}.Receive"/> 事件通知订阅者；<br/>
        /// - 建议对异常（长度不足/格式不符等）进行显式处理（抛出或上报）。
        /// </remarks>
        void DeserializeAndInvoke(ByteBuffer buffer);
    }

    /// <summary>
    /// 面向具体类型 <typeparamref name="T"/> 的客户端格式化器契约。
    /// </summary>
    /// <typeparam name="T">消息/数据的强类型。</typeparam>
    public interface IClientFormatter<T> : ILinkClientFormatter
    {
        /// <summary>
        /// 当成功反序列化得到一个 <typeparamref name="T"/> 实例时触发。
        /// </summary>
        /// <remarks>
        /// - 可能在 I/O 回调线程触发，订阅方应避免长时间阻塞；<br/>
        /// - 如需耗时处理，建议将数据投递到队列/线程池再处理。
        /// </remarks>
        event Action<T>? Receive;

        /// <summary>
        /// 将 <paramref name="value"/> 序列化为可发送的缓冲。
        /// </summary>
        /// <param name="value">要序列化的值。</param>
        /// <returns>
        /// 承载序列化结果的 <see cref="ByteBuffer"/>。
        /// 调用方在使用完成后应调用 <see cref="ByteBuffer.Dispose"/> 归还底层资源（若持有租约）。
        /// </returns>
        ByteBuffer Serialize(T value);
    }
}
