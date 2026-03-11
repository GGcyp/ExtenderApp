using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Networks.LinkChannels;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 表示链接管道中处理器上下文的抽象基类，封装了处理器实例、跳过标志以及与管道和链接客户端的关联关系。
    /// </summary>
    internal abstract class LinkChannelHandlerContext : DisposableObject, ILinkChannelHandlerContext
    {
        #region SkipFlags

        private static readonly SkipFlags AllFlags = Enum.GetValues<SkipFlags>().Aggregate((a, b) => a | b);

        private static readonly ConcurrentDictionary<Type, SkipFlags> _skipDict = new();

        protected static SkipFlags GetSkipFlags<T>() where T : ILinkChannelHandler
            => GetSkipFlags(typeof(T));

        protected static SkipFlags GetSkipFlags(Type handlerType)
            => _skipDict.GetOrAdd(handlerType, CreateSkipFlags);

        protected static SkipFlags GetSkipFlags(ILinkChannelHandler handler)
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
        public ILinkChannelHandler Handler { get; }

        /// <summary>
        /// 获取当前处理器的跳过标志（由处理器方法上的 <see cref="SkipAttribute"/> 决定）， 用于在查找下一个可用上下文时跳过不参与特定方向（入站/出站）的处理器。
        /// </summary>
        public SkipFlags HandlerSkipFlags { get; }

        /// <summary>
        /// 获取当前上下文所属的链接客户端实例。该属性在上下文被添加到管道时由管道设置，并在整个上下文生命周期内保持不变。
        /// </summary>
        public ILinkChannel LinkClient { get; }

        /// <summary>
        /// 管道中下一个上下文节点（入站方向）。该字段是易变的以支持并发更新。
        /// </summary>
        internal volatile LinkChannelHandlerContext? Next;

        /// <summary>
        /// 管道中上一个上下文节点（出站方向）。该字段是易变的以支持并发更新。
        /// </summary>
        internal volatile LinkChannelHandlerContext? Prev;

        /// <summary>
        /// 使用指定名称和处理器创建上下文，并使用处理器实际类型来计算跳过标志。
        /// </summary>
        /// <param name="name">上下文名称。</param>
        /// <param name="linkClient">所属的链接客户端实例。</param>
        /// <param name="handler">要关联的处理器实例。</param>
        public LinkChannelHandlerContext(string name, ILinkChannel linkClient, ILinkChannelHandler handler) : this(name, linkClient, handler, handler.GetType())
        {
        }

        /// <summary>
        /// 使用指定名称、处理器及处理器类型创建上下文。该构造函数允许调用方显式传入处理器类型以优化标志计算。
        /// </summary>
        /// <param name="name">上下文名称。</param>
        /// <param name="linkClient">所属的链接客户端实例。</param>
        /// <param name="handler">要关联的处理器实例。</param>
        /// <param name="handlerType">处理器的 <see cref="Type"/>，用于获取方法上的 <see cref="SkipAttribute"/> 信息。</param>
        public LinkChannelHandlerContext(string name, ILinkChannel linkClient, ILinkChannelHandler handler, Type handlerType)
        {
            Name = name;
            LinkClient = linkClient;
            Handler = handler;
            HandlerSkipFlags = GetSkipFlags(handlerType);
            Next = default!;
            Prev = default!;
        }

        #region Invoke

        /// <inheritdoc/>
        public ValueTask<Result> ActiveAsync(CancellationToken token = default)
            => FindContextInbound()?.InvokeActiveAsync(token) ?? Result.Success();

        /// <summary>
        /// 调用当前处理器的激活实现。该方法封装对具体处理器 <see cref="ILinkChannelHandler.ActiveAsync"/> 的调用。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>处理结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        private ValueTask<Result> InvokeActiveAsync(CancellationToken token)
            => Handler.ActiveAsync(this, token);

        /// <inheritdoc/>
        public ValueTask<Result> InactiveAsync(CancellationToken token = default)
            => FindContextInbound()?.InvokeInactiveAsync(token) ?? Result.Success();

        /// <summary>
        /// 调用当前处理器的停用实现。封装对 <see cref="ILinkChannelHandler.InactiveAsync"/> 的调用。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>处理结果的异步 <see cref="ValueTask{Result}"/>。</returns>
        private ValueTask<Result> InvokeInactiveAsync(CancellationToken token)
            => Handler.InactiveAsync(this, token);

        /// <inheritdoc/>
        public ValueTask<Result> ConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => FindContextOutbound()?.InvokeConnectAsync(remoteAddress, localAddress, token) ?? Result.Success();

        /// <summary>
        /// 调用当前处理器的连接实现。封装对 <see cref="ILinkChannelHandler.ConnectAsync"/> 的调用。
        /// </summary>
        private ValueTask<Result> InvokeConnectAsync(EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
            => Handler.ConnectAsync(this, remoteAddress, localAddress, token);

        /// <inheritdoc/>
        public ValueTask<Result> DisconnectAsync(CancellationToken token = default)
            => FindContextOutbound()?.InvokeDisconnectAsync(token) ?? Result.Success();

        /// <summary>
        /// 调用当前处理器的断开实现。封装对 <see cref="ILinkChannelHandler.DisconnectAsync"/> 的调用。
        /// </summary>
        private ValueTask<Result> InvokeDisconnectAsync(CancellationToken token)
            => Handler.DisconnectAsync(this, token);

        /// <inheritdoc/>
        public ValueTask<Result> BindAsync(EndPoint localAddress, CancellationToken token = default)
            => FindContextOutbound()?.InvokeBindAsync(localAddress, token) ?? Result.Success();

        /// <summary>
        /// 调用当前处理器的绑定实现。封装对 <see cref="ILinkChannelHandler.BindAsync"/> 的调用。
        /// </summary>
        private ValueTask<Result> InvokeBindAsync(EndPoint localAddress, CancellationToken token)
            => Handler.BindAsync(this, localAddress, token);

        /// <inheritdoc/>
        public ValueTask<Result> CloseAsync(CancellationToken token = default)
        => FindContextOutbound()?.InvokeCloseAsync(token) ?? Result.Success();

        /// <summary>
        /// 调用当前处理器的关闭实现。封装对 <see cref="ILinkChannelHandler.CloseAsync"/> 的调用。
        /// </summary>
        private ValueTask<Result> InvokeCloseAsync(CancellationToken token)
            => Handler.CloseAsync(this, token);

        /// <inheritdoc/>
        public Result ExceptionCaught(Exception exception)
            => FindContextOutbound()?.InvokeExceptionCaught(exception) ?? Result.FromException(exception);

        /// <summary>
        /// 调用当前处理器的异常处理实现。封装对 <see cref="ILinkChannelHandler.ExceptionCaught"/> 的调用。
        /// </summary>
        private Result InvokeExceptionCaught(Exception exception)
            => Handler.ExceptionCaught(this, exception);

        /// <summary>
        /// 将入站数据处理请求沿入站方向传递到下一个合适的处理器上下文。
        /// 若未找到合适的处理器则返回成功结果。
        /// </summary>
        /// <param name="cache">要处理的数据缓存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>异步返回处理结果。</returns>
        public ValueTask<Result> InboundHandleAsync(ValueCache cache, CancellationToken token = default)
            => FindContextInbound()?.InvokeInboundHandleAsync(cache, token) ?? Result.Success();

        /// <summary>
        /// 调用当前处理器的入站处理实现。封装对 <see cref="ILinkChannelHandler.InboundHandleAsync"/> 的调用。
        /// </summary>
        private ValueTask<Result> InvokeInboundHandleAsync(ValueCache cache, CancellationToken token)
            => Handler.InboundHandleAsync(this, cache, token);

        /// <summary>
        /// 将出站数据处理请求沿出站方向传递到上一个合适的处理器上下文。
        /// 若未找到合适的处理器则返回成功结果。
        /// </summary>
        /// <param name="cache">要处理的数据缓存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>异步返回处理结果。</returns>
        public ValueTask<Result> OutboundHandleAsync(ValueCache cache, CancellationToken token = default)
            => FindContextOutbound()?.InvokeOutboundHandleAsync(cache, token) ?? Result.Success();

        /// <summary>
        /// 调用当前处理器的出站处理实现。封装对 <see cref="ILinkChannelHandler.OutboundHandleAsync"/> 的调用。
        /// </summary>
        private ValueTask<Result> InvokeOutboundHandleAsync(ValueCache cache, CancellationToken token)
            => Handler.OutboundHandleAsync(this, cache, token);

        #endregion Invoke

        #region FindContext

        /// <summary>
        /// 查找下一个参与入站处理的上下文（跳过标记为跳过入站的处理器）。
        /// </summary>
        /// <returns>若找到则返回对应的上下文，否则返回 null。</returns>
        private LinkChannelHandlerContext? FindContextInbound()
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
        private LinkChannelHandlerContext? FindContextOutbound()
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
    /// 泛型化的 <see cref="LinkChannelHandlerContext"/>，用于在创建时指定处理器的静态类型以优化跳过标志的计算。
    /// </summary>
    /// <typeparam name="T">处理器类型，必须实现 <see cref="ILinkChannelHandler"/>。</typeparam>
    internal class LinkChannelHandlerContext<T> : LinkChannelHandlerContext where T : ILinkChannelHandler
    {
        /// <summary>
        /// 使用指定名称和类型化的处理器实例创建上下文。
        /// </summary>
        /// <param name="name">上下文名称，用于标识该处理器在管道中的位置或用途。</param>
        /// <param name="handler">类型化的处理器实例。</param>
        public LinkChannelHandlerContext(string name, ILinkChannel linkClient, T handler) : base(name, linkClient, handler, typeof(T))
        {
        }
    }
}