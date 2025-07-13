using System.Diagnostics;
using ExtenderApp.Common.IO;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
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
        /// 获取或设置分片哈希值
        /// </summary>
        public HashValues PieceHashValues { get; set; }

        /// <summary>
        /// 获取或设置种子文件的片段列表。
        /// </summary>
        /// <value>
        /// 包含种子文件片段信息的列表，或者包含种子文件片段信息的列表的列表。如果为null，则表示没有设置片段信息。
        /// </value>
        public List<ValueOrList<TorrentFileSegmentInfo>>? Segments { get; set; }

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
        }

        /// <summary>
        /// 初始化分片。
        /// </summary>
        /// <remarks>
        /// 此方法用于初始化分片，如果Segments属性不为null，则直接返回。
        /// 方法会遍历ParentNode中的所有节点，对每个文件节点进行处理，
        /// 根据分片长度PieceLength将文件分成多个分片，并将分片信息存储在Segments集合中。
        /// 每个分片信息包含多个TorrentFileSegmentInfo对象，表示分片中包含的文件段信息。
        /// </remarks>
        public void InitializeSegments()
        {
            if (Segments != null)
                return;

            Segments = new();

            // 跟踪当前分片的信息
            long currentSegmentOffset = 0;
            ValueOrList<TorrentFileSegmentInfo>? currentSegment = null;

            foreach (var node in ParentNode)
            {
                if (!node.IsFile)
                    continue;

                long fileOffset = 0;
                long remainingFileLength = node.Length;

                // 处理当前分片的剩余空间
                if (currentSegment != null && currentSegmentOffset < PieceLength)
                {
                    long spaceInCurrentSegment = PieceLength - currentSegmentOffset;
                    long lengthToAdd = Math.Min(remainingFileLength, spaceInCurrentSegment);

                    currentSegment.Add(new TorrentFileSegmentInfo
                    {
                        Node = node,
                        OffsetInFile = fileOffset,
                        Length = (int)lengthToAdd // 确保Length在int范围内
                    });

                    fileOffset += lengthToAdd;
                    remainingFileLength -= lengthToAdd;
                    currentSegmentOffset += lengthToAdd;

                    // 当前分片已满，创建新分片
                    if (currentSegmentOffset == PieceLength)
                    {
                        currentSegment = null;
                    }
                }

                // 为剩余数据创建新分片
                while (remainingFileLength > 0)
                {
                    currentSegment = new ValueOrList<TorrentFileSegmentInfo>();
                    Segments.Add(currentSegment);
                    currentSegmentOffset = 0;

                    long lengthToAdd = Math.Min(remainingFileLength, PieceLength);

                    currentSegment.Add(new TorrentFileSegmentInfo
                    {
                        Node = node,
                        OffsetInFile = fileOffset,
                        Length = (int)lengthToAdd // 确保Length在int范围内
                    });

                    fileOffset += lengthToAdd;
                    remainingFileLength -= lengthToAdd;
                    currentSegmentOffset += lengthToAdd;

                    // 如果添加的数据等于分片长度，则重置当前分片
                    if (currentSegmentOffset == PieceLength)
                    {
                        currentSegment = null;
                    }
                }
            }

            //// 调试输出
            //Debug.Print($"分片数量: {Segments.Count}, 分片长度: {PieceLength}");
            //if (Segments.Count > 0)
            //{
            //    var lastSegment = Segments[Segments.Count - 1];
            //    Debug.Print($"最后一个分片包含 {lastSegment.Count} 个文件段");
            //    foreach (var segmentInfo in lastSegment)
            //    {
            //        Debug.Print($"  文件: {segmentInfo.Node.Name}, 偏移: {segmentInfo.OffsetInFile}, 长度: {segmentInfo.Length}");
            //    }
            //}
        }

        public void GetPiece(int pieceIndex, int begin, int length, ref ExtenderBinaryWriter writer)
        {
            InitializeSegments();
        }

        #region 辅助方法

        /// <summary>
        /// 调试输出分片信息
        /// </summary>
        [Conditional("DEBUG")]
        private void DebugPrintSegmentInfo()
        {
            if (Segments == null || Segments.Count == 0)
            {
                Debug.Print("没有分片信息可供输出");
                return;
            }

            Debug.Print($"分片数量: {Segments.Count}, 分片长度: {PieceLength}");
            if (Segments.Count > 0)
            {
                var lastSegment = Segments[Segments.Count - 1];
                Debug.Print($"最后一个分片包含 {lastSegment.Count} 个文件段");
                foreach (var segmentInfo in lastSegment)
                {
                    Debug.Print($"  文件: {segmentInfo.Node.Name}, 偏移: {segmentInfo.OffsetInFile}, 长度: {segmentInfo.Length}");
                }
            }
        }

        #endregion
    }
}
