using System.Net;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示链路管道中的处理器基类，提供生命周期事件。
    /// </summary>
    public interface ILinkClientHandler : IDisposable
    {
        /// <summary>
        /// 当处理器被添加到管道时调用。
        /// </summary>
        void Added(ILinkClientHandlerContext context);

        /// <summary>
        /// 当处理器从管道移除时调用。
        /// </summary>
        void Removed(ILinkClientHandlerContext context);

        /// <summary>
        /// 当通道变为活跃状态时调用（例如连接建立）。
        /// </summary>
        void Active(ILinkClientHandlerContext context);

        /// <summary>
        /// 当通道变为非活跃状态时调用（例如连接断开）。
        /// </summary>
        void Inactive(ILinkClientHandlerContext context);

        /// <summary>
        /// 发起连接操作。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="remoteAddress">远程端点地址。</param>
        /// <param name="localAddress">本地端点地址。</param>
        ValueTask<Result> ConnectAsync(ILinkClientHandlerContext context, EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default);

        /// <summary>
        /// 发起断开连接操作。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        ValueTask<Result> DisconnectAsync(ILinkClientHandlerContext context, CancellationToken token = default);

        /// <summary>
        /// 发起关闭操作。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        ValueTask<Result> CloseAsync(ILinkClientHandlerContext context, CancellationToken token = default);

        /// <summary>
        /// 发起绑定操作。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="localAddress">本地端点地址。</param>
        ValueTask<Result> BindAsync(ILinkClientHandlerContext context, EndPoint localAddress, CancellationToken token = default);

        /// <summary>
        /// 当处理器捕获异常时调用。
        /// </summary>
        /// <param name="exception">捕获到的异常。</param>
        ValueTask ExceptionCaught(ILinkClientHandlerContext context, Exception exception);

        /// <summary>
        /// 处理入站数据。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="cache">管线数据缓存。</param>
        /// <param name="token">取消令牌。</param>
        ValueTask<Result<int>> InboundHandleAsync(ILinkClientHandlerContext context, ValueCache cache, CancellationToken token = default);

        /// <summary>
        /// 处理出站数据。
        /// </summary>
        /// <param name="context">处理器上下文。</param>
        /// <param name="cache">管线数据缓存。</param>
        /// <param name="token">取消令牌。</param>
        ValueTask<Result> OutboundHandleAsync(ILinkClientHandlerContext context, ValueCache cache, CancellationToken token = default);
    }
}