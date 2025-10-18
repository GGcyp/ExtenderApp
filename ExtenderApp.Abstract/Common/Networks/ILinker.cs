using System.Buffers;
using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个可进行“连接/断开/发送/接收”的链接抽象。
    /// </summary>
    /// <remarks>
    /// 约定与建议：
    /// - 线程安全：实现应保证发送与接收方法在并发调用时的正确性（常见做法是内部串行化同类操作）。<br/>
    /// - 资源管理：实现必须在 <see cref="IDisposable.Dispose"/> 中释放底层连接与内部资源。<br/>
    /// - TCP/UDP 兼容：<see cref="NoDelay"/> 仅对 TCP 生效；对 UDP 可忽略。<br/>
    /// - 统计与限流：通过 <see cref="CapacityLimiter"/> 和 <see cref="ValueCounter"/> 为上层提供限流与统计支持。
    /// </remarks>
    public interface ILinker : IDisposable
    {
        /// <summary>
        /// 链接是否处于已连接状态。
        /// </summary>
        /// <remarks>
        /// 对 TCP 表示底层套接字的连接状态；对 UDP（未 Connect）可始终为 true 或由实现定义。
        /// </remarks>
        bool Connected { get; }

        /// <summary>
        /// 本地终结点（本地地址与端口）。
        /// </summary>
        EndPoint? LocalEndPoint { get; }

        /// <summary>
        /// 远端终结点（对端地址与端口）。
        /// </summary>
        /// <remarks>
        /// 对 UDP 若未调用 Connect，可能为 null。
        /// </remarks>
        EndPoint? RemoteEndPoint { get; }

        /// <summary>
        /// 容量闸门（配额控制），用于对发送/接收操作按字节数进行限流。
        /// </summary>
        CapacityLimiter CapacityLimiter { get; }

        /// <summary>
        /// 发送统计计数器（可按周期结算）。
        /// </summary>
        ValueCounter SendCounter { get; }

        /// <summary>
        /// 接收统计计数器（可按周期结算）。
        /// </summary>
        ValueCounter ReceiveCounter { get; }

        /// <summary>
        /// 同步连接到指定远端。
        /// </summary>
        /// <param name="remoteEndPoint">目标远端终结点。</param>
        /// <remarks>
        /// - 对 TCP：建立连接；对 UDP：可将默认远端绑定为 <paramref name="remoteEndPoint"/>（实现可选）。<br/>
        /// - 已连接状态再次调用应抛出异常或由实现自行定义行为。
        /// </remarks>
        void Connect(EndPoint remoteEndPoint);

        /// <summary>
        /// 异步连接到指定远端。
        /// </summary>
        /// <param name="remoteEndPoint">目标远端终结点。</param>
        /// <param name="token">取消令牌。取消时应终止正在进行的连接过程。</param>
        ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default);

        /// <summary>
        /// 同步断开连接并释放底层会话。
        /// </summary>
        /// <remarks>
        /// - 对 TCP：通常执行优雅关闭（实现可选：<c>Shutdown</c> 后 <c>Close</c>）；<br/>
        /// - 对 UDP：清除默认远端或关闭套接字（由实现定义）。
        /// </remarks>
        void Disconnect();

        /// <summary>
        /// 异步断开连接并释放底层会话。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        ValueTask DisconnectAsync(CancellationToken token = default);

        /// <summary>
        /// 同步接收数据到指定缓冲区。
        /// </summary>
        /// <param name="memory">可写缓冲区，用于承载本次接收的数据。</param>
        /// <returns>
        /// 本次操作的结果，包含已接收字节数、远端地址（若适用）以及底层错误信息。
        /// </returns>
        /// <remarks>
        /// - TCP：返回 0 通常表示对端优雅关闭；可能不足额读取。<br/>
        /// - UDP：若缓冲不足可能被截断（可通过底层标志识别，取决于实现）。
        /// </remarks>
        SocketOperationResult Receive(Memory<byte> memory);

        /// <summary>
        /// 异步接收数据到指定缓冲区。
        /// </summary>
        /// <param name="memory">可写缓冲区，用于承载本次接收的数据。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>本次操作的结果，包含已接收字节数等。</returns>
        ValueTask<SocketOperationResult> ReceiveAsync(Memory<byte> memory, CancellationToken token = default);

        /// <summary>
        /// 同步发送指定缓冲区的数据。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <returns>本次操作的结果，包含已发送字节数等。</returns>
        /// <remarks>
        /// TCP 可能部分发送；若需确保全部发送完毕，应在调用方循环直至耗尽缓冲。
        /// </remarks>
        SocketOperationResult Send(Memory<byte> memory);

        /// <summary>
        /// 异步发送指定缓冲区的数据。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>本次操作的结果，包含已发送字节数等。</returns>
        /// <remarks>
        /// TCP 可能部分发送；若需确保全部发送完毕，应在调用方循环直至耗尽缓冲。
        /// </remarks>
        ValueTask<SocketOperationResult> SendAsync(Memory<byte> memory, CancellationToken token = default);
    }
}
