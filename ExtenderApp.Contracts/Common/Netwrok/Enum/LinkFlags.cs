using System.Net.Sockets;

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 链路发送/接收标志，数值与 <see cref="SocketFlags"/> 保持一致。
    /// </summary>
    [Flags]
    public enum LinkFlags
    {
        /// <summary>
        /// 默认行为（不使用任何标志）。
        /// </summary>
        None = SocketFlags.None,

        /// <summary>
        /// 处理带外数据。
        /// </summary>
        OutOfBand = SocketFlags.OutOfBand,

        /// <summary>
        /// 查看数据但不从缓冲区移除。
        /// </summary>
        Peek = SocketFlags.Peek,

        /// <summary>
        /// 不使用路由表发送。
        /// </summary>
        DontRoute = SocketFlags.DontRoute,

        /// <summary>
        /// 指示消息被截断。
        /// </summary>
        Truncated = SocketFlags.Truncated,

        /// <summary>
        /// 指示控制数据被截断。
        /// </summary>
        ControlDataTruncated = SocketFlags.ControlDataTruncated,

        /// <summary>
        /// 指示广播数据包。
        /// </summary>
        Broadcast = SocketFlags.Broadcast,

        /// <summary>
        /// 指示多播数据包。
        /// </summary>
        Multicast = SocketFlags.Multicast,

        /// <summary>
        /// 指示部分发送或接收。
        /// </summary>
        Partial = SocketFlags.Partial
    }
}
