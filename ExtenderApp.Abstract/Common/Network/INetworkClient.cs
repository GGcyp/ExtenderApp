using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义了一个网络客户端接口。
    /// </summary>
    public interface INetworkClient
    {
        /// <summary>
        /// 异步发送网络请求消息并返回响应消息。
        /// </summary>
        /// <param name="message">要发送的网络请求消息。</param>
        /// <param name="option">指定何时完成请求。</param>
        /// <returns>返回一个包含网络响应消息的 <see cref="ValueTask{NetworkResponseMessage}"/>。</returns>
        ValueTask<NetworkResponseMessage> SendAsync(NetworkRequestMessage message, HttpCompletionOption option);
    }
}
