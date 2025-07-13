using System.Net;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示对等节点信息的结构体。
    /// </summary>
    public readonly struct PeerInfo : IEquatable<PeerInfo>
    {
        /// <summary>
        /// 获取对等体地址。
        /// </summary>
        /// <returns>返回对等体地址。</returns>
        public PeerAddress PeerAddress { get; }

        /// <summary>
        /// 获取对等端地址的IP地址。
        /// </summary>
        /// <returns>返回对等端地址的IP地址。</returns>
        public IPAddress IP => PeerAddress.IP;

        /// <summary>
        /// 获取对等端地址的端口号。
        /// </summary>
        /// <returns>返回对等端地址的端口号。</returns>
        public int Port => PeerAddress.Port;

        /// <summary>
        /// 获取对等节点的ID。
        /// </summary>
        public PeerId Id { get; }

        /// <summary>
        /// 判断是否为空。
        /// </summary>
        /// <returns>如果 _peerAddress 或 Id 为空，则返回 true；否则返回 false。</returns>
        public bool IsEmpty => PeerAddress.IsEmpty || Id.IsEmpty;

        /// <summary>
        /// 初始化 <see cref="PeerInfo"/> 结构体实例。
        /// </summary>
        /// <param name="ip">对等节点的IP地址。</param>
        /// <param name="port">对等节点的端口号。</param>
        /// <param name="id">对等节点的ID。</param>
        public PeerInfo(IPAddress ip, int port, PeerId id) : this(new PeerAddress(ip, port), id)
        {

        }

        public PeerInfo(PeerAddress peerAddress, PeerId id)
        {
            PeerAddress = peerAddress;
            Id = id;
        }

        public bool Equals(PeerInfo other)
        {
            return Id.Equals(other.Id);
        }

        public static bool operator ==(PeerInfo left, PeerInfo right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(PeerInfo left, PeerInfo right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is PeerInfo && Equals((PeerInfo)obj);
        }
    }
}
