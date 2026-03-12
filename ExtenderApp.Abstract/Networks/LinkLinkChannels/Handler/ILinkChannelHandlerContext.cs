namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 管道中单个处理器的上下文接口，封装了对管道、邻居处理器及所属通道的访问。
    /// Context 提供将事件沿管道传递（激活/停用/收发/连接/断开等）的方法，
    /// 并允许处理器通过该上下文与管道交互。
    /// </summary>
    public interface ILinkChannelHandlerContext : ILinkChannelHandlerContextOperations
    {
        /// <summary>
        /// 获取当前处理器在管道中的注册名称（用于查找、替换或移除）。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取与此上下文关联的处理器实例。
        /// 处理器的生命周期由管道管理，处理器实现不应直接释放自身资源，需通过管道移除时释放。
        /// </summary>
        ILinkChannelHandler Handler { get; }

        /// <summary>
        /// 获取此上下文所属的链路通道实例，处理器可通过该通道访问底层链接与选项。
        /// </summary>
        ILinkChannel LinkChannel { get; }
    }
}