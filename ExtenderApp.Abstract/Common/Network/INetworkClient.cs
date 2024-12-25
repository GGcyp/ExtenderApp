using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义了一个网络客户端接口。
    /// </summary>
    public interface INetworkClient
    {
        /// <summary>
        /// 异步发送网络请求。
        /// </summary>
        /// <param name="request">网络请求对象。</param>
        /// <returns>返回一个包含请求结果的Task对象。</returns>
        Task<object> SendAsync(NetworkRequest request);
    }
}
