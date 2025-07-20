using System.Net;

namespace ExtenderApp.Data
{
    public struct PeerAddress : IEquatable<PeerAddress>
    {
        /// <summary>
        /// 获取对等节点的端口号。
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 获取对等节点的IP地址。
        /// </summary>
        public IPAddress IP { get; }

        /// <summary>
        /// 判断是否为空
        /// </summary>
        /// <returns>如果IP为null，则返回true；否则返回false</returns>
        public bool IsEmpty => IP == null;

        public PeerAddress(IPEndPoint iPEndPoint) : this(iPEndPoint.Address, iPEndPoint.Port)
        {
        }

        public PeerAddress(IPAddress iPAddress, int port)
        {
            IP = iPAddress;
            Port = port;
        }

        public bool Equals(PeerAddress other)
        {
            if (other.IsEmpty && this.IsEmpty)
                return true;
            else if (other.IsEmpty || this.IsEmpty)
                return false;

            return IP.Equals(other.IP) && other.Port == this.Port;
        }

        public static bool operator ==(PeerAddress left, PeerAddress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PeerAddress left, PeerAddress right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is PeerAddress && Equals((PeerAddress)obj);
        }

        public override int GetHashCode()
        {
            return IP.GetHashCode();
        }

        public override string ToString()
        {
            return IP + ":" + Port;
        }
    }
}
