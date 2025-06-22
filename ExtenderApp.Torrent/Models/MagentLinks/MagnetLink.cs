using System.Text.RegularExpressions;
using System.Text;
using System.Web;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示磁力链接的类
    /// </summary>
    internal class MagnetLink
    {
        #region 常量定义

        private const string MagnetPrefix = "magnet:?";
        private static readonly Regex MagnetRegex = new Regex(@"^magnet:\?([^#]+)(#.*)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ParamRegex = new Regex(@"([^&=]+)=([^&=]*)(&|$)", RegexOptions.Compiled);

        #endregion

        #region 属性

        /// <summary>
        /// 获取Torrent的哈希值
        /// </summary>
        /// <value>
        /// Torrent的哈希值
        /// </value>
        public InfoHash Hash { get; }

        /// <summary>
        /// 获取磁力链接的显示名称
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// 获取磁力链接的Tracker服务器列表
        /// </summary>
        public List<string>? Trackers { get; }

        /// <summary>
        /// 获取磁力链接的DHT节点列表
        /// </summary>
        public List<string>? DhtNodes { get; }

        #endregion

        public MagnetLink( InfoHash torrentHash, string? name, List<string>? trackers, List<string>? dhtNodes)
        {
            Hash = torrentHash.IsEmpty ? throw new ArgumentNullException(nameof(torrentHash)) : torrentHash;
            Name = name;
            Trackers = trackers;
            DhtNodes = dhtNodes;
        }

        /// <summary>
        /// 判断给定的URI是否是有效的磁力链接
        /// </summary>
        /// <param name="uri">要检查的URI</param>
        /// <returns>如果是有效的磁力链接返回true，否则返回false</returns>
        public static bool IsValidMagnetUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return false;

            return MagnetRegex.IsMatch(uri.Trim());
        }


        /// <summary>
        /// 返回磁力链接的字符串表示形式
        /// </summary>
        /// <returns>磁力链接的原始URI</returns>
        public override string ToString()
        {
            return ToStandardFormat();
        }

        /// <summary>
        /// 生成磁力链接的标准格式字符串
        /// </summary>
        /// <returns>标准格式的磁力链接字符串</returns>
        public string ToStandardFormat()
        {
            var builder = new StringBuilder(MagnetPrefix);

            // 添加哈希值
            if ( !Hash.IsEmpty)
            {
                //builder.AppendFormat("xt=urn:{0}:{1}", ProtocolType, Hash);
            }

            // 添加显示名称
            if (!string.IsNullOrEmpty(Name))
            {
                builder.AppendFormat("&dn={0}", HttpUtility.UrlEncode(Name));
            }

            if (Trackers != null)
            {
                // 添加Tracker服务器
                foreach (var tracker in Trackers)
                {
                    builder.AppendFormat("&tr={0}", HttpUtility.UrlEncode(tracker));
                }
            }

            if (DhtNodes != null)
            {
                // 添加DHT节点
                foreach (var node in DhtNodes)
                {
                    builder.AppendFormat("&x.pe={0}", HttpUtility.UrlEncode(node));
                }
            }

            return builder.ToString();
        }
    }
}
