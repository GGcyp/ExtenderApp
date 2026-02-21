namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 定义处理器方法可跳过的标记集合。
    /// </summary>
    [Flags]
    internal enum SkipFlags
    {
        /// <summary>
        /// 跳过 <see cref="ILinkClientHandler.Added"/> 调用。
        /// </summary>
        Added = 1,

        /// <summary>
        /// 跳过 <see cref="ILinkClientHandler.Removed"/> 调用。
        /// </summary>
        Removed = 1 << 1,

        /// <summary>
        /// 跳过 <see cref="ILinkClientHandler.ExceptionCaught"/> 调用。
        /// </summary>
        ExceptionCaught = 1 << 2,

        /// <summary>
        /// 跳过 <see cref="ILinkClientHandler.Active"/> 调用。
        /// </summary>
        Active = 1 << 3,

        /// <summary>
        /// 跳过 <see cref="ILinkClientHandler.Inactive"/> 调用。
        /// </summary>
        Inactive = 1 << 4,

        /// <summary>
        /// 跳过绑定相关调用。
        /// </summary>
        Bind = 1 << 5,

        /// <summary>
        /// 跳过连接相关调用。
        /// </summary>
        Connect = 1 << 6,

        /// <summary>
        /// 跳过断开连接相关调用。
        /// </summary>
        Disconnect = 1 << 7,

        /// <summary>
        /// 跳过关闭相关调用。
        /// </summary>
        Close = 1 << 8,

        /// <summary>
        /// 跳过入站处理调用。
        /// </summary>
        InboundHandle = 1 << 9,

        /// <summary>
        /// 跳过出站处理调用。
        /// </summary>
        OutboundHandle = 1 << 10,

        /// <summary>
        /// 入站处理器的组合跳过标记。
        /// </summary>
        Inbound = ExceptionCaught |
            Active |
            Inactive |
            InboundHandle,

        /// <summary>
        /// 出站处理器的组合跳过标记。
        /// </summary>
        Outbound = Bind |
            Connect |
            Disconnect |
            Close |
            OutboundHandle,
    }
}