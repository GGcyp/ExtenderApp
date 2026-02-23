using System.Collections;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal sealed class LinkClientPipeline : ILinkClientPipeline, ILinkClientHandlerContextOperations
    {
        private LinkClientHandlerContext head;
        private LinkClientHandlerContext tail;

        public LinkClientPipeline()
        {
            head = new HeadLinkClientHandlerContext();
            tail = new TailLinkClientHandlerContext();
            head.Next = tail;
            tail.Prev = head;
        }

        private static LinkClientHandlerContext<T> CreateContext<T>(string name, T handler) where T : ILinkClientHandler
        {
            return new LinkClientHandlerContext<T>(name, handler);
        }

        public ILinkClientPipeline AddAfter<T>(string baseName, string name, T handler) where T : ILinkClientHandler
        {
            var ctx = head;
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

        public ILinkClientPipeline AddBefore<T>(string baseName, string name, T handler) where T : ILinkClientHandler
        {
            var ctx = head.Next;
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

        public ILinkClientPipeline AddFirst<T>(string name, T handler) where T : ILinkClientHandler
        {
            var next = head.Next!;
            var newCtx = CreateContext(name, handler);
            newCtx.Next = next;
            newCtx.Prev = head;
            head.Next = newCtx;
            next.Prev = newCtx;
            return this;
        }

        public ILinkClientPipeline AddLast<T>(string name, T handler) where T : ILinkClientHandler
        {
            var prev = tail.Prev!;
            var newCtx = CreateContext(name, handler);
            newCtx.Next = tail;
            newCtx.Prev = prev;
            prev.Next = newCtx;
            tail.Prev = newCtx;
            return this;
        }

        public ILinkClientPipeline Remove<T>(T handler) where T : ILinkClientHandler
        {
            var ctx = head.Next;
            while (ctx != null && ctx != tail)
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

        public ILinkClientHandler Remove(string name)
        {
            var ctx = head.Next;
            while (ctx != null && ctx != tail)
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

        public ILinkClientPipeline Replace<T>(string oldName, string newName, T newHandler) where T : ILinkClientHandler
        {
            var ctx = head.Next;
            while (ctx != null && ctx != tail)
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

        public ILinkClientPipeline Replace<T>(ILinkClientHandler oldHandler, string newName, T newHandler) where T : ILinkClientHandler
        {
            var ctx = head.Next;
            while (ctx != null && ctx != tail)
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

        #region Operations

        public ValueTask<Result> BindAsync(EndPoint localAddress, CancellationToken token = default)
            => tail.BindAsync(localAddress, token);

        public ValueTask<Result> CloseAsync(CancellationToken token = default)
            => tail.CloseAsync(token);

        public ValueTask<Result> ConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => head.ConnectAsync(remoteAddress, localAddress, token);

        public ValueTask<Result> DisconnectAsync(CancellationToken token = default)
            => head.DisconnectAsync(token);

        public ValueTask ExceptionCaught(Exception exception)
            => head.ExceptionCaught(exception);

        public ValueTask<Result<int>> InboundHandleAsync(ValueCache cache, CancellationToken token = default)
        {
            return head.InboundHandleAsync(cache, token);
        }

        public ValueTask<Result> OutboundHandleAsync(ValueCache cache, CancellationToken token = default)
        {
            return tail.OutboundHandleAsync(cache, token);
        }

        #endregion Operations

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
        {
            return new Enumerator(head.Next, tail);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Enumerable
    }
}