using ExtenderApp.Data;
using System.Threading;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 发送端能力的抽象接口。提供同步与异步两种发送方法，调用方将待发送的数据写入提供的
    /// <see cref="Memory{Byte}"/>。
    /// </summary>
    /// <remarks>
    /// 实现应使用 <see cref="SocketOperationResult"/>
    /// 统一表达操作结果（成功/失败/取消/超时等）。 对于可预期的网络错误建议通过返回值表述；仅在参数非法或对象已释放等不可恢复的编程错误时抛出异常。
    /// </remarks>
    public interface ILinkSend
    {
        /// <summary>
        /// 同步发送指定缓冲区的数据。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。调用方负责提供合适的长度与内容。</param>
        /// <returns>
        /// 返回 <see cref="SocketOperationResult"/>，包含：
        /// - <see
        ///   cref="SocketOperationResult.BytesTransferred"/>：本次实际发送的字节数。对于面向连接的协议（如 TCP）可能发生部分发送。
        /// - <see cref="SocketOperationResult.SocketError"/>：若底层套接字发生错误，则包含对应的异常信息。
        /// - <see
        ///   cref="SocketOperationResult.Code"/>
        ///   / <see cref="SocketOperationResult.IsSuccess"/>：统一的操作状态描述。
        /// </returns>
        /// <remarks>
        /// 建议约定：
        /// - 对于 TCP，如需确保全部字节发送完毕，调用方应在返回
        ///   BytesTransferred 小于输入长度时循环重试直至发送完或发生错误。
        /// - 对于无连接协议（如 UDP），实现可能将整个数据作为单个报文发送；若超出底层最大报文大小，可能触发错误或被截断（由实现决定并在结果中说明）。
        /// - 此方法为阻塞调用；在可能长时间阻塞的场景下应使用 <see
        ///   cref="SendAsync(Memory{byte}, CancellationToken)"/>。
        /// - 实现应尽量避免在常见网络错误下抛出异常，而是通过返回的 <see
        ///   cref="SocketOperationResult"/> 提供详细信息。
        /// </remarks>
        SocketOperationResult Send(Memory<byte> memory);

        /// <summary>
        /// 异步发送指定缓冲区的数据。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="token">取消令牌。实现应尊重并响应该令牌。</param>
        /// <returns>
        /// 异步返回 <see
        /// cref="SocketOperationResult"/>，语义与
        /// <see cref="Send(Memory{byte})"/> 相同。
        /// 在取消场景下，优先通过返回 Code 为 <see
        /// cref="ResultCode.Canceled"/>
        /// 的结果表达取消；实现也可以选择抛出 <see cref="OperationCanceledException"/>（应在文档中明确）。
        /// </returns>
        /// <remarks>
        /// - 异步实现应对高并发场景友好，避免引入不必要的线程池阻塞或资源争用。
        /// - 建议调用方通过缓冲复用（例如 MemoryPool）来减少内存分配开销。
        /// - 若使用底层异步 I/O（如 SocketAsyncEventArgs / IOCP），应在返回结果中尽可能填充诊断信息以便调用方决定是否重试或记录告警。
        /// </remarks>
        ValueTask<SocketOperationResult> SendAsync(Memory<byte> memory, CancellationToken token = default);
    }
}