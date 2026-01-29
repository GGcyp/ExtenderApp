using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 链路客户端消息格式化器的公共接口契约。
    /// 用于标识消息类型并为具体类型的序列化/反序列化提供统一约定。
    /// </summary>
    public interface ILinkClientFormatter
    {
        /// <summary>
        /// 消息类型标识符（用于在运行时将接收到的帧路由到对应的格式化器）。
        /// </summary>
        int MessageType { get; }

        /// <summary>
        /// 将接收到的帧上下文反序列化并触发或执行相应的处理逻辑。
        /// </summary>
        /// <param name="operationValue">
        /// 与本次接收操作相关的附加信息（例如接收字节数、远端地址或错误码等），
        /// 用于格式化器在反序列化或后续处理时参考上下文。
        /// </param>
        /// <param name="buffer">
        /// 要反序列化的帧上下文。
        /// </param>
        /// <remarks>
        /// - 实现应尽量避免抛出未捕获异常；必要时抛出明确的异常以便上层处理。
        /// - 若实现需要将缓冲内容跨线程或异步保存，应先复制到堆内存副本，避免持有对池化或栈上内存的引用。
        /// - 本方法通常会在接收路径的同步栈内调用，以便能安全处理 <see cref="FrameContext"/>（避免跨异步边界持有栈上/池化内存）。
        /// </remarks>
        void DeserializeAndInvoke(SocketOperationValue operationValue, ref FrameContext buffer);
    }

    /// <summary>
    /// 针对具体业务类型的序列化/反序列化格式化器契约。
    /// </summary>
    /// <typeparam name="T">要序列化/反序列化的业务消息类型。</typeparam>
    public interface ILinkClientFormatter<T> : ILinkClientFormatter
    {
        /// <summary>
        /// 当本格式化器成功反序列化出 <typeparamref name="T"/> 类型消息时触发的事件。
        /// 事件参数使用 <see cref="LinkClientReceivedValue{T}"/>，以便同时传递反序列化对象与接收上下文。
        /// </summary>
        /// <remarks>
        /// - 请在文档中说明事件触发的线程上下文（同步/异步）与异常传播策略；若在非调用线程触发，应说明并保证调用方能安全处理回调。
        /// - 订阅者不得在回调中长时间阻塞或在回调返回后继续持有对池化/栈上内存的引用。
        /// </remarks>
        event Action<LinkClientReceivedValue<T>>? Received;

        /// <summary>
        /// 将业务对象序列化为可发送的帧上下文。
        /// </summary>
        /// <param name="value">要序列化的业务对象。</param>
        /// <returns>示例数据序列化后的<see cref="ByteBuffer"/>缓冲区</returns>
        ByteBuffer Serialize(T value);
    }
}