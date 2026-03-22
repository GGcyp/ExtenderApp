using System.Net.Sockets;
using ExtenderApp.Abstract.Options;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// 提供 UDP 链接器使用的套接字选项标识符（与多播相关）。 这些标识符用于在选项系统中注册和绑定对应的 Socket 选项（例如加入/离开多播组）。
    /// </summary>
    public static partial class LinkOptions
    {
        /// <summary>
        /// 标识将套接字加入多播组的选项（对应 <see cref="SocketOptionName.AddMembership"/>）。 值类型为 <see cref="object"/>，实际传递的应为适配 socket 的多播组结构（例如
        /// <c>MulticastOption</c> 或 <c>IPv6MulticastOption</c>）。
        /// </summary>
        public static readonly SocketOptionIdentifier<object> AddMulticastGroupIdentifier = new("AddMulticastGroup", SocketOptionLevel.IP, SocketOptionName.AddMembership);

        /// <summary>
        /// 标识将套接字从多播组移除的选项（对应 <see cref="SocketOptionName.DropMembership"/>）。 值类型为 <see cref="object"/>，实际传递的应为适配 socket 的多播组结构（例如
        /// <c>MulticastOption</c> 或 <c>IPv6MulticastOption</c>）。
        /// </summary>
        public static readonly SocketOptionIdentifier<object> DropMulticastGroupIdentifier = new("DropMulticastGroup", SocketOptionLevel.IP, SocketOptionName.DropMembership);

        /// <summary>
        /// 是否允许发送广播（UDP）。
        /// </summary>
        public static readonly OptionIdentifier<bool> EnableBroadcastIdentifier = new("EnableBroadcast");

        /// <summary>
        /// 是否禁止 IP 分片（仅对 IPv4 有效）。
        /// </summary>
        public static readonly OptionIdentifier<bool> DontFragmentIdentifier = new("DontFragment");
    }
}