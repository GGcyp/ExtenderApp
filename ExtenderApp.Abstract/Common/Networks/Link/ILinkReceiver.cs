using ExtenderApp.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义了接收端能力的抽象接口，负责将收到的数据写入调用方提供的缓冲区。
    /// </summary>
    /// <remarks>
    /// 实现应使用 <see cref="Result{T}"/> 包装 <see cref="SocketOperationValue"/>
    /// 来统一表达操作结果。对于可预期的网络错误，建议通过返回失败的 <see cref="Result"/> 表述；仅在参数非法或对象已释放等不可恢复的编程错误时抛出异常。
    /// </remarks>
    public interface ILinkReceiver
    {
        /// <summary>
        /// 同步接收数据到指定缓冲区。
        /// </summary>
        /// <param name="memory">可写缓冲区，用于承载本次接收的数据。调用方负责提供合适大小的缓冲区。</param>
        /// <returns>
        /// 一个 <see cref="Result{T}"/> 实例，其中：
        /// - 成功时，<see cref="Result{T}.Data"/> 包含一个 <see cref="SocketOperationValue"/>，其 <see cref="SocketOperationValue.BytesTransferred"/> 表示实际接收的字节数。
        /// - 失败时，<see cref="Result.Exception"/> 包含相应的异常信息。
        /// </returns>
        Result<SocketOperationValue> Receive(Memory<byte> memory);

        /// <summary>
        /// 异步接收数据到指定缓冲区。
        /// </summary>
        /// <param name="memory">可写缓冲区，用于承载本次接收的数据。</param>
        /// <param name="token">用于取消异步接收操作的 <see cref="CancellationToken"/>。实现应尊重该令牌。</param>
        /// <returns>
        /// 一个表示异步操作的 <see cref="ValueTask{TResult}"/>。其结果是一个 <see cref="Result{T}"/>，语义与 <see cref="Receive(Memory{byte})"/> 相同。
        /// 当操作被取消时，应返回一个包含 <see cref="OperationCanceledException"/> 的失败 <see cref="Result"/>。
        /// </returns>
        ValueTask<Result<SocketOperationValue>> ReceiveAsync(Memory<byte> memory, CancellationToken token = default);
    }
}