namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 管道头部的处理器上下文，用于将 <see cref="HeadLinkClientHandler"/> 绑定到特定的上下文名称。 该上下文在管道构建时作为第一项，承载默认的头部处理器实例。
    /// </summary>
    internal class HeadLinkClientHandlerContext : LinkClientHandlerContext
    {
        /// <summary>
        /// 使用指定名称创建 <see cref="HeadLinkClientHandlerContext"/> 实例，并将默认的头部处理器关联到该上下文。
        /// </summary>
        /// <param name="name">上下文名称，用于标识该处理器在管道中的位置或用途。</param>
        public HeadLinkClientHandlerContext() : base("Head", HeadLinkClientHandler.Default, typeof(HeadLinkClientHandler))
        {
        }
    }
}