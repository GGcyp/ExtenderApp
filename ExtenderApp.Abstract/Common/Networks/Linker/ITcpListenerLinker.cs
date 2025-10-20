using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 面向传入 TCP 连接的监听抽象。
    /// 对外暴露：绑定本地端点、开始监听、以及“新连接接入”事件。
    /// 实现通常基于 <see cref="Socket"/> 构建，新连接会被包装为 <see cref="ITcpLinker"/> 后发布。
    /// </summary>
    public interface ITcpListenerLinker : IDisposable
    {
        /// <summary>
        /// 当前监听器的本地终结点（本地地址与端口）。
        /// 在成功 <see cref="Bind(EndPoint)"/> 且开始监听后有效；未绑定或未监听时可能为 null。
        /// </summary>
        EndPoint? ListenerPoint { get; }

        /// <summary>
        /// 当有新连接被接受时触发。
        /// 事件参数为包装完成的 <see cref="ITcpLinker"/> 实例。
        /// </summary>
        /// <remarks>
        /// - 线程上下文：通常在线程池线程上触发；请避免在处理器中执行耗时/阻塞操作。<br/>
        /// - 异常：订阅方抛出的异常应自行处理；实现一般会隔离异常以保证接入循环不中断。<br/>
        /// - 语义：该事件仅用于通知，不提供背压；若需按消费速度取连接，请在实现层做队列/调度。
        /// </remarks>
        event EventHandler<ITcpLinker>? OnAccept;

        /// <summary>
        /// 绑定本地终结点。
        /// </summary>
        /// <param name="endPoint">要绑定的本地地址与端口。</param>
        /// <exception cref="ArgumentNullException"><paramref name="endPoint"/> 为 null。</exception>
        /// <exception cref="SocketException">底层套接字绑定失败。</exception>
        void Bind(EndPoint endPoint);

        /// <summary>
        /// 开始监听传入连接。
        /// </summary>
        /// <param name="backlog">挂起连接队列的最大长度（传递给 <see cref="Socket.Listen(int)"/>）。</param>
        /// <remarks>
        /// - 通常需要先调用 <see cref="Bind(EndPoint)"/> 再调用本方法；<br/>
        /// - 调用后，新的连接接入将通过 <see cref="OnAccept"/> 事件通知订阅者。
        /// </remarks>
        /// <exception cref="InvalidOperationException">未绑定即开始监听，或实现不允许的状态。</exception>
        /// <exception cref="SocketException">底层开始监听失败。</exception>
        void Listen(int backlog = 10);
    }
}
