namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端链路处理管道，负责管理一系列的处理器（Handler）。
    /// </summary>
    public interface ILinkClientPipeline : IEnumerable<ILinkClientHandler>
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

        /// <summary>
        /// 替换指定处理器实例。
        /// </summary>
        /// <param name="oldHandler">旧处理器实例。</param>
        /// <param name="newName">新处理器名称。</param>
        /// <param name="newHandler">新处理器实例。</param>
        /// <returns>当前管道实例。</returns>
        ILinkClientPipeline Replace(ILinkClientHandler oldHandler, string newName, ILinkClientHandler newHandler);
    }
}