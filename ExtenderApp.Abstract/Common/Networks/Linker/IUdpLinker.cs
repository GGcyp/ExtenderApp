using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个 UDP 链接器接口，继承自 <see cref="ILinker"/>。
    /// </summary>
    /// <remarks>
    /// 使用模式：
    /// <para>1) 已连接（单一对端）：调用 <see cref="ILinker.Connect(EndPoint)"/> 或 <see cref="ILinker.ConnectAsync(EndPoint, CancellationToken)"/> 设置默认远端，随后使用 <see cref="ILinker.Send(Memory{byte})"/> / <see cref="ILinker.Receive(Memory{byte})"/>（或其异步版本）。</para>
    /// <para>2) 未连接（多对端/通用接收）：调用 <see cref="Bind(EndPoint)"/> 绑定本地端口并使用 <see cref="ILinker.Receive(Memory{byte})"/>（或异步版本）接收任意来源数据报；向特定对端发送时使用 <see cref="SendTo(Memory{byte}, EndPoint)"/> 或 <see cref="SendToAsync(Memory{byte}, EndPoint, CancellationToken)"/>。</para>
    /// 注意：
    /// <para>- UDP 无“监听/接入”（Listen/Accept）语义；不要对 UDP 调用 Listen/Accept。</para>
    /// <para>- 若仅与单一对端通信，优先使用“已连接”模式；需要固定本地端口时，可先 <see cref="Bind(EndPoint)"/> 再 Connect。</para>
    /// </remarks>
    public interface IUdpLinker : ILinker
    {
        /// <summary>
        /// 绑定本地终结点（地址+端口）。
        /// </summary>
        /// <param name="endPoint">要绑定的本地地址与端口（如 127.0.0.1:12345）。</param>
        /// <remarks>
        /// <para>- 未连接模式下用于接收任意来源数据报；也可用于在已连接模式下指定固定的本地端口。</para>
        /// <para>- 一般应在首次收发前调用一次；重复绑定将抛出异常。</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">endPoint 为空。</exception>
        /// <exception cref="System.Net.Sockets.SocketException">底层套接字绑定失败（端口占用、权限不足、地址族不匹配等）。</exception>
        void Bind(EndPoint endPoint);

        /// <summary>
        /// 向指定远端发送一个 UDP 数据报（未连接或临时覆盖默认远端时使用）。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="endPoint">目标远端地址与端口；其地址族需与底层套接字一致。</param>
        /// <returns>
        /// 返回本次操作的结果：包含已发送字节数或 <see cref="System.Net.Sockets.SocketException"/>（失败时）。
        /// </returns>
        /// <remarks>
        /// <para>- 若 <paramref name="memory"/> 为空或长度为 0，结果中将携带 <c>SocketError.NoBufferSpaceAvailable</c>。</para>
        /// <para>- 当 <paramref name="endPoint"/> 为空或不合法时，结果中将携带 <c>SocketError.NotConnected</c> 或其他对应错误。</para>
        /// <para>- 若已调用 Connect，仍可使用本方法向不同远端发送（覆盖默认远端）。</para>
        /// </remarks>
        SocketOperationResult SendTo(Memory<byte> memory, EndPoint endPoint);

        /// <summary>
        /// 以异步方式向指定远端发送一个 UDP 数据报（未连接或临时覆盖默认远端时使用）。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="endPoint">目标远端地址与端口；其地址族需与底层套接字一致。</param>
        /// <param name="token">取消令牌；取消时结束等待并返回取消异常。</param>
        /// <returns>
        /// 返回本次操作的结果：包含已发送字节数或 <see cref="System.Net.Sockets.SocketException"/>（失败时）。
        /// </returns>
        /// <remarks>
        /// <para>- 若 <paramref name="memory"/> 为空或长度为 0，结果中将携带 <c>SocketError.NoBufferSpaceAvailable</c>。</para>
        /// <para>- 取消将通过 <see cref="OperationCanceledException"/> 体现；其它失败通过结果中的 <see cref="System.Net.Sockets.SocketException"/> 表达。</para>
        /// <para>- 若已调用 Connect，仍可使用本方法向不同远端发送（覆盖默认远端）。</para>
        /// </remarks>
        ValueTask<SocketOperationResult> SendToAsync(Memory<byte> memory, EndPoint endPoint, CancellationToken token = default);
    }
}
