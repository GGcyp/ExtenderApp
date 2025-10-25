using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 连接客户端插件基类，提供默认空实现。
    /// </summary>
    public abstract class LinkClientPluginBase<TLinker> : ILinkClientPlugin<TLinker> where TLinker : ILinker
    {
        public virtual void OnAttach(ILinkClient<TLinker> client)
        {
        }

        public virtual void OnConnected(ILinkClient<TLinker> client, EndPoint remoteEndPoint, Exception exception)
        {
        }

        public virtual void OnConnecting(ILinkClient<TLinker> client, EndPoint remoteEndPoint)
        {
        }

        public virtual void OnDetach(ILinkClient<TLinker> client)
        {
        }

        public virtual void OnDisconnected(ILinkClient<TLinker> client, Exception? error)
        {
        }

        public virtual void OnDisconnecting(ILinkClient<TLinker> client)
        {
        }

        public virtual void OnReceive(ILinkClient<TLinker> client, ref LinkClientPluginReceiveMessage message)
        {
        }

        public virtual void OnSend(ILinkClient<TLinker> client, ref LinkClientPluginSendMessage message)
        {
        }
    }
}