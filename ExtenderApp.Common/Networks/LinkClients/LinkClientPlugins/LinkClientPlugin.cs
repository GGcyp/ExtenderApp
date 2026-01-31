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

        public virtual Result OnAttach(ILinkClientAwareSender client)
        {
            Linker = client;
            return Result.Success();
        }

        public virtual void OnDetach()
        {
        }

        public virtual Result OnConnected(EndPoint remoteEndPoint, Exception? exception)
        {
            return Result.Success();
        }

        public virtual Result OnConnecting(EndPoint remoteEndPoint)
        {
            return Result.Success();
        }

        public virtual Result OnDisconnected(Exception? error)
        {
            return Result.Success();
        }

        public virtual Result OnDisconnecting()
        {
            return Result.Success();
        }

        public virtual Result OnReceive(SocketOperationValue operationValue, ref FrameContext frame)
        {
            return Result.Success();
        }

        public virtual Result OnSend(ref FrameContext frame)
        {
            return Result.Success();
        }
    }
}