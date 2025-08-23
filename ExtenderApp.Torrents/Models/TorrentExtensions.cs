

using System.Text;
using MonoTorrent;

namespace ExtenderApp.Torrents.Models
{
    /// <summary>
    /// Monotorrent中的Torrent对象扩展方法
    /// </summary>
    public static class TorrentExtensions
    {
        /// <summary>
        /// 从Torrent对象生成完整的磁力链接
        /// </summary>
        /// <param name="torrent">Torrent对象</param>
        /// <returns>完整的磁力链接字符串</returns>
        public static string GetMagnetLink(this Torrent torrent)
        {
            if (torrent == null)
                throw new ArgumentNullException(nameof(torrent), "Torrent对象不能为null");

            StringBuilder builder = new();
            builder.Append("magnet:?");

            // 构建磁力链接的基础部分（必须包含info hash）
            // xt字段：精确主题，包含info hash（btih表示BitTorrent Info Hash）
            InfoHashes infoHashes = torrent.InfoHashes;
            if (infoHashes.V1 != null)
            {
                builder.Append($"xt=urn:btih:{infoHashes.V1.ToHex()}");
            }
            if (infoHashes.V1 != null && infoHashes.V2 != null)
            {
                builder.Append('&');
            }
            if (infoHashes.V2 != null)
            {

                builder.Append($"xt=urn:btmh:{infoHashes.V2.ToHex()}");
            }

            // 添加显示名称（dn字段，可选但推荐）
            if (!string.IsNullOrWhiteSpace(torrent.Name))
            {
                builder.Append('&');
                builder.Append($"dn={Uri.EscapeDataString(torrent.Name)}");
            }

            // 添加所有跟踪器地址（tr字段，多个跟踪器需要重复添加）
            foreach (var trackerTier in torrent.AnnounceUrls)
            {
                foreach (var trackerUrl in trackerTier)
                {
                    if (!string.IsNullOrWhiteSpace(trackerUrl.ToString()))
                    {
                        builder.Append('&');
                        builder.Append($"tr={Uri.EscapeDataString(trackerUrl.ToString())}");
                    }
                }
            }

            // 添加文件大小信息（xl字段，可选）
            if (torrent.Size > 0)
            {
                builder.Append("&");
                builder.Append($"xl={torrent.Size}");
            }

            // 组合所有部分，形成完整的磁力链接
            return builder.ToString();
        }
    }
}
