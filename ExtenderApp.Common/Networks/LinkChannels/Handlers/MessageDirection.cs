namespace ExtenderApp.Common.Networks.LinkChannels.Handlers
{
    /// <summary>
    /// 指示消息处理器应处理的消息方向。 可通过按位组合来同时表示多个方向（例如 <see cref="Both"/>）。
    /// </summary>
    [Flags]
    public enum MessageDirection
    {
        /// <summary>
        /// 仅处理入站消息（从远端接收的消息）。
        /// </summary>
        Inbound = 1 << 1,

        /// <summary>
        /// 仅处理出站消息（要发送到远端的消息）。
        /// </summary>
        Outbound = 1 << 2,

        /// <summary>
        /// 同时处理入站与出站消息。
        /// </summary>
        Both = Inbound | Outbound
    }
}