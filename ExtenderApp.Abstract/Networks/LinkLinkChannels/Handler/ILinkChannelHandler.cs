using System.Net;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示链路通道（LinkChannel）管道中的处理器契约。
    /// 实现者负责处理生命周期事件（添加/移除/激活/停用）、绑定/连接/断开/关闭、以及入/出站数据的处理。
    /// </summary>
    public interface ILinkChannelHandler : IDisposable
    {
        /// <summary>
        /// 当处理器被添加到管道中时调用。实现者可在此处进行初始化或缓存上下文引用。
        /// </summary>
        /// <param name="context">当前处理器的上下文，包含对通道与相邻处理器的访问接口。</param>
        void Added(ILinkChannelHandlerContext context);

        /// <summary>
        /// 当处理器从管道中移除时调用。实现者应在此处释放与管道相关的资源或取消先前注册的回调。
        /// </summary>
        /// <param name="context">当前处理器的上下文。</param>
        void Removed(ILinkChannelHandlerContext context);

        /// <summary>
        /// 当通道变为活跃（例如连接已建立或本地已就绪）时调用。
        /// 实现者可以在此执行初始化异步工作（例如握手、订阅、准备缓冲等）。
        /// </summary>
        /// <param name="context">处理器上下文，用于在管道内转发后续事件或操作。</param>
        /// <param name="token">用于取消激活相关异步工作的取消令牌。</param>
        /// <returns>返回一个表示激活结果的异步 <see cref="ValueTask{Result}"/>，调用方应检查结果状态。</returns>
        ValueTask<Result> ActiveAsync(ILinkChannelHandlerContext context, CancellationToken token = default);

        /// <summary>
        /// 当通道变为非活跃（例如连接断开或通道关闭）时调用。
        /// 实现者应在此处执行清理工作并确保异步任务正确结束。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="token">用于取消停用相关异步工作的取消令牌。</param>
        /// <returns>返回一个表示停用结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> InactiveAsync(ILinkChannelHandlerContext context, CancellationToken token = default);

        /// <summary>
        /// 发起对远端的连接请求（异步）。实现者应尝试建立到 <paramref name="remoteAddress"/> 的连接，
        /// 并将可能的状态或错误通过返回结果或调用 <see cref="ILinkChannelHandlerContext.ExceptionCaught"/> 上报。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="remoteAddress">目标远端终结点（不能为空）。</param>
        /// <param name="localAddress">本地绑定地址（可为 null 表示不指定）。</param>
        /// <param name="token">用于取消连接过程的取消令牌。</param>
        /// <returns>表示连接结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> ConnectAsync(ILinkChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default);

        /// <summary>
        /// 发起断开连接操作（异步）。实现者应关闭底层会话并释放相关资源。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="token">用于取消断开操作的取消令牌。</param>
        /// <returns>表示断开结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> DisconnectAsync(ILinkChannelHandlerContext context, CancellationToken token = default);

        /// <summary>
        /// 发起通道关闭操作（异步）。关闭通常表示更彻底的资源释放与不可恢复的终止。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="token">用于取消关闭过程的取消令牌。</param>
        /// <returns>表示关闭结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> CloseAsync(ILinkChannelHandlerContext context, CancellationToken token = default);

        /// <summary>
        /// 发起本地绑定操作（异步），例如在 UDP 服务端或本地监听端口时调用。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="localAddress">要绑定的本地终结点（不能为空）。</param>
        /// <param name="token">用于取消绑定过程的取消令牌。</param>
        /// <returns>表示绑定结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> BindAsync(ILinkChannelHandlerContext context, EndPoint localAddress, CancellationToken token = default);

        /// <summary>
        /// 当处理器在执行过程中捕获到异常时调用。
        /// 实现者应根据需要记录异常或决定是否在管道中继续传播。返回的 <see cref="Result"/> 可用于指示异常是否被处理。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="exception">捕获到的异常实例。</param>
        /// <returns>返回一个 <see cref="Result"/> 表示异常处理结果（成功表示已处理，失败表示未处理）。</returns>
        Result ExceptionCaught(ILinkChannelHandlerContext context, Exception exception);

        /// <summary>
        /// 处理入站数据。实现者应从 <paramref name="cache"/> 中取出一个或多个缓冲区并对其进行解析/处理，
        /// 然后通过调用 <c>context.InboundHandleAsync</c> 将处理后的数据继续传递给下游处理器。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="cache">包含入站数据段的缓存容器（通常为一个或多个 <see cref="AbstractBuffer{byte}"/> 实例）。</param>
        /// <param name="token">用于取消入站处理的取消令牌。</param>
        /// <returns>表示处理结果的异步 <see cref="ValueTask{Result}"/>；方法应在处理完当前数据后返回。</returns>
        ValueTask<Result> InboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default);

        /// <summary>
        /// 处理出站数据。实现者应从 <paramref name="cache"/> 中读取要发送的数据，对其进行必要的转换/序列化，
        /// 并通过调用 <c>context.OutboundHandleAsync</c> 或底层发送接口将数据发送到下游（传输层）。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="cache">包含出站数据段的缓存容器。</param>
        /// <param name="token">用于取消出站处理的取消令牌。</param>
        /// <returns>表示处理结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> OutboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default);
    }
}