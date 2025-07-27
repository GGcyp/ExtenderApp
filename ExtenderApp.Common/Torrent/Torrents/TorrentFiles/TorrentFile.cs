using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// 种子文件内容的类，包含种子文件的元数据和信息字典。
    /// </summary>
    public class TorrentFile : DisposableObject, IResettable
    {
        // 文件元数据
        /// <summary>
        /// 获取或设置广播列表。
        /// </summary>
        /// <value>
        /// 一个包含字符串的ValueOrList对象，表示广播列表。
        /// </value>
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
        public HashValues Pieces { get; set; }

        /// <summary>
        /// 是否为单文件
        /// </summary>
        /// <returns>如果文件长度大于0，则为单文件，返回true；否则为false</returns>
        public bool IsSingleFile => !FileNode.HasChildNodes;

        /// <summary>
        /// 获取Torrent文件信息列表
        /// </summary>
        public FileNode FileNode { get; set; }

        // InfoHash (20字节)
        /// <summary>
        /// 种子哈希值
        /// </summary>
        public InfoHash Hash { get; set; }

        /// <summary>
        /// 获取或设置元数据版本。
        /// </summary>
        public int MetaVersion { get; set; }

        public TorrentFile()
        {
            AnnounceList = new();
            FileNode = new();
        }

        public bool TryReset()
        {
            AnnounceList.Clear();
            FileNode.Clear();
            Hash = InfoHash.Empty;
            Pieces = HashValues.Empty;
            Name = string.Empty;
            CreatedBy = string.Empty;
            Comment = string.Empty;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            AnnounceList.Clear();
            AnnounceList = null;
            FileNode.Clear();
            FileNode = null;
            Hash = InfoHash.Empty;
            Pieces = HashValues.Empty;
            Name = string.Empty;
            CreatedBy = string.Empty;
            Comment = string.Empty;
        }
    }
}
