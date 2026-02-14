using System.Buffers;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示链路管道处理器上下文，负责处理器之间的交互以及向后传递事件。
    /// </summary>
    public interface ILinkClientHandlerContext
    {
        /// <summary>
        /// 获取当前上下文关联的客户端链路。
        /// </summary>
        ILinkClient Client { get; }

        /// <summary>
        /// 获取所在的管道。
        /// </summary>
        ILinkClientPipeline Pipeline { get; }

        /// <summary>
        /// 获取处理器的名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取对应的处理器实例。
        /// </summary>
        ILinkClientHandler Handler { get; }

        /// <summary>
        /// 触发通道激活事件，传递给下一个处理器。
        /// </summary>
        ILinkClientHandlerContext FireChannelActive();

        /// <summary>
        /// 触发通道非激活事件，传递给下一个处理器。
        /// </summary>
        ILinkClientHandlerContext FireChannelInactive();

        /// <summary>
        /// 触发通道读取事件，将消息传递给下一个处理器。
        /// </summary>
        /// <param name="message">读取到的消息对象（通常是 IByteBuffer 或解码后的对象）。</param>
        ILinkClientHandlerContext FireChannelRead(object message);

        /// <summary>
        /// 触发读取完成事件，传递给下一个处理器。
        /// </summary>
        ILinkClientHandlerContext FireChannelReadComplete();

        /// <summary>
        /// 触发异常捕获事件，传递给下一个处理器。
        /// </summary>
        /// <param name="ex">捕获到的异常。</param>
        ILinkClientHandlerContext FireExceptionCaught(Exception ex);

        /// <summary>
        /// 请求向通道写入消息，传递给上一个处理器。
        /// </summary>
        /// <param name="message">要写入的消息。</param>
        /// <returns>写入任务。</returns>
        Task WriteAsync(object message);

        /// <summary>
        /// 请求刷新通道，将缓冲区数据发送出去。
        /// </summary>
        ILinkClientHandlerContext Flush();

        /// <summary>
        /// 写入并刷新消息。
        /// </summary>
        /// <param name="message">消息。</param>
        /// <returns>操作任务。</returns>
        Task WriteAndFlushAsync(object message);
    }
}