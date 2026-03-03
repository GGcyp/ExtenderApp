using System.Collections;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// LinkClientPipeline实现，使用双向链表存储处理器上下文，支持动态添加、移除和替换处理器。
    /// </summary>
    internal sealed class LinkClientPipeline : ILinkClientPipeline, ILinkClientHandlerContextOperations
    {
        /// <summary>
        /// 头哨兵节点，简化链表操作
        /// </summary>
        private readonly LinkClientHandlerContext _head;

        /// <summary>
        /// 尾哨兵节点，简化链表操作
        /// </summary>
        private readonly LinkClientHandlerContext _tail;

        private readonly ILinkClient _linkClient;

        public LinkClientPipeline(ILinkClient linkClient)
        {
            _linkClient = linkClient;
            _head = new HeadLinkClientHandlerContext(linkClient);
            _tail = new TailLinkClientHandlerContext(linkClient);
            _head.Next = _tail;
            _tail.Prev = _head;
        }

        private static LinkClientHandlerContext<T> CreateContext<T>(string name, ILinkClient linkClient, T handler) where T : ILinkClientHandler
        {
            return new LinkClientHandlerContext<T>(name, linkClient, handler);
        }

        #region Pipeline Operations

        ///<inheritdoc/>
        public ILinkClientPipeline AddAfter<T>(string baseName, string name, T handler) where T : ILinkClientHandler
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
        public ILinkClientPipeline AddBefore<T>(string baseName, string name, T handler) where T : ILinkClientHandler
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
        public ILinkClientPipeline AddFirst<T>(string name, T handler) where T : ILinkClientHandler
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
        public ILinkClientPipeline AddLast<T>(string name, T handler) where T : ILinkClientHandler
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
        public ILinkClientPipeline Remove<T>(T handler) where T : ILinkClientHandler
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
        public ILinkClientHandler Remove(string name)
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
        public ILinkClientPipeline Replace<T>(string oldName, string newName, T newHandler) where T : ILinkClientHandler
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
        public ILinkClientPipeline Replace<T>(ILinkClientHandler oldHandler, string newName, T newHandler) where T : ILinkClientHandler
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
        public ValueTask<Result> BindAsync(EndPoint localAddress, CancellationToken token = default)
            => _tail.BindAsync(localAddress, token);

        ///<inheritdoc/>
        public ValueTask<Result> CloseAsync(CancellationToken token = default)
            => _tail.CloseAsync(token);

        ///<inheritdoc/>
        public ValueTask<Result> ConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => _head.ConnectAsync(remoteAddress, localAddress, token);

        ///<inheritdoc/>
        public ValueTask<Result> DisconnectAsync(CancellationToken token = default)
            => _head.DisconnectAsync(token);

        ///<inheritdoc/>
        public void ExceptionCaught(Exception exception)
            => _head.ExceptionCaught(exception);

        ///<inheritdoc/>
        public ValueTask<Result<int>> InboundHandleAsync(ValueCache cache, CancellationToken token = default)
            => _head.InboundHandleAsync(cache, token);

        ///<inheritdoc/>
        public ValueTask<Result> OutboundHandleAsync(ValueCache cache, CancellationToken token = default)
            => _tail.OutboundHandleAsync(cache, token);

        #endregion Handlers Operations

        #region Enumerable

        private struct Enumerator : IEnumerator<ILinkClientHandler>
        {
            private LinkClientHandlerContext? _currentCtx;
            private readonly LinkClientHandlerContext? _startCtx;
            private readonly LinkClientHandlerContext _tail;
            private ILinkClientHandler? _current;

            public Enumerator(LinkClientHandlerContext? start, LinkClientHandlerContext tail)
            {
                _startCtx = start;
                _currentCtx = start;
                _tail = tail;
                _current = null;
            }

            public ILinkClientHandler Current => _current!;

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

        public IEnumerator<ILinkClientHandler> GetEnumerator()
            => new Enumerator(_head.Next, _tail);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        #endregion Enumerable

        #region HeadLinkClientHandlerContext

        /// <summary>
        /// 管道头部的处理器上下文，用于将 <see cref="HeadLinkClientHandler"/> 绑定到特定的上下文名称。 该上下文在管道构建时作为第一项，承载默认的头部处理器实例。
        /// </summary>
        private class HeadLinkClientHandlerContext : LinkClientHandlerContext
        {
            /// <summary>
            /// 管道中头部的链接客户端处理器，作为链式处理的起始节点。
            /// </summary>
            private class HeadLinkClientHandler : LinkClientHandler
            {
                public override void ExceptionCaught(ILinkClientHandlerContext context, Exception exception)
                {
                    throw exception;
                }
            }

            /// <summary>
            /// 通用的头部处理器实例，作为管道头部的默认处理器，提供基础的连接和数据处理能力。 该实例在所有头部上下文中共享，确保一致的行为和资源利用。
            /// </summary>
            private static readonly HeadLinkClientHandler headLinkClientHandler = new HeadLinkClientHandler();

            /// <summary>
            /// 使用指定名称创建 <see cref="HeadLinkClientHandlerContext"/> 实例，并将默认的头部处理器关联到该上下文。
            /// </summary>
            /// <param name="name">上下文名称，用于标识该处理器在管道中的位置或用途。</param>
            public HeadLinkClientHandlerContext(ILinkClient linkClient) : base("Head", linkClient, headLinkClientHandler, typeof(HeadLinkClientHandler))
            {
            }
        }

        #endregion HeadLinkClientHandlerContext

        #region TailLinkClientHandlerContext

        /// <summary>
        /// 管道尾部的处理器上下文，用于将 <see cref="TailLinkClientHandler"/> 绑定到尾部上下文。 该上下文在管道构建时作为最后一项，承载默认的尾部处理器实例。
        /// </summary>
        private class TailLinkClientHandlerContext : LinkClientHandlerContext
        {
            /// <summary>
            /// 管道中尾部的链接客户端处理器，作为链式处理的终结节点。
            /// </summary>
            private class TailLinkClientHandler : LinkClientHandler
            {
                public override void ExceptionCaught(ILinkClientHandlerContext context, Exception exception)
                {
                    throw exception;
                }
            }

            private static readonly TailLinkClientHandler tailLinkClientHandler = new TailLinkClientHandler();

            /// <summary>
            /// 创建默认的尾部上下文实例，名称为 "Tail"，并关联默认的尾部处理器。
            /// </summary>
            public TailLinkClientHandlerContext(ILinkClient linkClient) : base("Tail", linkClient, tailLinkClientHandler, typeof(TailLinkClientHandler))
            {
            }
        }

        #endregion TailLinkClientHandlerContext
    }
}