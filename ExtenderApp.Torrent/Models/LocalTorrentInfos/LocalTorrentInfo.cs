using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示本地种子信息的类。
    /// </summary>
    public class LocalTorrentInfo : LocalEndpointInfo
    {
        /// <summary>
        /// 获取或设置对等体的ID。
        /// </summary>
        public PeerId Id { get; set; }
    }
}
