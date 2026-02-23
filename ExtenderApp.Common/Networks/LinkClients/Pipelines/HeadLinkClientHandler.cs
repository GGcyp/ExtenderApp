namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 管道中头部的链接客户端处理器，作为链式处理的起始节点。
    /// 该处理器实现为单例字段 <see cref="Default"/>，用于在管道中复用同一实例。
    /// </summary>
    internal class HeadLinkClientHandler : LinkClientHandler
    {
        /// <summary>
        /// 获取默认的 <see cref="HeadLinkClientHandler"/> 实例，管道头部使用此实例作为默认处理器。
        /// </summary>
        internal static readonly HeadLinkClientHandler Default = new ();
    }
}
