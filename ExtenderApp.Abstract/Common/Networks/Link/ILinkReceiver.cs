using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义了接收端能力的抽象接口，负责将收到的数据写入调用方提供的缓冲区。
    /// </summary>
    /// <remarks>实现应使用 <see cref="Result{T}"/> 包装 <see cref="SocketOperationValue"/> 来统一表达操作结果。 对于可预期的网络错误，建议通过返回失败的 <see cref="Result"/> 表述；仅在参数非法或对象已释放等不可恢复的编程错误时抛出异常。</remarks>
    public interface ILinkReceiver
    {
        #region Receive

        /// <summary>
        /// 同步接收数据到指定跨度。
        /// </summary>
        /// <param name="span">用于接收数据的跨度。</param>
        /// <returns>一个包含 <see cref="SocketOperationValue"/> 的 <see cref="Result{T}"/> 实例。</returns>
        Result<SocketOperationValue> Receive(Span<byte> span);

        /// <summary>
        /// 同步接收数据到指定跨度，并指定接收标志。
        /// </summary>
        /// <param name="span">用于接收数据的跨度。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>一个包含 <see cref="SocketOperationValue"/> 的 <see cref="Result{T}"/> 实例。</returns>
        Result<SocketOperationValue> Receive(Span<byte> span, LinkFlags flags);

        /// <summary>
        /// 同步接收数据到指定缓冲区。
        /// </summary>
        /// <param name="memory">可写缓冲区，用于承载本次接收的数据。调用方负责提供合适大小的缓冲区。</param>
        /// <returns>
        /// 一个包含 <see cref="SocketOperationValue"/> 的 <see cref="Result{T}"/> 实例，其中： <br/>- 成功时，可通过 <see
        /// cref="SocketOperationValue.BytesTransferred"/> 获取实际收到的字节数。 <br/>- 失败时， <see cref="Result.Exception"/> 包含相应的异常信息。
        /// </returns>
        Result<SocketOperationValue> Receive(Memory<byte> memory);

        /// <summary>
        /// 同步接收数据到指定缓冲区，并指定接收标志。
        /// </summary>
        /// <param name="memory">可写缓冲区，用于承载本次接收的数据。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>一个包含 <see cref="SocketOperationValue"/> 的 <see cref="Result{T}"/> 实例。</returns>
        Result<SocketOperationValue> Receive(Memory<byte> memory, LinkFlags flags);

        /// <summary>
        /// 同步接收数据到非连续存储的内存块（Gather 接收）中。
        /// </summary>
        /// <param name="buffer">用于存放接收数据的 <see cref="ArraySegment{T}"/> 列表。系统将按顺序填充这些分片。</param>
        /// <returns>一个包含 <see cref="SocketOperationValue"/> 的 <see cref="Result{T}"/> 实例。</returns>
        Result<SocketOperationValue> Receive(IList<ArraySegment<byte>> buffer);

        /// <summary>
        /// 同步接收数据到非连续存储的内存块（Gather 接收）中，并指定接收标志。
        /// </summary>
        /// <param name="buffer">用于存放接收数据的 <see cref="ArraySegment{T}"/> 列表。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>一个包含 <see cref="SocketOperationValue"/> 的 <see cref="Result{T}"/> 实例。</returns>
        Result<SocketOperationValue> Receive(IList<ArraySegment<byte>> buffer, LinkFlags flags);

        #endregion Receive

        #region ReceiveAsync

        /// <summary>
        /// 异步接收数据到指定缓冲区。
        /// </summary>
        /// <param name="memory">可写缓冲区，用于承载本次接收的数据。</param>
        /// <param name="token">用于取消异步接收操作的 <see cref="CancellationToken"/>。实现应尊重该令牌。</param>
        /// <returns>
        /// 一个表示异步操作的 <see cref="ValueTask{TResult}"/>。其结果是一个 <see cref="Result{T}"/>，语义与 <see cref="Receive(Memory{byte})"/> 相同。 当操作被取消时，应返回一个包含 <see
        /// cref="OperationCanceledException"/> 的失败 <see cref="Result"/>。
        /// </returns>
        ValueTask<Result<SocketOperationValue>> ReceiveAsync(Memory<byte> memory, CancellationToken token = default);

        /// <summary>
        /// 异步接收数据到指定缓冲区，并指定接收标志。
        /// </summary>
        /// <param name="memory">可写缓冲区，用于承载本次接收的数据。</param>
        /// <param name="flags">接收标志。</param>
        /// <param name="token">用于取消异步接收操作的 <see cref="CancellationToken"/>。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask{TResult}"/>。</returns>
        ValueTask<Result<SocketOperationValue>> ReceiveAsync(Memory<byte> memory, LinkFlags flags, CancellationToken token = default);

        /// <summary>
        /// 异步接收数据到非连续存储的内存块（Gather 接收）中。
        /// </summary>
        /// <param name="buffer">用于存放接收数据的非连续内存分片列表。</param>
        /// <param name="token">用于取消异步操作的令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask{TResult}"/>。其结果是一个 <see cref="Result{T}"/>，语义与同步 Gather 接收方法一致。</returns>
        ValueTask<Result<SocketOperationValue>> ReceiveAsync(IList<ArraySegment<byte>> buffer, CancellationToken token = default);

        /// <summary>
        /// 异步接收数据到非连续存储的内存块（Gather 接收）中，并指定接收标志。
        /// </summary>
        /// <param name="buffer">用于存放接收数据的非连续内存分片列表。</param>
        /// <param name="flags">接收标志。</param>
        /// <param name="token">用于取消异步操作的令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask{TResult}"/>。</returns>
        ValueTask<Result<SocketOperationValue>> ReceiveAsync(IList<ArraySegment<byte>> buffer, LinkFlags flags, CancellationToken token = default);

        #endregion ReceiveAsync
    }
}