using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 多文件模式下的文件信息
    /// </summary>
    public class TorrentFile
    {
        /// <summary>
        /// 表示一个Torrent文件信息节点的类。
        /// </summary>
        public class TorrentFileInfoNode : Node<TorrentFileInfoNode>, IResettable
        {
            private static readonly ObjectPool<TorrentFileInfoNode> _pool
                = ObjectPool.CreateDefaultPool<TorrentFileInfoNode>();

            public static TorrentFileInfoNode Get() => _pool.Get();
            public static void Release(TorrentFileInfoNode node) => _pool.Release(node);

            /// <summary>
            /// 获取或设置文件的名称。
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// 获取或设置文件的大小（以字节为单位）。
            /// </summary>
            public long Length { get; set; }

            /// <summary>
            /// 获取或设置一个值，该值指示当前节点是否表示一个文件。
            /// </summary>
            public bool IsFile { get; set; } = false;

            /// <summary>
            /// 获取或设置文件的完整路径。
            /// </summary>
            public string? FullPath { get; set; }

            /// <summary>
            /// 获取或设置文件操作接口
            /// </summary>
            /// <value>文件操作接口</value>
            public IFileOperate? fileOperate { get; set; }

            public bool TryReset()
            {
                Name = string.Empty;
                Length = 0;
                FullPath = string.Empty;
                fileOperate = null;
                return true;
            }
        }

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
        public bool IsSingleFile => FileInfoNode?.Count > 1;

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
            AnnounceList = new ValueOrList<string>();
            FileInfoNode = TorrentFileInfoNode.Get();
        }
    }
}
