namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 管道中尾部的链接客户端处理器，作为链式处理的终结节点。 该处理器实现为单例字段 <see cref="Default"/>，用于在管道中复用同一实例。
    /// </summary>
    internal class TailLinkClientHandler : LinkClientHandler
    {
        /// <summary>
        /// 获取默认的 <see cref="TailLinkClientHandler"/> 实例，管道尾部使用此实例作为默认处理器。
        /// </summary>
        internal static readonly TailLinkClientHandler Default = new();
    }
}