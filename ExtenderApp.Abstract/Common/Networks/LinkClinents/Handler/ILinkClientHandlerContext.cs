namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示链路管道处理器上下文，负责处理器之间的交互以及向后传递事件。
    /// </summary>
    public interface ILinkClientHandlerContext : ILinkClientHandlerContextOperations
    {
        /// <summary>
        /// 获取处理器的名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取对应的处理器实例。
        /// </summary>
        ILinkClientHandler Handler { get; }
    }
}