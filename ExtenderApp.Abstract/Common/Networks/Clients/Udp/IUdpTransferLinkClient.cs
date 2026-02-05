using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// UDP 客户端侧的链路抽象，扩展自 <see cref="ILinkClientAwareSender{TUdpLinker}"/> 与 <see cref="IUdpLinker"/>。
    /// 提供基于“泛型对象→字节”格式化后按指定远端发送的能力（单次数据报发送），支持同步与异步两种形式。
    /// </summary>
    public interface IUdpTransferLinkClient : ITransferLinkClient<IUdpLinker>
    {
        /// <summary>
        /// 将业务对象序列化并同步发送到指定的 UDP 远端终结点。
        /// </summary>
        /// <remarks>
        /// 此方法会阻塞调用线程，直到数据发送完成或失败。
        /// 实现应通过 <see cref="ILinkClientAwareSender{TLinkClient}.FormatterManager"/> 查找并使用对应的格式化器来序列化 <paramref name="value"/>。
        /// 注意：尽管方法名为 "SendToAsync"，但其返回类型 <see cref="Result{T}"/> 表明它是一个同步操作。
        /// </remarks>
        /// <typeparam name="T">要发送的业务对象类型。</typeparam>
        /// <param name="value">要发送的业务对象实例。</param>
        /// <param name="endPoint">目标远端终结点。</param>
        /// <returns>一个 <see cref="Result{T}"/> 实例，其中包含操作结果，例如成功发送的字节数或失败时的异常信息。</returns>
        Result<SocketOperationValue> SendToAsync<T>(T value, EndPoint endPoint);

        /// <summary>
        /// 将业务对象序列化并异步发送到指定的 UDP 远端终结点。
        /// </summary>
        /// <remarks>
        /// 此方法为非阻塞操作。实现应通过 <see cref="ILinkClientAwareSender{TLinkClient}.FormatterManager"/> 查找并使用对应的格式化器来序列化 <paramref name="value"/>。
        /// </remarks>
        /// <typeparam name="T">要发送的业务对象类型。</typeparam>
        /// <param name="value">要发送的业务对象实例。</param>
        /// <param name="endPoint">目标远端终结点。</param>
        /// <param name="token">用于取消异步操作的令牌。</param>
        /// <returns>一个表示异步发送操作的 <see cref="ValueTask{TResult}"/>。其结果是一个 <see cref="Result{T}"/>，其中包含操作结果。</returns>
        ValueTask<Result<SocketOperationValue>> SendToAsync<T>(T value, EndPoint endPoint, CancellationToken token = default);
    }
}