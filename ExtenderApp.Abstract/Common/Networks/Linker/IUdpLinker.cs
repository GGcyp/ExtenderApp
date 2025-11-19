using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个 UDP 链接器接口，继承自 <see cref="ITransportLinker"/>。
    /// </summary>
    public interface IUdpLinker : ILinker, ILinkBind
    {
        /// <summary>
        /// 向指定远端发送一个 UDP 数据报（未连接或临时覆盖默认远端时使用）。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="endPoint">目标远端地址与端口；其地址族需与底层套接字一致。</param>
        /// <returns>
        /// 返回本次操作的结果：包含已发送字节数或 <see cref="System.Net.Sockets.SocketException"/>（失败时）。
        /// </returns>
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
        ValueTask<SocketOperationResult> SendToAsync(Memory<byte> memory, EndPoint endPoint, CancellationToken token = default);
    }
}