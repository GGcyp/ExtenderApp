using System.Text;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// 表示种子文件的哈希值结构体，实现了IEquatable<TorrentHashValue>接口
    /// </summary>
    public struct InfoHash : IEquatable<InfoHash>
    {
        /// <summary>
        /// 获取一个空的 InfoHash 实例。
        /// </summary>
        /// <returns>返回一个空的 InfoHash 实例。</returns>
        public static InfoHash Empty => new InfoHash();

        /// <summary>
        /// 根据给定的字节数组生成一个 SHA1 哈希值的 InfoHash 实例。
        /// </summary>
        /// <param name="bytes">要生成哈希值的字节数组。</param>
        /// <returns>返回生成的 InfoHash 实例。</returns>
        public static InfoHash SHA1InfoHash(byte[] bytes) => new InfoHash(new HashValue(bytes), HashValue.SHA256Empty);

        /// <summary>
        /// 根据给定的字节数组生成一个 SHA256 哈希值的 InfoHash 实例。
        /// </summary>
        /// <param name="bytes">要生成哈希值的字节数组。</param>
        /// <returns>返回生成的 InfoHash 实例。</returns>
        public static InfoHash SHA256InfoHash(byte[] bytes) => new InfoHash(HashValue.SHA1Empty, new HashValue(bytes));

        /// <summary>
        /// 获取或设置种子文件的sha1哈希值
        /// </summary>
        public HashValue sha1 { get; private set; }

        /// <summary>
        /// 获取或设置种子文件的sha256哈希值
        /// </summary>
        public HashValue sha256 { get; private set; }

        /// <summary>
        /// 获取该TorrentHashValue对象是否为空
        /// </summary>
        public bool IsEmpty => sha1.IsEmpty && sha256.IsEmpty;

        /// <summary>
        /// 初始化TorrentHashValue对象
        /// </summary>
        /// <param name="sha1">种子文件的sha1哈希值</param>
        /// <param name="sha256">种子文件的sha256哈希值</param>
        /// <exception cref="ArgumentException">当sha1和sha256同时为空时抛出异常</exception>
        /// <exception cref="ArgumentException">当sha1哈希值不为空且长度不为20字节时抛出异常</exception>
        /// <exception cref="ArgumentException">当sha256哈希值不为空且长度不为32字节时抛出异常</exception>
        public InfoHash(HashValue sha1, HashValue sha256)
        {
            if (sha1.IsEmpty && sha256.IsEmpty)
                throw new ArgumentException("种子文件的sha-1和sha-256哈希值不能同时为空");
            if (!sha1.IsEmpty && sha1.Length != 20)
                throw new ArgumentException("种子文件的sha-1哈希值必须为20字节", nameof(sha1));
            if (!sha256.IsEmpty && sha256.Length != 32)
                throw new ArgumentException("种子文件的sha-256哈希值必须为32字节", nameof(sha256));

            this.sha1 = sha1;
            this.sha256 = sha256;
        }

        /// <summary>
        /// 获取sha1或sha256哈希值，优先返回sha1哈希值
        /// </summary>
        /// <returns>返回sha1哈希值，若sha1为空则返回sha256哈希值</returns>
        public HashValue GetSha1orSha256()
        {
            return !sha1.IsEmpty ? sha1 : sha256;
        }

        public override int GetHashCode()
        {
            return GetSha1orSha256().GetHashCode();
        }

        public override string ToString()
        {
            if (IsEmpty)
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            if (!sha1.IsEmpty)
            {
                builder.Append(string.Format("xt=urn:btih:{0}", sha1.ToHexString()));
            }

            if (!sha256.IsEmpty)
            {
                if (builder.Length > 0)
                    builder.Append("&");

                builder.Append(string.Format("xt=urn:btmh:{0}", sha256.ToHexString()));
            }

            return builder.ToString();
        }

        public bool Equals(InfoHash other)
        {
            if (other.IsEmpty && IsEmpty) return true;
            if (other.IsEmpty || IsEmpty) return false;

            if (sha1 == other.sha1 || sha256 == other.sha256) return true;

            return false;
        }

        public static bool operator ==(InfoHash left, InfoHash right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InfoHash left, InfoHash right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            return obj is InfoHash && Equals((InfoHash)obj);
        }
    }
}
