namespace ExtenderApp.Data
{
    /// <summary>
    /// 套接字选项级别枚举，用于在设置或获取套接字选项时指定选项所属的协议/层级。
    /// <para>数值与底层协议编号或实现约定保持一致（例如 TCP=6, UDP=17）。</para>
    /// </summary>
    public enum LinkOptionLevel
    {
        /// <summary>
        /// Internet Protocol (IP) 层选项级别（值 = 0）。
        /// </summary>
        IP = 0,

        /// <summary>
        /// Transmission Control Protocol (TCP) 层选项级别（协议编号 = 6）。
        /// </summary>
        Tcp = 6,

        /// <summary>
        /// User Datagram Protocol (UDP) 层选项级别（协议编号 = 17）。
        /// </summary>
        Udp = 17,

        /// <summary>
        /// Internet Protocol version 6 (IPv6) 层选项级别（协议编号 = 41）。
        /// </summary>
        IPv6 = 41,

        /// <summary>
        /// Socket 层级别，用于对套接字本身的选项（通常对应于 <c>SocketOptionLevel.Socket</c>）。
        /// <para>此值在不同平台/实现中可能与系统定义一致或被映射为本库的自定义级别。</para>
        /// </summary>
        Socket = 65535
    }
}