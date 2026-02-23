namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 管道尾部的处理器上下文，用于将 <see cref="TailLinkClientHandler"/> 绑定到尾部上下文。 该上下文在管道构建时作为最后一项，承载默认的尾部处理器实例。
    /// </summary>
    internal class TailLinkClientHandlerContext : LinkClientHandlerContext
    {
        /// <summary>
        /// 创建默认的尾部上下文实例，名称为 "Tail"，并关联默认的尾部处理器。
        /// </summary>
        public TailLinkClientHandlerContext() : base("Tail", TailLinkClientHandler.Default, typeof(TailLinkClientHandler))
        {
        }
    }
}