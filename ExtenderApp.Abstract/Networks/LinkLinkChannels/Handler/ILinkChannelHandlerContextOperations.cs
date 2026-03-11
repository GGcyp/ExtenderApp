using System.Net;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义处理器上下文的操作契约：为管道内处理器之间的事件传递提供核心方法（激活/停用、连接/断开、收发等）。
    /// 实现应负责将调用转发到下一个合适的处理器，并处理取消与异常传播语义。
    /// </summary>
    public interface ILinkChannelHandlerContextOperations
    {
        /// <summary>
        /// 将激活事件沿入站方向传递到下一个处理器（连接已建立或通道就绪时调用）。
        /// </summary>
        /// <param name="token">用于取消激活相关异步工作的取消令牌。</param>
        /// <returns>表示激活处理结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> ActiveAsync(CancellationToken token = default);

        /// <summary>
        /// 将停用事件沿入站方向传递到下一个处理器（连接断开或通道不可用时调用）。
        /// </summary>
        /// <param name="token">用于取消停用相关异步工作的取消令牌。</param>
        /// <returns>表示停用处理结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> InactiveAsync(CancellationToken token = default);

        /// <summary>
        /// 向下一个处理器发起连接请求并沿着出站方向传递该事件。
        /// </summary>
        /// <param name="remoteAddress">目标远端终结点（不能为空）。</param>
        /// <param name="localAddress">本地绑定地址（可为 <c>null</c> 表示不指定）。</param>
        /// <param name="token">用于取消连接过程的取消令牌。</param>
        /// <returns>表示连接结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> ConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default);

        /// <summary>
        /// 向下一个处理器发起断开请求并沿着出站方向传递该事件。
        /// </summary>
        /// <param name="token">用于取消断开过程的取消令牌。</param>
        /// <returns>表示断开结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> DisconnectAsync(CancellationToken token = default);

        /// <summary>
        /// 向下一个处理器发起绑定操作（例如监听或绑定本地端口）并传递该事件。
        /// </summary>
        /// <param name="localAddress">要绑定的本地终结点（不能为空）。</param>
        /// <param name="token">用于取消绑定过程的取消令牌。</param>
        /// <returns>表示绑定结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> BindAsync(EndPoint localAddress, CancellationToken token = default);

        /// <summary>
        /// 向下一个处理器发起关闭请求并传递该事件。关闭通常意味着更彻底的资源释放。
        /// </summary>
        /// <param name="token">用于取消关闭过程的取消令牌。</param>
        /// <returns>表示关闭结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> CloseAsync(CancellationToken token = default);

        /// <summary>
        /// 当管道中某个处理器捕获异常时，将异常沿管线传递以便上层处理器决定如何处理。
        /// </summary>
        /// <param name="exception">捕获到的异常实例。</param>
        /// <returns>返回一个 <see cref="Result"/>，表示异常是否被处理（成功表示已处理）。</returns>
        Result ExceptionCaught(Exception exception);

        /// <summary>
        /// 将入站数据沿入站方向传递到下一个参与入站处理的处理器。
        /// </summary>
        /// <param name="cache">包含一个或多个待处理缓冲区的 <see cref="ValueCache"/>。</param>
        /// <param name="token">用于取消入站处理的取消令牌。</param>
        /// <returns>表示入站处理结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> InboundHandleAsync(ValueCache cache, CancellationToken token = default);

        /// <summary>
        /// 将出站数据沿出站方向传递到下一个参与出站处理的处理器（通常最终到达传输层）。
        /// </summary>
        /// <param name="cache">包含要发送的数据缓冲的 <see cref="ValueCache"/>。</param>
        /// <param name="token">用于取消出站处理的取消令牌。</param>
        /// <returns>表示出站处理结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        ValueTask<Result> OutboundHandleAsync(ValueCache cache, CancellationToken token = default);
    }
}