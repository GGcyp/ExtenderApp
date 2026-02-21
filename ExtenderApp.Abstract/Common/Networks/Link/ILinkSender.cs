using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义了发送端能力的抽象接口。
    /// </summary>
    /// <remarks>实现应使用 <see cref="Result{T}"/> 包装 <see cref="LinkOperationValue"/> 来统一表达操作结果。对于可预期的网络错误，建议通过返回失败的 <see cref="Result"/> 表述；仅在参数非法或对象已释放等不可恢复的编程错误时抛出异常。</remarks>
    public interface ILinkSender
    {
        #region Send

        /// <summary>
        /// 同步发送指定的只读字节跨度数据。
        /// </summary>
        /// <remarks>该重载通常用于小块数据或位于栈上的 <see cref="Span{T}"/>，以避免不必要的内存装箱或分配。</remarks>
        /// <param name="span">要发送的数据跨度。</param>
        /// <returns>一个包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/> 实例，语义与 <see cref="Send(Memory{byte})"/> 一致。</returns>
        Result<LinkOperationValue> Send(ReadOnlySpan<byte> span);

        /// <summary>
        /// 同步发送指定的只读字节跨度数据，并指定发送标志。
        /// </summary>
        /// <param name="span">要发送的数据跨度。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>一个包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/> 实例，语义与 <see cref="Send(Memory{byte})"/> 一致。</returns>
        Result<LinkOperationValue> Send(ReadOnlySpan<byte> span, LinkFlags flags);

        /// <summary>
        /// 同步发送指定缓冲区的数据。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。调用方负责提供合适的长度与内容。</param>
        /// <returns>
        /// 一个包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/> 实例，其中： <br/>- 成功时，可通过 <see
        /// cref="LinkOperationValue.BytesTransferred"/> 获取实际发出的字节数。 <br/>- 失败时， <see cref="Result.Exception"/> 包含相应的网络或逻辑异常信息。
        /// </returns>
        Result<LinkOperationValue> Send(ReadOnlyMemory<byte> memory);

        /// <summary>
        /// 同步发送指定缓冲区的数据，并指定发送标志。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。调用方负责提供合适的长度与内容。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>
        /// 一个包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/> 实例，其中： <br/>- 成功时，可通过 <see
        /// cref="LinkOperationValue.BytesTransferred"/> 获取实际发出的字节数。 <br/>- 失败时， <see cref="Result.Exception"/> 包含相应的网络或逻辑异常信息。
        /// </returns>
        Result<LinkOperationValue> Send(ReadOnlyMemory<byte> memory, LinkFlags flags);

        /// <summary>
        /// 同步发送非连续存储的内存块（Scatter 发送）数据。
        /// </summary>
        /// <param name="buffer">包含多个内存分片的列表。实现应按顺序发送列表中的所有分片。</param>
        /// <returns>一个包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/> 实例。</returns>
        Result<LinkOperationValue> Send(IList<ArraySegment<byte>> buffer);

        /// <summary>
        /// 同步发送非连续存储的内存块（Scatter 发送）数据，并指定发送标志。
        /// </summary>
        /// <param name="buffer">包含多个内存分片的列表。实现应按顺序发送列表中的所有分片。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>一个包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/> 实例。</returns>
        Result<LinkOperationValue> Send(IList<ArraySegment<byte>> buffer, LinkFlags flags);

        #endregion Send

        #region SendAsync

        /// <summary>
        /// 异步发送指定缓冲区的数据。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="token">用于取消异步操作的令牌。实现应尊重并响应该令牌。</param>
        /// <returns>
        /// 一个表示异步操作的 <see cref="ValueTask{TResult}"/>。其结果是一个 <see cref="Result{T}"/>，语义与 <see cref="Send(Memory{byte})"/> 一致。 当操作被取消时，应返回一个包含 <see
        /// cref="OperationCanceledException"/> 的失败 <see cref="Result"/>。
        /// </returns>
        ValueTask<Result<LinkOperationValue>> SendAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default);

        /// <summary>
        /// 异步发送指定缓冲区的数据，并指定发送标志。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="flags">发送标志。</param>
        /// <param name="token">用于取消异步操作的令牌。实现应尊重并响应该令牌。</param>
        /// <returns>
        /// 一个表示异步操作的 <see cref="ValueTask{TResult}"/>。其结果是一个 <see cref="Result{T}"/>，语义与 <see cref="Send(Memory{byte})"/> 一致。 当操作被取消时，应返回一个包含 <see
        /// cref="OperationCanceledException"/> 的失败 <see cref="Result"/>。
        /// </returns>
        ValueTask<Result<LinkOperationValue>> SendAsync(ReadOnlyMemory<byte> memory, LinkFlags flags, CancellationToken token = default);

        /// <summary>
        /// 异步发送非连续存储的内存块（Scatter 发送）数据。
        /// </summary>
        /// <param name="buffer">要发送的非连续内存分片列表。</param>
        /// <param name="token">用于取消异步操作的令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask{TResult}"/>。其结果是一个 <see cref="Result{T}"/>，语义与同步 Scatter 发送方法一致。</returns>
        ValueTask<Result<LinkOperationValue>> SendAsync(IList<ArraySegment<byte>> buffer, CancellationToken token = default);

        /// <summary>
        /// 异步发送非连续存储的内存块（Scatter 发送）数据，并指定发送标志。
        /// </summary>
        /// <param name="buffer">要发送的非连续内存分片列表。</param>
        /// <param name="flags">发送标志。</param>
        /// <param name="token">用于取消异步操作的令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask{TResult}"/>。其结果是一个 <see cref="Result{T}"/>，语义与同步 Scatter 发送方法一致。</returns>
        ValueTask<Result<LinkOperationValue>> SendAsync(IList<ArraySegment<byte>> buffer, LinkFlags flags, CancellationToken token = default);

        #endregion SendAsync
    }
}