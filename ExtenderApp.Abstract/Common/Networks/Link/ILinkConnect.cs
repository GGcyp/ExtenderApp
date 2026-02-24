using System.Net;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供连接/断开连接能力的抽象接口。实现应负责建立与远端的会话并在断开时释放相关资源。
    /// </summary>
    /// <remarks>
    /// 实现应使用返回/异常语义明确区分可恢复的网络错误与不可恢复的编程错误（例如参数非法或对象已释放）。
    /// 对于异步方法，应尊重并响应 <see cref="CancellationToken"/>。
    /// </remarks>
    public interface ILinkConnect
    {
        /// <summary>
        /// 同步连接到指定远端终结点。
        /// </summary>
        /// <param name="remoteEndPoint">
        /// 目标远端终结点，不能为 <c>null</c>。
        /// </param>
        /// <remarks>
        /// - 对于面向连接的协议（例如 TCP），应建立到 <paramref
        ///   name="remoteEndPoint"/> 的连接。
        /// - 对于无连接协议（例如 UDP），实现可选择将套接字的默认远端设置为
        ///   <paramref name="remoteEndPoint"/> 或执行等效操作。
        /// - 若当前已处于连接状态，重复调用应抛出异常或由实现明确文档化其行为（例如忽略或重新连接）。
        /// - 参数非法（例如
        ///   <c>null</c>）或对象已释放应抛出相应的异常（例如 <see
        ///   cref="System.ArgumentNullException"/>、
        ///   <see cref="System.ObjectDisposedException"/>）。
        /// </remarks>
        void Connect(EndPoint remoteEndPoint);

        /// <summary>
        /// 同步连接到指定远端终结点。
        /// </summary>
        /// <param name="remoteEndPoint">
        /// 目标远端终结点，不能为 <c>null</c>。
        /// </param>
        /// <param name="localAddress">
        /// 本地终结点，不能为 <c>null</c>。
        /// </param>
        /// <remarks>
        /// - 对于面向连接的协议（例如 TCP），应建立到 <paramref
        ///   name="remoteEndPoint"/> 的连接。
        /// - 对于无连接协议（例如 UDP），实现可选择将套接字的默认远端设置为
        ///   <paramref name="remoteEndPoint"/> 或执行等效操作。
        /// - 若当前已处于连接状态，重复调用应抛出异常或由实现明确文档化其行为（例如忽略或重新连接）。
        /// - 参数非法（例如
        ///   <c>null</c>）或对象已释放应抛出相应的异常（例如 <see
        ///   cref="System.ArgumentNullException"/>、
        ///   <see cref="System.ObjectDisposedException"/>）。
        /// </remarks>
        void Connect(EndPoint remoteEndPoint, EndPoint localAddress);

        /// <summary>
        /// 异步连接到指定远端终结点。
        /// </summary>
        /// <param name="remoteEndPoint">目标远端终结点，不能为空。</param>
        /// <param name="token">
        /// 用于取消连接操作的 <see cref="CancellationToken"/>。实现应在取消时尽快中止连接并释放相关临时资源。
        /// </param>
        /// <returns>
        /// 一个表示异步连接操作的 <see
        /// cref="ValueTask"/>。在取消时可以抛出 <see
        /// cref="OperationCanceledException"/> 或以已取消的语义完成任务（应在实现文档中说明）。
        /// </returns>
        ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default);

        /// <summary>
        /// 异步连接到指定远端终结点。
        /// </summary>
        /// <param name="remoteEndPoint">目标远端终结点，不能为空。</param>
        /// <param name="localAddress">本地终结点，不能为空。</param>
        /// <param name="token">
        /// 用于取消连接操作的 <see cref="CancellationToken"/>。实现应在取消时尽快中止连接并释放相关临时资源。
        /// </param>
        /// <returns>
        /// 一个表示异步连接操作的 <see
        /// cref="ValueTask"/>。在取消时可以抛出 <see
        /// cref="OperationCanceledException"/> 或以已取消的语义完成任务（应在实现文档中说明）。
        /// </returns>
        ValueTask ConnectAsync(EndPoint remoteEndPoint, EndPoint localAddress, CancellationToken token = default);

        /// <summary>
        /// 同步断开当前连接并释放底层会话资源。
        /// </summary>
        /// <remarks>
        /// - 对于 TCP，通常会执行优雅关闭（例如可先调用
        ///   <c>Shutdown</c> 再 <c>Close</c>）；实现可根据需求选择立即关闭或优雅关闭。
        /// - 对于 UDP，通常清除默认远端或关闭套接字。
        /// - 调用后对象可进入已断开或已释放状态；后续对已释放对象的操作应抛出
        ///   <see cref="System.ObjectDisposedException"/>（或由实现明确文档化）。
        /// </remarks>
        void Disconnect();

        /// <summary>
        /// 异步断开当前连接并释放底层会话资源。
        /// </summary>
        /// <param name="token">
        /// 用于取消断开过程的 <see cref="CancellationToken"/>。实现应尊重该令牌。
        /// </param>
        /// <returns>
        /// 一个表示异步断开操作的 <see
        /// cref="ValueTask"/>。在取消时可以抛出 <see
        /// cref="OperationCanceledException"/> 或以已取消的语义完成任务（应在实现文档中说明）。
        /// </returns>
        ValueTask DisconnectAsync(CancellationToken token = default);
    }
}