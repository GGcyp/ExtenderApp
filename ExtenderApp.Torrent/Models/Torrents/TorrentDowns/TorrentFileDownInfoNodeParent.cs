using ExtenderApp.Common.IO;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示用于管理种子文件下载信息节点（<see cref="TorrentFileDownInfoNode"/>）的父节点类。
    /// 该类继承自 <see cref="FileOperateNodeParent{TorrentFileDownInfoNode}"/>，用于组织和管理与
    /// 单个 Torrent 文件相关的所有下载信息节点，并可扩展存储种子文件名等额外信息。
    /// </summary>
    /// <remarks>
    /// 典型用法：用于描述一个 Torrent 文件下所有待下载文件的结构树，便于批量管理和操作。
    /// </remarks>
    public class TorrentFileDownInfoNodeParent : FileOperateNodeParent<TorrentFileDownInfoNode>
    {
        /// <summary>
        /// 获取或设置当前父节点所关联的 Torrent 文件名（含扩展名）。
        /// </summary>
        public LocalFileInfo TorrentFileInfo { get; set; }

        /// <summary>
        /// 公告列表属性。
        /// </summary>
        /// <value>
        /// 公告列表，类型为<see cref="ValueOrList{string}"/>，可以为null。
        /// </value>
        public ValueOrList<string>? AnnounceList { get; set; }

        /// <summary>
        /// 获取或设置InfoHash属性
        /// </summary>
        public InfoHash Hash { get; set; }

        /// <summary>
        /// 获取或设置分片长度。
        /// </summary>
        public long PieceLength { get; set; }

        /// <summary>
        /// 获取或设置分片哈希值
        /// </summary>
        public HashValues PieceHashValues { get; set; }

        public TorrentFileSegment[] Segments { get; set; }

        /// <summary>
        /// 设置Torrent文件信息。
        /// </summary>
        /// <param name="torrentFile">要设置的Torrent文件。</param>
        /// <exception cref="ArgumentNullException">如果<paramref name="torrentFile"/>为null，则抛出此异常。</exception>
        public void Set(TorrentFile torrentFile)
        {
            if (torrentFile == null)
                throw new ArgumentNullException(nameof(torrentFile), "Torrent文件不可为空");

            Hash = torrentFile.Hash;
            AnnounceList = torrentFile.AnnounceList;
            ParentNode.SetTorrentInfo(torrentFile.FileNode);
            PieceHashValues = torrentFile.Pieces;
            PieceLength = torrentFile.PieceLength;

            Segments = new TorrentFileSegment[PieceHashValues.PieceCount];

        }
    }
}
