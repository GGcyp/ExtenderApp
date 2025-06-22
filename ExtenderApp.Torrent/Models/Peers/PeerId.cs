

using System.Security.Cryptography;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示对等节点ID的结构体。
    /// </summary>
    /// <remarks>
    /// 实现了 <see cref="IEquatable{PeerId}"/> 接口，以支持对等节点ID的比较。
    /// </remarks>
    public struct PeerId : IEquatable<PeerId>
    {
        /// <summary>
        /// 对等节点ID。
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// 初始化 <see cref="PeerId"/> 结构体的新实例。
        /// </summary>
        /// <param name="id">对等节点ID。</param>
        /// <exception cref="ArgumentException">如果 <paramref name="id"/> 为空、为 null 或长度不为 20 个字符，则抛出此异常。</exception>
        public PeerId(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length != 20)
            {
                throw new ArgumentException("Peer ID must be exactly 20 characters long.", nameof(id));
            }
            _id = id;
        }

        public bool Equals(PeerId other)
        {
            return string.Equals(_id, other._id);
        }

        public static bool operator ==(PeerId left, PeerId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PeerId left, PeerId right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is PeerId && Equals((PeerId)obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}
