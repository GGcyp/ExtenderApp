using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端插件：用于拦截连接生命周期与收发数据管道（例如为数据添加/剥离网络头）。
    /// </summary>
    public interface ILinkClientPlugin<TLinker> where TLinker : ILinker
    {
        /// <summary>
        /// 插件附加到客户端（初始化/获取服务等）。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="token">取消令牌。</param>
        void OnAttach(ILinkClient<TLinker> client);

        /// <summary>
        /// 插件从客户端分离（释放资源等）。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="token">取消令牌。</param>
        void OnDetach(ILinkClient<TLinker> client);

        /// <summary>
        /// 连接前（尚未发起连接）。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="remoteEndPoint">目标终结点。</param>
        /// <param name="token">取消令牌。</param>
        void OnConnecting(ILinkClient<TLinker> client, EndPoint remoteEndPoint);

        /// <summary>
        /// 连接后（已建立连接）。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="remoteEndPoint">目标终结点。</param>
        /// <param name="token">取消令牌。</param>
        void OnConnected(ILinkClient<TLinker> client, EndPoint remoteEndPoint, Exception exception);

        /// <summary>
        /// 断开前（即将断开）。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="token">取消令牌。</param>
        void OnDisconnecting(ILinkClient<TLinker> client);

        /// <summary>
        /// 断开后（已断开）。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="error">若因错误断开，这里携带异常；正常断开为 null。</param>
        /// <param name="token">取消令牌。</param>
        void OnDisconnected(ILinkClient<TLinker> client, Exception? error);

        /// <summary>
        /// 发送前：可对业务负载进行包装（例如添加网络头）。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>最终要发送的字节（可为同一实例或新缓冲）。</returns>
        void OnSend(ILinkClient<TLinker> client, ref LinkClientPluginSendMessage message);

        /// <summary>
        /// 接收后：可对收到的数据进行解包（例如移除网络头并返回业务负载）。
        /// </summary>
        /// <param name="client">客户端实例。</param>
        /// <param name="received">一次收到的原始字节（可能包含头部）。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>业务负载（解包后的字节）。</returns>
        void OnReceive(ILinkClient<TLinker> client, ref LinkClientPluginReceiveMessage message);
    }
}
