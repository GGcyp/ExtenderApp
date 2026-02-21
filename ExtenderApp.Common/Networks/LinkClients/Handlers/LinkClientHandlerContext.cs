using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Networks.LinkClients;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks
{
    internal abstract class LinkClientHandlerContext : DisposableObject, ILinkClientHandlerContext
    {
        #region SkipFlags

        private static readonly SkipFlags AllFlags = Enum.GetValues<SkipFlags>().Aggregate((a, b) => a | b);

        private static readonly ConcurrentDictionary<Type, SkipFlags> _skipDict = new();

        protected static SkipFlags GetSkipFlags<T>() where T : ILinkClientHandler
            => GetSkipFlags(typeof(T));

        protected static SkipFlags GetSkipFlags(Type handlerType)
            => _skipDict.GetOrAdd(handlerType, CreateSkipFlags);

        protected static SkipFlags GetSkipFlags(ILinkClientHandler handler)
            => GetSkipFlags(handler.GetType());

        private static SkipFlags CreateSkipFlags(Type handlerType)
        {
            SkipFlags flags = AllFlags;

            foreach (var method in handlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var skipAttr = method.GetCustomAttribute<SkipAttribute>(false);
                if (skipAttr != null)
                {
                    flags &= ~skipAttr.Flags;
                }
            }
            return flags;
        }

        #endregion SkipFlags

        public string Name { get; }

        public ILinkClientHandler Handler { get; }

        public SkipFlags HandlerSkipFlags { get; }

        private volatile LinkClientHandlerContext? next;
        private volatile LinkClientHandlerContext? prev;

        public LinkClientHandlerContext(string name, ILinkClientHandler handler) : this(name, handler, handler.GetType())
        {
        }

        public LinkClientHandlerContext(string name, ILinkClientHandler handler, Type handlerType)
        {
            Name = name;
            Handler = handler;
            HandlerSkipFlags = GetSkipFlags(handlerType);
            next = default!;
            prev = default!;
        }

        #region Invoke

        public ValueTask<Result> ConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => FindContextInbound()?.InvokeConnectAsync(remoteAddress, localAddress, token) ?? new ValueTask<Result>(Result.Success());

        private ValueTask<Result> InvokeConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => Handler.ConnectAsync(this, remoteAddress, localAddress, token);

        public ValueTask<Result> DisconnectAsync(CancellationToken token = default)
            => FindContextInbound()?.InvokeDisconnectAsync(token) ?? new ValueTask<Result>(Result.Success());

        private ValueTask<Result> InvokeDisconnectAsync(CancellationToken token)
            => Handler.DisconnectAsync(this, token);

        public ValueTask<Result> BindAsync(EndPoint localAddress, CancellationToken token = default)
            => FindContextOutbound()?.InvokeBindAsync(localAddress, token) ?? new ValueTask<Result>(Result.Success());

        private ValueTask<Result> InvokeBindAsync(EndPoint localAddress, CancellationToken token)
            => Handler.BindAsync(this, localAddress, token);

        public ValueTask<Result> CloseAsync(CancellationToken token = default)
        => FindContextOutbound()?.InvokeCloseAsync(token) ?? new ValueTask<Result>(Result.Success());

        private ValueTask<Result> InvokeCloseAsync(CancellationToken token)
            => Handler.CloseAsync(this, token);

        public ValueTask ExceptionCaught(Exception exception)
            => FindContextInbound()?.InvokeExceptionCaught(exception) ?? ValueTask.FromException(exception);

        private ValueTask InvokeExceptionCaught(Exception exception)
            => Handler.ExceptionCaught(this, exception);

        public ValueTask<Result<int>> InboundHandleAsync(ValueCache cache, CancellationToken token = default)
            => FindContextInbound()?.InvokeInboundHandleAsync(cache, token) ?? new ValueTask<Result<int>>(Result.Success(0));

        private ValueTask<Result<int>> InvokeInboundHandleAsync(ValueCache cache, CancellationToken token)
            => Handler.InboundHandleAsync(this, cache, token);

        public ValueTask<Result> OutboundHandleAsync(ValueCache cache, CancellationToken token = default)
            => FindContextOutbound()?.InvokeOutboundHandleAsync(cache, token) ?? new ValueTask<Result>(Result.Success());

        private ValueTask<Result> InvokeOutboundHandleAsync(ValueCache cache, CancellationToken token)
            => Handler.OutboundHandleAsync(this, cache, token);

        #endregion Invoke

        #region FindContext

        private LinkClientHandlerContext? FindContextInbound()
        {
            var ctx = this;
            do
            {
                ctx = ctx.next;
            } while (ctx != null && (ctx.HandlerSkipFlags & SkipFlags.Inbound) == SkipFlags.Inbound);
            return ctx;
        }

        private LinkClientHandlerContext? FindContextOutbound()
        {
            var ctx = this;
            do
            {
                ctx = ctx.prev;
            } while (ctx != null && (ctx.HandlerSkipFlags & SkipFlags.Outbound) == SkipFlags.Outbound);
            return ctx;
        }

        #endregion FindContext

        protected override void DisposeManagedResources()
        {
            Handler?.DisposeSafe();
        }
    }

    internal class LinkClientHandlerContext<T> : LinkClientHandlerContext where T : ILinkClientHandler
    {
        public LinkClientHandlerContext(string name, T handler) : base(name, handler, typeof(T))
        {
        }
    }
}