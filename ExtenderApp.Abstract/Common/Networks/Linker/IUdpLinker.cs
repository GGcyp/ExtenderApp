using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义了 UDP 链接器的接口，在 <see cref="ILinker"/> 的基础上增加了发送到指定终结点的方法。
    /// </summary>
    public interface IUdpLinker : ILinker, ILinkBind
    {
        /// <summary>
        /// 向指定远端发送一个 UDP 数据报。此方法适用于未连接的 UDP 套接字，或需要临时覆盖默认远端的场景。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="endPoint">目标远端地址与端口。其地址族需与底层套接字一致。</param>
        /// <returns>
        /// 一个 <see cref="Result{T}"/> 实例，其中：
        /// - 成功时， <see cref="Result{T}.Data"/> 包含一个 <see cref="SocketOperationValue"/>，其 <see cref="SocketOperationValue.BytesTransferred"/> 表示实际发送的字节数。
        /// - 失败时， <see cref="Result.Exception"/> 包含相应的异常信息。
        /// </returns>
        Result<SocketOperationValue> SendTo(Memory<byte> memory, EndPoint endPoint);

        /// <summary>
        /// 以异步方式向指定远端发送一个 UDP 数据报。此方法适用于未连接的 UDP 套接字，或需要临时覆盖默认远端的场景。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="endPoint">目标远端地址与端口。其地址族需与底层套接字一致。</param>
        /// <param name="token">用于取消异步操作的令牌。</param>
        /// <returns>
        /// 一个表示异步操作的 <see cref="ValueTask{TResult}"/>。其结果是一个 <see cref="Result{T}"/>，语义与 <see cref="SendTo(Memory{byte}, EndPoint)"/> 相同。
        /// 当操作被取消时，应返回一个包含 <see cref="OperationCanceledException"/> 的失败 <see cref="Result"/>。
        /// </returns>
        ValueTask<Result<SocketOperationValue>> SendToAsync(Memory<byte> memory, EndPoint endPoint, CancellationToken token = default);
    }
}