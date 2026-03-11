using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkChannels
{
    /// <summary>
    /// 链路处理器的基础实现。
    /// </summary>
    public abstract class LinkChannelHandler : DisposableObject, ILinkChannelHandler
    {
        ///<inheritdoc/>
        [Skip(SkipFlags.Added)]
        public virtual void Added(ILinkChannelHandlerContext context)
        {
        }

        ///<inheritdoc/>
        [Skip(SkipFlags.Removed)]
        public virtual void Removed(ILinkChannelHandlerContext context)
        {
        }

        ///<inheritdoc/>
        [Skip(SkipFlags.Active)]
        public virtual ValueTask<Result> ActiveAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
            => context.ActiveAsync(token);

        ///<inheritdoc/>
        [Skip(SkipFlags.Inactive)]
        public virtual ValueTask<Result> InactiveAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
            => context.InactiveAsync(token);

        ///<inheritdoc/>
        [Skip(SkipFlags.Close)]
        public virtual ValueTask<Result> CloseAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
            => context.CloseAsync(token);

        ///<inheritdoc/>
        [Skip(SkipFlags.Connect)]
        public virtual ValueTask<Result> ConnectAsync(ILinkChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => context.ConnectAsync(remoteAddress, localAddress, token);

        ///<inheritdoc/>
        [Skip(SkipFlags.Disconnect)]
        public virtual ValueTask<Result> DisconnectAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
            => context.DisconnectAsync(token);

        ///<inheritdoc/>
        [Skip(SkipFlags.ExceptionCaught)]
        public virtual Result ExceptionCaught(ILinkChannelHandlerContext context, Exception exception)
            => context.ExceptionCaught(exception);

        ///<inheritdoc/>
        [Skip(SkipFlags.Bind)]
        public virtual ValueTask<Result> BindAsync(ILinkChannelHandlerContext context, EndPoint localAddress, CancellationToken token = default)
            => context.BindAsync(localAddress, token);

        ///<inheritdoc/>
        [Skip(SkipFlags.InboundHandle)]
        public virtual ValueTask<Result> InboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
            => context.InboundHandleAsync(cache, token);

        ///<inheritdoc/>
        [Skip(SkipFlags.OutboundHandle)]
        public virtual ValueTask<Result> OutboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
            => context.OutboundHandleAsync(cache, token);
    }
}