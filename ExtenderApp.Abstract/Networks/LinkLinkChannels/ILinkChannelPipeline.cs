namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 链路通道处理器管线接口，负责管理一系列 <see cref="ILinkChannelHandler"/> 实例的注册、移除与替换。
    /// 管线按顺序维护处理器，入站数据按注册顺序从管线头部流入，出站数据按相反顺序流向传输层。
    /// 实现应保证对处理器的增删改操作为线程安全或在单线程上下文内使用。
    /// </summary>
    public interface ILinkChannelPipeline : IEnumerable<ILinkChannelHandler>
    {
        /// <summary>
        /// 在管道末尾添加一个处理器节点（尾部插入）。
        /// 新添加的处理器将成为管道中最后一个被执行的出站处理器和第一个被执行的入站处理器（取决于处理方向）。
        /// </summary>
        /// <typeparam name="T">处理器类型，必须实现 <see cref="ILinkChannelHandler"/>。</typeparam>
        /// <param name="name">注册名称，用于后续通过名称定位或替换处理器，应在同一管道中保持唯一性。</param>
        /// <param name="handler">处理器实例。</param>
        /// <returns>返回当前管道实例，便于链式调用。</returns>
        ILinkChannelPipeline AddLast<T>(string name, T handler) where T : ILinkChannelHandler;

        /// <summary>
        /// 在管道开头添加一个处理器节点（头部插入）。
        /// 新添加的处理器会最先参与入站处理并最后参与出站处理。
        /// </summary>
        /// <typeparam name="T">处理器类型，必须实现 <see cref="ILinkChannelHandler"/>。</typeparam>
        /// <param name="name">注册名称，用于标识该处理器。</param>
        /// <param name="handler">处理器实例。</param>
        /// <returns>返回当前管道实例，便于链式调用。</returns>
        ILinkChannelPipeline AddFirst<T>(string name, T handler) where T : ILinkChannelHandler;

        /// <summary>
        /// 在指定的基准处理器之前插入一个新的处理器节点。
        /// 如果找不到指定的基准名称，应抛出异常或由实现提供替代行为（例如添加到末尾）。
        /// </summary>
        /// <typeparam name="T">处理器类型，必须实现 <see cref="ILinkChannelHandler"/>。</typeparam>
        /// <param name="baseName">基准处理器的注册名称，新的处理器会插入到该处理器之前。</param>
        /// <param name="name">新处理器的注册名称。</param>
        /// <param name="handler">新处理器实例。</param>
        /// <returns>返回当前管道实例。</returns>
        ILinkChannelPipeline AddBefore<T>(string baseName, string name, T handler) where T : ILinkChannelHandler;

        /// <summary>
        /// 在指定的基准处理器之后插入一个新的处理器节点。
        /// </summary>
        /// <typeparam name="T">处理器类型，必须实现 <see cref="ILinkChannelHandler"/>。</typeparam>
        /// <param name="baseName">基准处理器的注册名称，新的处理器会插入到该处理器之后。</param>
        /// <param name="name">新处理器的注册名称。</param>
        /// <param name="handler">新处理器实例。</param>
        /// <returns>返回当前管道实例。</returns>
        ILinkChannelPipeline AddAfter<T>(string baseName, string name, T handler) where T : ILinkChannelHandler;

        /// <summary>
        /// 从管道中移除指定的处理器实例（按引用匹配）。
        /// 移除操作应同时调用被移除处理器的 <see cref="ILinkChannelHandler.Removed(ILinkChannelHandlerContext)"/> 回调并释放其资源。
        /// </summary>
        /// <typeparam name="T">要移除的处理器类型。</typeparam>
        /// <param name="handler">要移除的处理器实例。</param>
        /// <returns>返回当前管道实例以便链式调用。</returns>
        ILinkChannelPipeline Remove<T>(T handler) where T : ILinkChannelHandler;

        /// <summary>
        /// 按名称从管道中移除处理器，并返回被移除的实例（若不存在则返回 null 或抛出，取决于实现）。
        /// </summary>
        /// <param name="name">要移除的处理器注册名称。</param>
        /// <returns>被移除的处理器实例。</returns>
        ILinkChannelHandler Remove(string name);

        /// <summary>
        /// 用新的处理器替换管道中指定名称的现有处理器。
        /// 替换应保留原处理器在管道中的位置，并在替换时调用原处理器的 <see cref="ILinkChannelHandler.Removed(ILinkChannelHandlerContext)"/>。
        /// </summary>
        /// <typeparam name="T">新处理器类型。</typeparam>
        /// <param name="oldName">要替换的旧处理器名称。</param>
        /// <param name="newName">新处理器的注册名称。</param>
        /// <param name="newHandler">新处理器实例。</param>
        /// <returns>返回当前管道实例。</returns>
        ILinkChannelPipeline Replace<T>(string oldName, string newName, T newHandler) where T : ILinkChannelHandler;

        /// <summary>
        /// 用新的处理器替换管道中指定的旧处理器实例（按引用匹配）。
        /// </summary>
        /// <typeparam name="T">新处理器类型。</typeparam>
        /// <param name="oldHandler">要替换的旧处理器实例。</param>
        /// <param name="newName">新处理器的注册名称。</param>
        /// <param name="newHandler">新处理器实例。</param>
        /// <returns>返回当前管道实例。</returns>
        ILinkChannelPipeline Replace<T>(ILinkChannelHandler oldHandler, string newName, T newHandler) where T : ILinkChannelHandler;
    }
}