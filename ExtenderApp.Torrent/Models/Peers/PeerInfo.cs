using System.Net;
using System.Text;

namespace ExtenderApp.Torrent.Models.Peers
{
    /// <summary>
    /// 表示对等节点信息的结构体。
    /// </summary>
    public readonly struct PeerInfo : IEquatable<PeerInfo>
    {
        /// <summary>
        /// 获取对等节点的IP地址。
        /// </summary>
        public IPAddress IP { get; }

        /// <summary>
        /// 获取对等节点的端口号。
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 获取对等节点的ID。
        /// </summary>
        public PeerId Id { get; }

        /// <summary>
        /// 初始化 <see cref="PeerInfo"/> 结构体实例。
        /// </summary>
        /// <param name="ip">对等节点的IP地址。</param>
        /// <param name="port">对等节点的端口号。</param>
        /// <param name="id">对等节点的ID。</param>
        public PeerInfo(IPAddress ip, int port, PeerId id)
        {
            IP = ip;
            Port = port;
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
