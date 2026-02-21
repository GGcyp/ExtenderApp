using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 链路处理器基类，提供可选的默认实现。
    /// </summary>
    public abstract class LinkClientHandler : DisposableObject, ILinkClientHandler
    {
        ///<inheritdoc/>
        [Skip(SkipFlags.Added)]
        public virtual void Added(ILinkClientHandlerContext context)
        {
        }

        ///<inheritdoc/>
        [Skip(SkipFlags.Removed)]
        public virtual void Removed(ILinkClientHandlerContext context)
        {
        }

        ///<inheritdoc/>
        [Skip(SkipFlags.Active)]
        public virtual void Active(ILinkClientHandlerContext context)
        {
        }

        ///<inheritdoc/>
        [Skip(SkipFlags.Inactive)]
        public virtual void Inactive(ILinkClientHandlerContext context)
        {
        }

        ///<inheritdoc/>
        [Skip(SkipFlags.Close)]
        public virtual ValueTask<Result> CloseAsync(ILinkClientHandlerContext context, CancellationToken token = default)
            => context.CloseAsync();

        ///<inheritdoc/>
        [Skip(SkipFlags.Connect)]
        public virtual ValueTask<Result> ConnectAsync(ILinkClientHandlerContext context, EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => context.ConnectAsync(remoteAddress, localAddress);

        ///<inheritdoc/>
        [Skip(SkipFlags.Disconnect)]
        public virtual ValueTask<Result> DisconnectAsync(ILinkClientHandlerContext context, CancellationToken token = default)
            => context.DisconnectAsync();

        ///<inheritdoc/>
        [Skip(SkipFlags.ExceptionCaught)]
        public virtual ValueTask ExceptionCaught(ILinkClientHandlerContext context, Exception exception)
            => context.ExceptionCaught(exception);

        ///<inheritdoc/>
        [Skip(SkipFlags.Bind)]
        public virtual ValueTask<Result> BindAsync(ILinkClientHandlerContext context, EndPoint localAddress, CancellationToken token = default)
            => context.BindAsync(localAddress);

        ///<inheritdoc/>
        [Skip(SkipFlags.InboundHandle)]
        public virtual ValueTask<Result<int>> InboundHandleAsync(ILinkClientHandlerContext context, ValueCache cache, CancellationToken token = default)
            => context.InboundHandleAsync(cache, token);

        ///<inheritdoc/>
        [Skip(SkipFlags.OutboundHandle)]
        public virtual ValueTask<Result> OutboundHandleAsync(ILinkClientHandlerContext context, ValueCache cache, CancellationToken token = default)
            => context.OutboundHandleAsync(cache, token);
    }
}