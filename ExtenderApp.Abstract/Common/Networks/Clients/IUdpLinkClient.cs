using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// UDP 客户端侧的链路抽象，扩展自 <see cref="ILinkClientAwareSender{TUdpLinker}"/> 与 <see cref="IUdpLinker"/>。
    /// 提供基于“泛型对象→字节”格式化后按指定远端发送的能力（单次数据报发送），支持同步与异步两种形式。
    /// </summary>
    public interface IUdpLinkClient : ILinkClientAwareSender<IUdpLinkClient>, IUdpLinker
    {
        /// <summary>
        /// 同步（立即）向指定远端发送一个泛型业务对象（单个 UDP 数据报）。
        /// </summary>
        /// <typeparam name="T">要发送的业务对象类型；必须已在 <see cref="ILinkClientAwareSender{IUdpLinkClient}.FormatterManager"/> 中注册对应格式化器。</typeparam>
        /// <param name="value">要发送的业务对象实例，不能为空。</param>
        /// <param name="endPoint">目标远端地址与端口（必须与底层套接字地址族匹配）。</param>
        /// <returns>
        /// 发送结果：包含实际发送的字节数与可能的 <see cref="System.Net.Sockets.SocketException"/>。
        /// </returns>
        /// <remarks>
        /// <para>流程：序列化 <typeparamref name="T"/> →（可选插件管线包装/Framer 编码）→ 调用底层 <c>SendTo</c>。</para>
        /// <para>命名说明：方法名含 “Async” 但返回同步结果；保持现有签名不变，仅文档指出其为同步操作。</para>
        /// <para>失败场景：未注册格式化器 / 对象已释放 / <paramref name="endPoint"/> 为 null / 底层套接字错误。</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">未设置或未找到类型 <typeparamref name="T"/> 的格式化器。</exception>
        /// <exception cref="ObjectDisposedException">客户端或底层链接器已释放。</exception>
        SocketOperationResult SendToAsync<T>(T value, EndPoint endPoint);

        /// <summary>
        /// 异步向指定远端发送一个泛型业务对象（单个 UDP 数据报），支持取消。
        /// </summary>
        /// <typeparam name="T">要发送的业务对象类型；必须已在 <see cref="ILinkClientAwareSender{IUdpLinkClient}.FormatterManager"/> 中注册。</typeparam>
        /// <param name="value">要发送的业务对象实例。</param>
        /// <param name="endPoint">目标远端地址与端口。</param>
        /// <param name="token">取消令牌；取消时应尽快结束并返回已取消结果或抛出 <see cref="OperationCanceledException"/>（由实现决定）。</param>
        /// <returns>
        /// 异步任务：完成后返回发送结果（字节数 / 错误）。
        /// </returns>
        /// <remarks>
        /// <para>适用于：潜在需等待底层异步 I/O（如 IOCP）或需要取消控制的场景。</para>
        /// <para>建议实现：使用 <see cref="TaskCreationOptions.RunContinuationsAsynchronously"/> 创建内部等待源，避免在完成线程内同步执行大量延续导致阻塞。</para>
        /// <para>与同步版本区别：支持 <paramref name="token"/> 取消；可在高并发场景下减少线程阻塞。</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">未设置或未找到类型 <typeparamref name="T"/> 的格式化器。</exception>
        /// <exception cref="ObjectDisposedException">客户端或底层链接器已释放。</exception>
        /// <exception cref="OperationCanceledException">当实现选择以抛出方式表达取消。</exception>
        ValueTask<SocketOperationResult> SendToAsync<T>(T value, EndPoint endPoint, CancellationToken token = default);
    }
}
