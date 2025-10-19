using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 监听器链接器接口：对外暴露“绑定端点”“开始监听”与“新连接接入事件”。
    /// 实现者通常基于 <see cref="Socket"/> 构建，接入后将 <see cref="Socket"/> 包装为 <typeparamref name="T"/> 并通过事件发布。
    /// </summary>
    /// <typeparam name="T">连接器类型，必须实现 <see cref="ILinker"/>。</typeparam>
    public interface IListenerLinker<T> : IDisposable where T : ILinker
    {
        /// <summary>
        /// 当前监听器的本地终结点（本地地址与端口）。
        /// 在 <see cref="Bind(EndPoint)"/> 并开始监听后有效；未绑定或未开始监听时可能为 null。
        /// </summary>
        EndPoint? ListenerPoint { get; }

        /// <summary>
        /// 当有新连接被接受时触发。
        /// 事件参数为包装完成的 <typeparamref name="T"/> 实例。
        /// </summary>
        /// <remarks>
        /// - 线程上下文：一般在线程池线程上触发；请避免在处理器中执行耗时/阻塞操作。<br/>
        /// - 异常：事件处理器抛出的异常应由订阅方自行处理；实现者通常会隔离异常以保证接入循环不中断。<br/>
        /// - 语义：本事件用于通知，不提供“背压”能力；若需要按消费速度取连接，请在实现层使用 await 接口并自行调度。
        /// </remarks>
        event EventHandler<T>? OnAccept;

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
        /// - 一般需要先调用 <see cref="Bind(EndPoint)"/> 再调用此方法。<br/>
        /// - 调用后，新的连接接入将通过 <see cref="OnAccept"/> 事件通知订阅者。
        /// </remarks>
        /// <exception cref="InvalidOperationException">未绑定即开始监听，或实现不允许的状态。实际限制视实现而定。</exception>
        /// <exception cref="SocketException">底层开始监听失败。</exception>
        void Listen(int backlog = 10);
    }
}
