

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 网络服务接口
    /// </summary>
    public interface INetWorkService
    {
        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="message">要发送的消息对象</param>
        /// <returns>返回消息处理结果</returns>
        Task<object> SendAsync(object message);
    }
}
