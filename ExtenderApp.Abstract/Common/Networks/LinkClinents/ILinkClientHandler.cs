namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示链路管道中的处理器，用于拦截和处理 I/O 事件。
    /// </summary>
    public interface ILinkClientHandler : IDisposable
    {
        /// <summary>
        /// 当处理器被添加到管道时调用。
        /// </summary>
        void HandlerAdded(ILinkClientHandlerContext context) { }

        /// <summary>
        /// 当处理器从管道移除时调用。
        /// </summary>
        void HandlerRemoved(ILinkClientHandlerContext context) { }

        /// <summary>
        /// 当通道变为活跃状态时调用（例如连接建立）。
        /// </summary>
        void ChannelActive(ILinkClientHandlerContext context) { context.FireChannelActive(); }

        /// <summary>
        /// 当通道变为非活跃状态时调用（例如连接断开）。
        /// </summary>
        void ChannelInactive(ILinkClientHandlerContext context) { context.FireChannelInactive(); }

        /// <summary>
        /// 当从通道读取到数据时调用。
        /// </summary>
        /// <param name="message">读取的消息数据。</param>
        void ChannelRead(ILinkClientHandlerContext context, object message) { context.FireChannelRead(message); }

        /// <summary>
        /// 当通道读取操作完成时调用。
        /// </summary>
        void ChannelReadComplete(ILinkClientHandlerContext context) { context.FireChannelReadComplete(); }

        /// <summary>
        /// 当处理过程中发生异常时调用。
        /// </summary>
        void ExceptionCaught(ILinkClientHandlerContext context, Exception exception) { context.FireExceptionCaught(exception); }

        /// <summary>
        /// 请求向通道写入数据。
        /// </summary>
        /// <param name="message">要写入的数据。</param>
        Task WriteAsync(ILinkClientHandlerContext context, object message) { return context.WriteAsync(message); }

        /// <summary>
        /// 请求刷新通道数据。
        /// </summary>
        void Flush(ILinkClientHandlerContext context) { context.Flush(); }
    }
}