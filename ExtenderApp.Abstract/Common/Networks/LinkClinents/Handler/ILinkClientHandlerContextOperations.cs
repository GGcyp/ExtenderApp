using System.Net;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 连接客户端处理器上下文操作接口，定义了处理器之间事件传递的核心方法。
    /// </summary>
    public interface ILinkClientHandlerContextOperations
    {
        /// <summary>
        /// 发起连接操作并向下一个处理器传递事件。
        /// </summary>
        /// <param name="remoteAddress">远程端点地址。</param>
        /// <param name="localAddress">本地端点地址。</param>
        /// <param name="token">取消令牌。</param>
        ValueTask<Result> ConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default);

        /// <summary>
        /// 发起断开连接并向下一个处理器传递事件。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        ValueTask<Result> DisconnectAsync(CancellationToken token = default);

        /// <summary>
        /// 发起绑定操作并向下一个处理器传递事件。
        /// </summary>
        /// <param name="localAddress">本地端点地址。</param>
        /// <param name="token">取消令牌。</param>
        ValueTask<Result> BindAsync(EndPoint localAddress, CancellationToken token = default);

        /// <summary>
        /// 发起关闭并向下一个处理器传递事件。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        ValueTask<Result> CloseAsync(CancellationToken token = default);

        /// <summary>
        /// 当处理器捕获异常时调用。
        /// </summary>
        /// <param name="exception">捕获到的异常。</param>
        /// <param name="token">取消令牌。</param>
        ValueTask ExceptionCaught(Exception exception);

        /// <summary>
        /// 执行入站处理并向下一个处理器传递事件。
        /// </summary>
        /// <param name="cache">管线数据缓存。</param>
        /// <param name="token">取消令牌。</param>
        ValueTask<Result<int>> InboundHandleAsync(ValueCache cache, CancellationToken token = default);

        /// <summary>
        /// 执行出站处理并向下一个处理器传递事件。
        /// </summary>
        /// <param name="cache">管线数据缓存。</param>
        /// <param name="token">取消令牌。</param>
        ValueTask<Result> OutboundHandleAsync(ValueCache cache, CancellationToken token = default);
    }
}