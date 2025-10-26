using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 连接客户端插件基类，提供默认空实现。
    /// </summary>
    public abstract class LinkClientPluginBase<TLinkClient> : ILinkClientPlugin<TLinkClient>
        where TLinkClient : ILinkClientAwareSender<TLinkClient>
    {
        public virtual void OnAttach(TLinkClient client)
        {
        }

        public virtual void OnConnected(TLinkClient client, EndPoint remoteEndPoint, Exception? exception)
        {
        }

        public virtual void OnConnecting(TLinkClient client, EndPoint remoteEndPoint)
        {
        }

        public virtual void OnDetach(TLinkClient client)
        {
        }

        public virtual void OnDisconnected(TLinkClient client, Exception? error)
        {
        }

        public virtual void OnDisconnecting(TLinkClient client)
        {
        }

        public virtual void OnReceive(TLinkClient client, ref LinkClientPluginReceiveMessage message)
        {
        }

        public virtual void OnSend(TLinkClient client, ref LinkClientPluginSendMessage message)
        {
        }
    }
}