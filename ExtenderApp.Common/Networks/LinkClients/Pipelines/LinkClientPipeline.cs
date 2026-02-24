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

        public LinkClientPipeline()
        {
            _head = new HeadLinkClientHandlerContext();
            _tail = new TailLinkClientHandlerContext();
            _head.Next = _tail;
            _tail.Prev = _head;
        }

        private static LinkClientHandlerContext<T> CreateContext<T>(string name, T handler) where T : ILinkClientHandler
        {
            return new LinkClientHandlerContext<T>(name, handler);
        }

        #region Pipeline Operations

        ///<inheritdoc/>
        public ILinkClientPipeline AddAfter<T>(string baseName, string name, T handler) where T : ILinkClientHandler
        {
            var ctx = _head;
            while (ctx != null && ctx.Name != baseName)
                ctx = ctx.Next!;
            if (ctx == null)
                throw new KeyNotFoundException($"Base handler not found: {baseName}");

            var newCtx = CreateContext(name, handler);
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
                throw new KeyNotFoundException($"Base handler not found: {baseName}");

            var prev = ctx.Prev!;
            var newCtx = CreateContext(name, handler);
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
            var newCtx = CreateContext(name, handler);
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
            var newCtx = CreateContext(name, handler);
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
                    var newCtx = CreateContext(newName, newHandler);
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
                    var newCtx = CreateContext(newName, newHandler);
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
        public ValueTask ExceptionCaught(Exception exception)
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
    }
}