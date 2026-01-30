using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端格式化器管理器接口。
    /// 负责维护消息类型（MessageType）到对应 <see cref="ILinkClientFormatter"/> 实例的映射，
    /// 并在接收路径中将原始帧路由到合适的格式化器进行反序列化与分发。
    /// </summary>
    /// <remarks>
    /// 职责：
    /// - 维护“数据类型哈希（MessageType）→ <see cref="ILinkClientFormatter"/> 实例”的映射；
    /// - 支持以类型哈希或泛型类型两种方式获取已注册的格式化器；
    /// - 供网络管道在反序列化前进行快速路由与分发。
    /// 约定：
    /// - <c>MessageType</c> 应与 <see cref="ILinkClientFormatter{T}"/> 的内部约定保持一致（例如基于类型名的稳定哈希）；
    /// - 建议实现为线程安全（读多写少）以适配运行期收发并发。
    /// </remarks>
    /// <seealso cref="ILinkClientFormatter"/>
    /// <seealso cref="ILinkClientFormatter{T}"/>
    public interface ILinkClientFormatterManager : IDisposable
    {
        /// <summary>
        /// 注册一个客户端格式化器实例。
        /// </summary>
        /// <param name="formatter">要注册的格式化器实例；其 <see cref="ILinkClientFormatter.MessageType"/> 将作为映射键。</param>
        void AddFormatter(ILinkClientFormatter formatter);

        /// <summary>
        /// 确保并返回指定类型的格式化器实例。
        /// 如果已存在则返回已注册的实例；如果不存在则尝试创建并注册一个新实例（例如通过依赖注入）。
        /// </summary>
        /// <typeparam name="T">要获取或创建的格式化器类型，必须实现 <see cref="ILinkClientFormatter"/>。</typeparam>
        /// <returns>已注册或新创建的格式化器实例；如果无法创建则返回 <c>null</c>。</returns>
        T? AddFormatter<T>() where T : class, ILinkClientFormatter;

        /// <summary>
        /// 删除已注册的某一业务类型 <typeparamref name="T"/> 的格式化器。
        /// </summary>
        /// <typeparam name="T">要删除的格式化器类型。</typeparam>
        void RemoveFormatter<T>();

        /// <summary>
        /// 尝试获取指定消息类型的格式化器实例，当实例不存在时尝试通过依赖注入创建一个新实例。
        /// </summary>
        /// <typeparam name="T">要删除的消息/数据类型。</typeparam>
        void RemoveFormatter<T>();

        /// <summary>
        /// 将要发送的消息对象序列化为一个帧上下文，以便发送管道消费。
        /// </summary>
        /// <typeparam name="T">要序列化的消息/数据类型。</typeparam>
        /// <param name="value">要序列化的消息实例。</param>
        /// <returns>表示已准备好发送的帧的 <see cref="FrameContext"/> 实例。</returns>
        FrameContext ProcessSendVlaue<T>(T value);

        /// <summary>
        /// 在接收路径中处理/路由一个已解析出的帧上下文。
        /// 管理器应根据帧中的 MessageType（或其它协议约定）选择合适的 <see cref="ILinkClientFormatter"/>，
        /// 并调用其反序列化/分发逻辑（例如触发回调或返回对象）。
        /// </summary>
        /// <param name="operationValue">与本次套接字接收操作相关的元数据（例如接收字节数、远端地址或错误码）。</param>
        /// <param name="frameContext">
        /// 要处理的帧上下文（按引用传递以便实现可以在必要时替换或释放其内部缓冲）。
        /// 实现应在文档中明确 <see cref="FrameContext"/> 的所有权与释放约定（谁负责调用 <see cref="FrameContext.Dispose"/>）。
        /// </param>
        void ProcessReceivedFrame(SocketOperationValue operationValue, ref FrameContext frameContext);
    }
}