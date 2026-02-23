using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Networks.LinkClients;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 表示链接客户端处理器在管道中的上下文节点。
    /// 每个上下文包裹一个 <see cref="ILinkClientHandler"/> 实例，并维护管道中前后相邻的上下文引用，
    /// 提供对处理器方法的查找与转发（包括入站/出站调用、异常处理、绑定/连接等）。
    /// </summary>
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

        /// <summary>
        /// 获取此上下文的名称，用于标识该处理器在管道中的位置或用途。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 获取与此上下文关联的处理器实例。
        /// </summary>
        public ILinkClientHandler Handler { get; }

        /// <summary>
        /// 获取当前处理器的跳过标志（由处理器方法上的 <see cref="SkipAttribute"/> 决定），
        /// 用于在查找下一个可用上下文时跳过不参与特定方向（入站/出站）的处理器。
        /// </summary>
        public SkipFlags HandlerSkipFlags { get; }

        /// <summary>
        /// 管道中下一个上下文节点（入站方向）。该字段是易变的以支持并发更新。
        /// </summary>
        internal volatile LinkClientHandlerContext? Next;

        /// <summary>
        /// 管道中上一个上下文节点（出站方向）。该字段是易变的以支持并发更新。
        /// </summary>
        internal volatile LinkClientHandlerContext? Prev;

        /// <summary>
        /// 使用指定名称和处理器创建上下文，并使用处理器实际类型来计算跳过标志。
        /// </summary>
        /// <param name="name">上下文名称。</param>
        /// <param name="handler">要关联的处理器实例。</param>
        public LinkClientHandlerContext(string name, ILinkClientHandler handler) : this(name, handler, handler.GetType())
        {
        }

        /// <summary>
        /// 使用指定名称、处理器及处理器类型创建上下文。该构造函数允许调用方显式传入处理器类型以优化标志计算。
        /// </summary>
        /// <param name="name">上下文名称。</param>
        /// <param name="handler">要关联的处理器实例。</param>
        /// <param name="handlerType">处理器的 <see cref="Type"/>，用于获取方法上的 <see cref="SkipAttribute"/> 信息。</param>
        public LinkClientHandlerContext(string name, ILinkClientHandler handler, Type handlerType)
        {
            Name = name;
            Handler = handler;
            HandlerSkipFlags = GetSkipFlags(handlerType);
            Next = default!;
            Prev = default!;
        }

        #region Invoke

        /// <summary>
        /// 将连接请求沿入站方向传递到下一个合适的处理器上下文，若无下游处理器则直接返回成功结果。
        /// </summary>
        /// <param name="remoteAddress">远程终结点。</param>
        /// <param name="localAddress">本地终结点。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>异步返回操作结果。</returns>
        public ValueTask<Result> ConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => FindContextInbound()?.InvokeConnectAsync(remoteAddress, localAddress, token) ?? new ValueTask<Result>(Result.Success());

        /// <summary>
        /// 调用当前处理器的连接实现。
        /// </summary>
        private ValueTask<Result> InvokeConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => Handler.ConnectAsync(this, remoteAddress, localAddress, token);

        /// <summary>
        /// 将断开请求沿入站方向传递到下一个合适的处理器上下文，若无下游处理器则直接返回成功结果。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>异步返回操作结果。</returns>
        public ValueTask<Result> DisconnectAsync(CancellationToken token = default)
            => FindContextInbound()?.InvokeDisconnectAsync(token) ?? new ValueTask<Result>(Result.Success());

        /// <summary>
        /// 调用当前处理器的断开实现。
        /// </summary>
        private ValueTask<Result> InvokeDisconnectAsync(CancellationToken token)
            => Handler.DisconnectAsync(this, token);

        /// <summary>
        /// 将绑定请求沿出站方向传递到上一个合适的处理器上下文，若无上游处理器则直接返回成功结果。
        /// </summary>
        /// <param name="localAddress">本地终结点。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>异步返回操作结果。</returns>
        public ValueTask<Result> BindAsync(EndPoint localAddress, CancellationToken token = default)
            => FindContextOutbound()?.InvokeBindAsync(localAddress, token) ?? new ValueTask<Result>(Result.Success());

        /// <summary>
        /// 调用当前处理器的绑定实现。
        /// </summary>
        private ValueTask<Result> InvokeBindAsync(EndPoint localAddress, CancellationToken token)
            => Handler.BindAsync(this, localAddress, token);

        /// <summary>
        /// 将关闭请求沿出站方向传递到上一个合适的处理器上下文，若无上游处理器则直接返回成功结果。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>异步返回操作结果。</returns>
        public ValueTask<Result> CloseAsync(CancellationToken token = default)
        => FindContextOutbound()?.InvokeCloseAsync(token) ?? new ValueTask<Result>(Result.Success());

        /// <summary>
        /// 调用当前处理器的关闭实现。
        /// </summary>
        private ValueTask<Result> InvokeCloseAsync(CancellationToken token)
            => Handler.CloseAsync(this, token);

        /// <summary>
        /// 将异常沿入站方向传递到下一个合适的处理器上下文进行处理。
        /// 若无下游处理器则返回一个表示异常的失败 <see cref="ValueTask"/>。
        /// </summary>
        /// <param name="exception">要传递的异常。</param>
        /// <returns>用于等待异常处理完成的任务。</returns>
        public ValueTask ExceptionCaught(Exception exception)
            => FindContextInbound()?.InvokeExceptionCaught(exception) ?? ValueTask.FromException(exception);

        /// <summary>
        /// 调用当前处理器的异常处理实现。
        /// </summary>
        private ValueTask InvokeExceptionCaught(Exception exception)
            => Handler.ExceptionCaught(this, exception);

        /// <summary>
        /// 将入站数据处理请求沿入站方向传递到下一个合适的处理器上下文。
        /// </summary>
        /// <param name="cache">要处理的数据缓存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>异步返回处理结果，通常包含已消费的字节数。</returns>
        public ValueTask<Result<int>> InboundHandleAsync(ValueCache cache, CancellationToken token = default)
            => FindContextInbound()?.InvokeInboundHandleAsync(cache, token) ?? new ValueTask<Result<int>>(Result.Success(0));

        /// <summary>
        /// 调用当前处理器的入站处理实现。
        /// </summary>
        private ValueTask<Result<int>> InvokeInboundHandleAsync(ValueCache cache, CancellationToken token)
            => Handler.InboundHandleAsync(this, cache, token);

        /// <summary>
        /// 将出站数据处理请求沿出站方向传递到上一个合适的处理器上下文。
        /// </summary>
        /// <param name="cache">要处理的数据缓存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>异步返回处理结果。</returns>
        public ValueTask<Result> OutboundHandleAsync(ValueCache cache, CancellationToken token = default)
            => FindContextOutbound()?.InvokeOutboundHandleAsync(cache, token) ?? new ValueTask<Result>(Result.Success());

        /// <summary>
        /// 调用当前处理器的出站处理实现。
        /// </summary>
        private ValueTask<Result> InvokeOutboundHandleAsync(ValueCache cache, CancellationToken token)
            => Handler.OutboundHandleAsync(this, cache, token);

        #endregion Invoke

        #region FindContext

        /// <summary>
        /// 查找下一个参与入站处理的上下文（跳过标记为跳过入站的处理器）。
        /// </summary>
        /// <returns>若找到则返回对应的上下文，否则返回 null。</returns>
        private LinkClientHandlerContext? FindContextInbound()
        {
            var ctx = this;
            do
            {
                ctx = ctx.Next;
            } while (ctx != null && (ctx.HandlerSkipFlags & SkipFlags.Inbound) == SkipFlags.Inbound);
            return ctx;
        }

        /// <summary>
        /// 查找上一个参与出站处理的上下文（跳过标记为跳过出站的处理器）。
        /// </summary>
        /// <returns>若找到则返回对应的上下文，否则返回 null。</returns>
        private LinkClientHandlerContext? FindContextOutbound()
        {
            var ctx = this;
            do
            {
                ctx = ctx.Prev;
            } while (ctx != null && (ctx.HandlerSkipFlags & SkipFlags.Outbound) == SkipFlags.Outbound);
            return ctx;
        }

        #endregion FindContext

        protected override void DisposeManagedResources()
        {
            Handler?.DisposeSafe();
        }
    }

    /// <summary>
    /// 泛型化的 <see cref="LinkClientHandlerContext"/>，用于在创建时指定处理器的静态类型以优化跳过标志的计算。
    /// </summary>
    /// <typeparam name="T">处理器类型，必须实现 <see cref="ILinkClientHandler"/>。</typeparam>
    internal class LinkClientHandlerContext<T> : LinkClientHandlerContext where T : ILinkClientHandler
    {
        /// <summary>
        /// 使用指定名称和类型化的处理器实例创建上下文。
        /// </summary>
        /// <param name="name">上下文名称，用于标识该处理器在管道中的位置或用途。</param>
        /// <param name="handler">类型化的处理器实例。</param>
        public LinkClientHandlerContext(string name, T handler) : base(name, handler, typeof(T))
        {
        }
    }
}