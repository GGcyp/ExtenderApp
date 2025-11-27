using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义了发送端能力的抽象接口。
    /// </summary>
    /// <remarks>
    /// 实现应使用 <see cref="Result{T}"/> 包装 <see cref="SocketOperationValue"/>
    /// 来统一表达操作结果。对于可预期的网络错误，建议通过返回失败的 <see cref="Result"/> 表述；仅在参数非法或对象已释放等不可恢复的编程错误时抛出异常。
    /// </remarks>
    public interface ILinkSender
    {
        /// <summary>
        /// 同步发送指定缓冲区的数据。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。调用方负责提供合适的长度与内容。</param>
        /// <returns>
        /// 一个 <see cref="Result{T}"/> 实例，其中：
        /// - 失败时，<see cref="Result.Exception"/> 包含相应的异常信息。
        /// </returns>
        Result<SocketOperationValue> Send(Memory<byte> memory);

        /// <summary>
        /// 异步发送指定缓冲区的数据。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="token">用于取消异步操作的令牌。实现应尊重并响应该令牌。</param>
        /// <returns>
        /// 一个表示异步操作的 <see cref="ValueTask{TResult}"/>。其结果是一个 <see cref="Result{T}"/>，语义与 <see cref="Send(Memory{byte})"/> 相同。
        /// 当操作被取消时，应返回一个包含 <see cref="OperationCanceledException"/> 的失败 <see cref="Result"/>。
        /// </returns>
        ValueTask<Result<SocketOperationValue>> SendAsync(Memory<byte> memory, CancellationToken token = default);
    }
}