using ExtenderApp.Common.IO;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// 表示用于管理种子文件下载信息节点（<see cref="TorrentFileInfoNode"/>）的父节点类。
    /// 该类继承自 <see cref="FileOperateNodeParent{TorrentFileDownInfoNode}"/>，用于组织和管理与
    /// 单个 Torrent 文件相关的所有下载信息节点，并可扩展存储种子文件名等额外信息。
    /// </summary>
    /// <remarks>
    /// 典型用法：用于描述一个 Torrent 文件下所有待下载文件的结构树，便于批量管理和操作。
    /// </remarks>
    public class TorrentFileInfoNodeParent : FileOperateNodeParent<TorrentFileInfoNode>
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
        /// 获取或设置文件的长度。
        /// </summary>
        public long FileLength { get; set; }

        /// <summary>
        /// 上传的数据量
        /// </summary>
        internal long Uploaded;

        /// <summary>
        /// 下载的数据量
        /// </summary>
        internal long Downloaded;

        /// <summary>
        /// 剩余的数据量
        /// </summary>
        internal long Left;

        /// <summary>
        /// 获取或设置分片哈希值
        /// </summary>
        public HashValues PieceHashValues { get; set; }

        /// <summary>
        /// 获取或设置种子文件的片段列表。
        /// </summary>
        /// <value>
        /// 包含种子文件片段信息的列表，或者包含种子文件片段信息的列表的列表。如果为null，则表示没有设置片段信息。
        /// </value>
        public TorrentPieceData PieceData { get; set; }

        /// <summary>
        /// 获取或设置本地位字段数据。
        /// </summary>
        public BitFieldData LocalBiteField { get; set; }

        public TorrentFileInfoNodeParent()
        {
            PieceData = new(this);
        }

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
            LocalBiteField = new BitFieldData(PieceHashValues.PieceCount);
            FileLength = ParentNode.GetLength();
        }
    }
}
