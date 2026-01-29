using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 连接客户端插件基类，提供默认空实现。
    /// </summary>
    public abstract class LinkClientPlugin<TLinkClient> : DisposableObject, ILinkClientPlugin
    {
        protected ILinkClientAwareSender Linker { get; private set; }

        public LinkClientPlugin()
        {
            Linker = null!;
        }

        public virtual void OnAttach(ILinkClientAwareSender client)
        {
            Linker = client;
        }

        public virtual void OnDetach()
        {
        }

        public virtual void OnConnected(EndPoint remoteEndPoint, Exception? exception)
        {
        }

        public virtual void OnConnecting(EndPoint remoteEndPoint)
        {
        }

        public virtual void OnDisconnected(Exception? error)
        {
        }

        public virtual void OnDisconnecting()
        {
        }

        public virtual void OnReceive(SocketOperationValue operationValue, ref FrameContext frame)
        {
        }

        public virtual void OnSend(ref FrameContext frame)
        {
        }
    }
}