

using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示对等节点ID的结构体。
    /// </summary>
    /// <remarks>
    /// 实现了 <see cref="IEquatable{PeerId}"/> 接口，以支持对等节点ID的比较。
    /// </remarks>
    public struct PeerId : IEquatable<PeerId>
    {
        public const string CLITNET_PREFIX = "-EX0001-";

        public static PeerId CreateId()
        {
            //char[] buffer = new char[20];
            //我只需要guid前12位字符
            string id = CLITNET_PREFIX + Guid.NewGuid().ToString("N").Substring(0, 12);
            var bytes = Encoding.UTF8.GetBytes(id, 0, id.Length);
            return new PeerId(bytes);
        }

        /// <summary>
        /// 对等节点ID。
        /// </summary>
        internal byte[] Id { get; }

        public bool IsEmpty => Id == null;

        /// <summary>
        /// 初始化 <see cref="PeerId"/> 结构体的新实例。
        /// </summary>
        /// <param name="id">对等节点ID。</param>
        /// <exception cref="ArgumentException">如果 <paramref name="id"/> 为空、为 null 或长度不为 20 个字符，则抛出此异常。</exception>
        public PeerId(byte[] id)
        {
            if (id.Length != 20)
            {
                throw new ArgumentException("对等节点ID不能不为20个字节", nameof(id));
            }
            Id = id;
        }

        public bool Equals(PeerId other)
        {
            return string.Equals(Id, other.Id);
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
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(Id);
        }
    }
}
