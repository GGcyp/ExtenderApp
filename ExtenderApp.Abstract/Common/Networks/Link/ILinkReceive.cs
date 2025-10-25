using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 接收端能力的抽象接口。提供同步与异步两种接收方法，将收到的原始字节写入调用方提供的缓冲区并返回一次接收操作的结果描述。
    /// </summary>
    /// <remarks>
    /// 实现者应使用 <see cref="SocketOperationResult"/> 来统一表达本次接收的状态（成功/失败/取消/超时等），
    /// 尽量将可预期的网络错误封装到返回值中，而将真正的编程错误（例如参数为 null 或对象已释放）通过异常抛出。
    /// </remarks>
    public interface ILinkReceive
    {
        /// <summary>
        /// 同步接收数据到指定缓冲区。
        /// </summary>
        /// <param name="memory">可写缓冲区，用于承载本次接收的数据。调用方负责提供合适大小的缓冲区。</param>
        /// <returns>
        /// 返回 <see cref="SocketOperationResult"/>，包含：
        /// - <see cref="SocketOperationResult.BytesTransferred"/>：本次实际接收的字节数。TCP 中返回 0 常表示对端已优雅关闭连接；UDP 中若缓冲不足可能导致截断（由实现通过额外字段或标志标识）。  
        /// - <see cref="SocketOperationResult.RemoteEndPoint"/>：若适用（例如 UDP/ReceiveFrom/ReceiveMessageFrom），包含发送方地址，否则为 null。  
        /// - <see cref="SocketOperationResult.SocketError"/>：底层套接字异常信息（失败时非 null）。  
        /// - <see cref="SocketOperationResult.Code"/> / <see cref="SocketOperationResult.IsSuccess"/>：统一的操作结果状态。
        /// </returns>
        /// <remarks>
        /// 行为约定（建议实现）：
        /// - 实现应尽量避免在可预期的网络错误下抛出异常，而是通过返回值反映错误状态；仅在参数非法或对象已释放等不可恢复的编程错误时抛出异常（例如 <see cref="ObjectDisposedException"/>）。  
        /// - 对于面向连接的协议（例如 TCP），调用方应准备处理 0 字节的情况（表示对端关闭）。  
        /// - 对于无连接协议（例如 UDP），如果缓冲区小于单个报文长度，数据可能被截断，必要时通过实现层的扩展信息告知截断发生。
        /// - 此方法为阻塞调用；在可能长时间阻塞的情形下建议使用 <see cref="ReceiveAsync(Memory{byte}, CancellationToken)"/>。
        /// </remarks>
        SocketOperationResult Receive(Memory<byte> memory);

        /// <summary>
        /// 异步接收数据到指定缓冲区。
        /// </summary>
        /// <param name="memory">可写缓冲区，用于承载本次接收的数据。</param>
        /// <param name="token">用于取消异步接收操作的 <see cref="CancellationToken"/>。实现应尊重该令牌。</param>
        /// <returns>
        /// 异步返回 <see cref="SocketOperationResult"/>，语义与 <see cref="Receive(Memory{byte})"/> 相同。
        /// 在取消情形下，优选返回 Code 为 <see cref="ResultCode.Canceled"/> 的结果；实现也可以选择在被取消时抛出 <see cref="OperationCanceledException"/>（应在文档中明确）。
        /// </returns>
        /// <remarks>
        /// - 此方法应对高并发、重复调用保持良好表现，建议避免在实现中造成线程池饥饿或资源竞争。  
        /// - 对于要求低延迟的场景，调用方可以通过合适的缓冲策略（预分配缓冲区、循环使用 MemoryPool）来减少分配开销。  
        /// - 若实现使用 I/O 完成端口 / SocketAsyncEventArgs，应在返回的 <see cref="SocketOperationResult"/> 中填充尽可能多的诊断信息以便调用方处理。
        /// </remarks>
        ValueTask<SocketOperationResult> ReceiveAsync(Memory<byte> memory, CancellationToken token = default);
    }
}