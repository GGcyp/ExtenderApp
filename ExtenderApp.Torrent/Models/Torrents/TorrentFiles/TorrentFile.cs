using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 多文件模式下的文件信息
    /// </summary>
    public class TorrentFile : DisposableObject, IResettable
    {
        // 文件元数据
        public ValueOrList<string> AnnounceList { get; set; }

        /// <summary>
        /// 获取Torrent文件的评论。
        /// </summary>
        /// <value>包含评论的字符串。</value>
        public string? Comment { get; set; }

        /// <summary>
        /// 获取创建Torrent文件的程序或工具的名称。
        /// </summary>
        /// <value>包含创建程序名称的字符串。</value>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 获取Torrent文件的创建日期。
        /// </summary>
        /// <value>表示创建日期的DateTime对象。</value>
        public DateTime CreationDate { get; set; }

        // 信息字典

        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 分片长度
        /// </summary>
        public long PieceLength { get; set; }

        /// <summary>
        /// 分片数组
        /// </summary>
        public byte[]? Pieces { get; set; }

        /// <summary>
        /// 是否为单文件
        /// </summary>
        /// <returns>如果文件长度大于0，则为单文件，返回true；否则为false</returns>
        public bool IsSingleFile => !FileInfoNode.HasChildNodes;

        ///// <summary>
        ///// 文件长度
        ///// </summary>
        //public long FileLength => FileInfoNode.Length;

        /// <summary>
        /// 获取Torrent文件信息列表
        /// </summary>
        public TorrentFileInfoNode FileInfoNode { get; set; }

        // InfoHash (20字节)
        /// <summary>
        /// 种子哈希值
        /// </summary>
        public InfoHash Hash { get; set; }

        public TorrentFile()
        {
            AnnounceList = new();
            FileInfoNode = TorrentFileInfoNode.Get();
        }

        public bool TryReset()
        {
            AnnounceList.Clear();
            FileInfoNode.Clear();
            Hash = InfoHash.Empty;
            Pieces = null;
            Name = string.Empty;
            CreatedBy = string.Empty;
            Comment = string.Empty;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            AnnounceList.Clear();
            AnnounceList = null;
            FileInfoNode.Clear();
            TorrentFileInfoNode.Release(FileInfoNode);
            Hash = InfoHash.Empty;
            Pieces = null;
            Name = string.Empty;
            CreatedBy = string.Empty;
            Comment = string.Empty;
        }
    }
}
