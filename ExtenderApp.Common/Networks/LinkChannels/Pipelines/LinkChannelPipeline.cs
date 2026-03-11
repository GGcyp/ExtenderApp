using System.Collections;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkChannels
{
    /// <summary>
    /// LinkChannelPipeline实现，使用双向链表存储处理器上下文，支持动态添加、移除和替换处理器。
    /// </summary>
    internal sealed class LinkChannelPipeline : ILinkChannelPipeline, ILinkChannelHandlerContextOperations
    {
        /// <summary>
        /// 头哨兵节点，简化链表操作
        /// </summary>
        private readonly LinkChannelHandlerContext _head;

        /// <summary>
        /// 尾哨兵节点，简化链表操作
        /// </summary>
        private readonly LinkChannelHandlerContext _tail;

        private readonly LinkChannel _linkClient;

        public LinkChannelPipeline(LinkChannel linkClient)
        {
            _linkClient = linkClient;
            var linkerTransportHandler = new LinkerTransportHandler(linkClient);
            _head = new HeadLinkChannelHandlerContext(linkClient, linkerTransportHandler);
            _tail = new TailLinkChannelHandlerContext(linkClient);
            _head.Next = _tail;
            _tail.Prev = _head;
        }

        private static LinkChannelHandlerContext<T> CreateContext<T>(string name, ILinkChannel linkClient, T handler) where T : ILinkChannelHandler
        {
            return new LinkChannelHandlerContext<T>(name, linkClient, handler);
        }

        #region Pipeline Operations

        ///<inheritdoc/>
        public ILinkChannelPipeline AddAfter<T>(string baseName, string name, T handler) where T : ILinkChannelHandler
        {
            var ctx = _head;
            while (ctx != null && ctx.Name != baseName)
                ctx = ctx.Next!;
            if (ctx == null)
                throw new KeyNotFoundException($"未发现名为 {baseName} 的处理器");

            var newCtx = CreateContext(name, _linkClient, handler);
            var next = ctx.Next!;
            newCtx.Next = next;
            newCtx.Prev = ctx;
            ctx.Next = newCtx;
            next.Prev = newCtx;
            return this;
        }

        ///<inheritdoc/>
        public ILinkChannelPipeline AddBefore<T>(string baseName, string name, T handler) where T : ILinkChannelHandler
        {
            var ctx = _head.Next;
            while (ctx != null && ctx.Name != baseName)
                ctx = ctx.Next;
            if (ctx == null)
                throw new KeyNotFoundException($"未发现名为 {baseName} 的处理器");

            var prev = ctx.Prev!;
            var newCtx = CreateContext(name, _linkClient, handler);
            newCtx.Next = ctx;
            newCtx.Prev = prev;
            prev.Next = newCtx;
            ctx.Prev = newCtx;
            return this;
        }

        ///<inheritdoc/>
        public ILinkChannelPipeline AddFirst<T>(string name, T handler) where T : ILinkChannelHandler
        {
            var next = _head.Next!;
            var newCtx = CreateContext(name, _linkClient, handler);
            newCtx.Next = next;
            newCtx.Prev = _head;
            _head.Next = newCtx;
            next.Prev = newCtx;
            return this;
        }

        ///<inheritdoc/>
        public ILinkChannelPipeline AddLast<T>(string name, T handler) where T : ILinkChannelHandler
        {
            var prev = _tail.Prev!;
            var newCtx = CreateContext(name, _linkClient, handler);
            newCtx.Next = _tail;
            newCtx.Prev = prev;
            prev.Next = newCtx;
            _tail.Prev = newCtx;
            return this;
        }

        ///<inheritdoc/>
        public ILinkChannelPipeline Remove<T>(T handler) where T : ILinkChannelHandler
        {
            var ctx = _head.Next;
            while (ctx != null && ctx != _tail)
            {
                if (ReferenceEquals(ctx.Handler, handler))
                {
                    var prev = ctx.Prev!;
                    var next = ctx.Next!;
                    prev.Next = next;
                    next.Prev = prev;
                    ctx.Dispose();
                    break;
                }
                ctx = ctx.Next;
            }
            return this;
        }

        ///<inheritdoc/>
        public ILinkChannelHandler Remove(string name)
        {
            var ctx = _head.Next;
            while (ctx != null && ctx != _tail)
            {
                if (ctx.Name == name)
                {
                    var prev = ctx.Prev!;
                    var next = ctx.Next!;
                    prev.Next = next;
                    next.Prev = prev;
                    var handler = ctx.Handler;
                    ctx.Dispose();
                    return handler;
                }
                ctx = ctx.Next;
            }
            return null!;
        }

        ///<inheritdoc/>
        public ILinkChannelPipeline Replace<T>(string oldName, string newName, T newHandler) where T : ILinkChannelHandler
        {
            var ctx = _head.Next;
            while (ctx != null && ctx != _tail)
            {
                if (ctx.Name == oldName)
                {
                    var newCtx = CreateContext(newName, _linkClient, newHandler);
                    newCtx.Prev = ctx.Prev!;
                    newCtx.Next = ctx.Next!;
                    ctx.Prev!.Next = newCtx;
                    ctx.Next!.Prev = newCtx;
                    ctx.Dispose();
                    return this;
                }
                ctx = ctx.Next;
            }
            throw new KeyNotFoundException($"Handler not found: {oldName}");
        }

        ///<inheritdoc/>
        public ILinkChannelPipeline Replace<T>(ILinkChannelHandler oldHandler, string newName, T newHandler) where T : ILinkChannelHandler
        {
            var ctx = _head.Next;
            while (ctx != null && ctx != _tail)
            {
                if (ReferenceEquals(ctx.Handler, oldHandler))
                {
                    var newCtx = CreateContext(newName, _linkClient, newHandler);
                    newCtx.Prev = ctx.Prev!;
                    newCtx.Next = ctx.Next!;
                    ctx.Prev!.Next = newCtx;
                    ctx.Next!.Prev = newCtx;
                    ctx.Dispose();
                    return this;
                }
                ctx = ctx.Next;
            }
            throw new KeyNotFoundException("Handler not found");
        }

        #endregion Pipeline Operations

        #region Handlers Operations

        ///<inheritdoc/>
        public ValueTask<Result> ActiveAsync(CancellationToken token = default)
            => _head.ActiveAsync(token);

        ///<inheritdoc/>
        public ValueTask<Result> InactiveAsync(CancellationToken token = default)
            => _head.InactiveAsync(token);

        ///<inheritdoc/>
        public ValueTask<Result> BindAsync(EndPoint localAddress, CancellationToken token = default)
            => _tail.BindAsync(localAddress, token);

        ///<inheritdoc/>
        public ValueTask<Result> CloseAsync(CancellationToken token = default)
            => _tail.CloseAsync(token);

        ///<inheritdoc/>
        public ValueTask<Result> ConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => _tail.ConnectAsync(remoteAddress, localAddress, token);

        ///<inheritdoc/>
        public ValueTask<Result> DisconnectAsync(CancellationToken token = default)
            => _tail.DisconnectAsync(token);

        ///<inheritdoc/>
        public Result ExceptionCaught(Exception exception)
            => _head.ExceptionCaught(exception);

        ///<inheritdoc/>
        public ValueTask<Result> InboundHandleAsync(ValueCache cache, CancellationToken token = default)
            => _head.InboundHandleAsync(cache, token);

        ///<inheritdoc/>
        public ValueTask<Result> OutboundHandleAsync(ValueCache cache, CancellationToken token = default)
            => _tail.OutboundHandleAsync(cache, token);

        #endregion Handlers Operations

        #region Enumerable

        private struct Enumerator : IEnumerator<ILinkChannelHandler>
        {
            private LinkChannelHandlerContext? _currentCtx;
            private readonly LinkChannelHandlerContext? _startCtx;
            private readonly LinkChannelHandlerContext _tail;
            private ILinkChannelHandler? _current;

            public Enumerator(LinkChannelHandlerContext? start, LinkChannelHandlerContext tail)
            {
                _startCtx = start;
                _currentCtx = start;
                _tail = tail;
                _current = null;
            }

            public ILinkChannelHandler Current => _current!;

            object IEnumerator.Current => Current!;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var ctx = _currentCtx;
                if (ctx != null && !ReferenceEquals(ctx, _tail))
                {
                    _current = ctx.Handler;
                    _currentCtx = ctx.Next;
                    return true;
                }
                _current = null;
                return false;
            }

            public void Reset()
            {
                _currentCtx = _startCtx;
                _current = null;
            }
        }

        public IEnumerator<ILinkChannelHandler> GetEnumerator()
            => new Enumerator(_head.Next, _tail);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        #endregion Enumerable

        #region HeadLinkChannelHandlerContext

        /// <summary>
        /// 管道头部的处理器上下文，用于将 <see cref="HeadLinkChannelHandler"/> 绑定到特定的上下文名称。 该上下文在管道构建时作为第一项，承载默认的头部处理器实例。
        /// </summary>
        private sealed class HeadLinkChannelHandlerContext : LinkChannelHandlerContext
        {
            /// <summary>
            /// 管道中头部的链接客户端处理器，作为链式处理的起始节点。
            /// </summary>
            private sealed class HeadLinkChannelHandler : LinkChannelHandler
            {
                private readonly ILinkChannelHandler _handler;

                public HeadLinkChannelHandler(ILinkChannelHandler handler) => _handler = handler;

                public override ValueTask<Result> ConnectAsync(ILinkChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default) => _handler.ConnectAsync(context, remoteAddress, localAddress, token);

                public override ValueTask<Result> DisconnectAsync(ILinkChannelHandlerContext context, CancellationToken token = default) => _handler.DisconnectAsync(context, token);

                public override ValueTask<Result> InboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default) => _handler.InboundHandleAsync(context, cache, token);

                public override ValueTask<Result> OutboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default) => _handler.OutboundHandleAsync(context, cache, token);

                public override Result ExceptionCaught(ILinkChannelHandlerContext context, Exception exception) => Result.FromException(exception, "链路客户端管道头部处理器捕获异常");
            }

            /// <summary>
            /// 使用指定名称创建 <see cref="HeadLinkChannelHandlerContext"/> 实例，并将默认的头部处理器关联到该上下文。
            /// </summary>
            public HeadLinkChannelHandlerContext(LinkChannel linkClient, ILinkChannelHandler handler) : base("Head", linkClient, new HeadLinkChannelHandler(handler), typeof(HeadLinkChannelHandler))
            {
            }
        }

        #endregion HeadLinkChannelHandlerContext

        #region TailLinkChannelHandlerContext

        /// <summary>
        /// 管道尾部的处理器上下文，用于将 <see cref="TailLinkChannelHandler"/> 绑定到尾部上下文。 该上下文在管道构建时作为最后一项，承载默认的尾部处理器实例。
        /// </summary>
        private sealed class TailLinkChannelHandlerContext : LinkChannelHandlerContext
        {
            /// <summary>
            /// 管道中尾部的链接客户端处理器，作为链式处理的终结节点。
            /// </summary>
            private sealed class TailLinkChannelHandler : LinkChannelHandler
            {
                public override Result ExceptionCaught(ILinkChannelHandlerContext context, Exception exception)
                {
                    return Result.FromException(exception, "链路客户端管道尾部处理器捕获异常");
                }
            }

            private static readonly TailLinkChannelHandler tailLinkChannelHandler = new TailLinkChannelHandler();

            /// <summary>
            /// 创建默认的尾部上下文实例，名称为 "Tail"，并关联默认的尾部处理器。
            /// </summary>
            public TailLinkChannelHandlerContext(ILinkChannel linkClient) : base("Tail", linkClient, tailLinkChannelHandler, typeof(TailLinkChannelHandler))
            {
            }
        }

        #endregion TailLinkChannelHandlerContext
    }
}