namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端链路处理管道，负责管理一系列的处理器（Handler）。
    /// </summary>
    public interface ILinkClientPipeline :  IEnumerable<ILinkClientHandler>
    {
        /// <summary>
        /// 在管道末尾添加处理器。
        /// </summary>
        /// <param name="name">处理器名称。</param>
        /// <param name="handler">处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline AddLast(string name, ILinkClientHandler handler);

        /// <summary>
        /// 在管道开头添加处理器。
        /// </summary>
        /// <param name="name">处理器名称。</param>
        /// <param name="handler">处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline AddFirst(string name, ILinkClientHandler handler);

        /// <summary>
        /// 在指定处理器之前添加处理器。
        /// </summary>
        /// <param name="baseName">基准处理器名称。</param>
        /// <param name="name">处理器名称。</param>
        /// <param name="handler">处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline AddBefore(string baseName, string name, ILinkClientHandler handler);

        /// <summary>
        /// 在指定处理器之后添加处理器。
        /// </summary>
        /// <param name="baseName">基准处理器名称。</param>
        /// <param name="name">处理器名称。</param>
        /// <param name="handler">处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline AddAfter(string baseName, string name, ILinkClientHandler handler);

        /// <summary>
        /// 从管道中移除指定处理器。
        /// </summary>
        /// <param name="handler">要移除的处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline Remove(ILinkClientHandler handler);

        /// <summary>
        /// 从管道中移除指定名称的处理器。
        /// </summary>
        /// <param name="name">处理器名称。</param>
        /// <returns>被移除的处理器实例。</returns>
        ILinkClientHandler Remove(string name);

        /// <summary>
        /// 替换指定名称的处理器。
        /// </summary>
        /// <param name="oldName">旧处理器名称。</param>
        /// <param name="newName">新处理器名称。</param>
        /// <param name="newHandler">新处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline Replace(string oldName, string newName, ILinkClientHandler newHandler);

        ILinkClientPipeline Replace(ILinkClientHandler oldHandler, string newName, ILinkClientHandler newHandler);

        /// <summary>
        /// 获取指定名称处理器的上下文。
        /// </summary>
        /// <param name="name">处理器名称。</param>
        /// <returns>处理器上下文。</returns>
        ILinkClientHandlerContext GetContext(string name);

        /// <summary>
        /// 获取指定处理器的上下文。
        /// </summary>
        /// <param name="handler">处理器实例。</param>
        /// <returns>处理器上下文。</returns>
        ILinkClientHandlerContext GetContext(ILinkClientHandler handler);

        /// <summary>
        /// 触发通道激活事件。
        /// </summary>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline Active();

        /// <summary>
        /// 触发通道失活事件。
        /// </summary>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline Inactive();

        /// <summary>
        /// 触发通道读事件。
        /// </summary>
        /// <param name="message">读取到的消息。</param>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline FireChannelRead(object message);

        /// <summary>
        /// 触发通道读完成事件。
        /// </summary>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline FireChannelReadComplete();

        /// <summary>
        /// 触发异常捕获事件。
        /// </summary>
        /// <param name="ex">异常信息。</param>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline FireExceptionCaught(Exception ex);

        /// <summary>
        /// 异步写入消息。
        /// </summary>
        /// <param name="message">要写入的消息。</param>
        /// <returns>异步任务。</returns>
        Task WriteAsync(object message);

        /// <summary>
        /// 刷新管道中的待发送消息。
        /// </summary>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline Flush();

        /// <summary>
        /// 异步写入并刷新消息。
        /// </summary>
        /// <param name="message">要写入的消息。</param>
        /// <returns>异步任务。</returns>
        Task WriteAndFlushAsync(object message);
    }
}