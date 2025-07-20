using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示一个种子片段数据的类。
    /// </summary>
    public class TorrentPieceData
    {
        /// <summary>
        /// 种子文件信息节点父对象。
        /// </summary>
        private readonly TorrentFileInfoNodeParent _nodeParent;

        /// <summary>
        /// 获取种子文件信息节点。
        /// </summary>
        /// <returns>返回种子文件信息节点。</returns>
        private TorrentFileInfoNode fileInfoNode => _nodeParent.ParentNode;

        /// <summary>
        /// 获取或设置种子文件的片段列表。
        /// </summary>
        /// <value>
        /// 包含种子文件片段信息的列表，或者包含种子文件片段信息的列表的列表。如果为null，则表示没有设置片段信息。
        /// </value>
        public List<ValueOrList<TorrentFileSegmentInfo>>? Segments { get; set; }

        public TorrentPieceData(TorrentFileInfoNodeParent nodeParent)
        {
            _nodeParent = nodeParent;
        }

        /// <summary>
        /// 初始化分片信息
        /// </summary>
        private void InitializeSegments()
        {
            if (Segments != null)
                return;

            Segments = new();

            // 跟踪当前分片的信息
            long currentSegmentOffset = 0;
            ValueOrList<TorrentFileSegmentInfo>? currentSegment = null;

            foreach (var node in fileInfoNode)
            {
                if (!node.IsFile)
                    continue;

                long fileOffset = 0;
                long remainingFileLength = node.Length;

                // 处理当前分片的剩余空间
                if (currentSegment != null && currentSegmentOffset < _nodeParent.PieceLength)
                {
                    long spaceInCurrentSegment = _nodeParent.PieceLength - currentSegmentOffset;
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
                    if (currentSegmentOffset == _nodeParent.PieceLength)
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

                    long lengthToAdd = Math.Min(remainingFileLength, _nodeParent.PieceLength);

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
                    if (currentSegmentOffset == _nodeParent.PieceLength)
                    {
                        currentSegment = null;
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定分片的部分数据。
        /// </summary>
        /// <param name="pieceIndex">分片索引。</param>
        /// <param name="begin">起始偏移。</param>
        /// <param name="length">需要获取的数据长度。</param>
        /// <param name="provider">文件操作提供者。</param>
        /// <returns>返回指定分片的部分数据。</returns>
        /// <exception cref="ArgumentOutOfRangeException">如果偏移和长度超出分片范围，或者分片索引超出范围，或者偏移或长度为负数。</exception>
        /// <exception cref="InvalidOperationException">如果分片信息未初始化或为空。</exception>
        public void GetPieceAsync(int pieceIndex, int begin, int length, IFileOperateProvider provider, Action<byte[]> callback)
        {
            InitializeSegments();

            if (begin + length > _nodeParent.PieceLength)
                throw new ArgumentOutOfRangeException("偏移和长度超出分片范围");
            if (pieceIndex < 0 || pieceIndex >= Segments.Count)
                throw new ArgumentOutOfRangeException(nameof(pieceIndex), "分片索引超出范围");
            if (begin < 0 || length < 0)
                throw new ArgumentOutOfRangeException("偏移或长度不能为负数");

            var list = Segments[pieceIndex];
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("分片信息未初始化或为空");

            // 计算当前分片的起始偏移
            byte[] pieceData = ArrayPool<byte>.Shared.Rent(length);
            int currentOffset = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var segmentInfo = list[i];
                if (currentOffset < begin)
                {
                    // 如果当前偏移小于请求的偏移，则跳过
                    currentOffset += segmentInfo.Length;
                    continue;
                }
                else if (currentOffset >= begin + length)
                {
                    // 如果当前偏移已经超过请求的结束偏移，则退出循环
                    break;
                }

                // 计算需要读取的长度
                var node = segmentInfo.Node;
                _nodeParent.CreateFileOperate(provider, node);
                if (node.FileOperate == null)
                {
                    currentOffset += segmentInfo.Length;
                    continue;
                }

                int readLength = Math.Min(segmentInfo.Length, begin + length - currentOffset);
                node.FileOperate.ReadAsync(segmentInfo.OffsetInFile, readLength, pieceData, callback);
                currentOffset += segmentInfo.Length;
            }
        }

        /// <summary>
        /// 设置指定分片的部分数据。
        /// </summary>
        /// <param name="pieceIndex">分片索引。</param>
        /// <param name="begin">起始偏移。</param>
        /// <param name="length">需要设置的数据长度。</param>
        /// <param name="data">需要设置的数据。</param>
        /// <param name="provider">文件操作提供者。</param>
        /// <exception cref="ArgumentOutOfRangeException">如果偏移和长度超出分片范围，或者分片索引超出范围，或者偏移或长度为负数。</exception>
        /// <exception cref="InvalidOperationException">如果分片信息未初始化或为空。</exception>
        public void SetPiece(int pieceIndex, int begin, int length, byte[] data, IFileOperateProvider provider)
        {
            InitializeSegments();
            if (begin + length > _nodeParent.PieceLength)
                throw new ArgumentOutOfRangeException("偏移和长度超出分片范围");
            if (pieceIndex < 0 || pieceIndex >= Segments.Count)
                throw new ArgumentOutOfRangeException(nameof(pieceIndex), "分片索引超出范围");
            if (begin < 0 || length < 0)
                throw new ArgumentOutOfRangeException("偏移或长度不能为负数");
            var list = Segments[pieceIndex];
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("分片信息未初始化或为空");
            // 计算当前分片的起始偏移
            int currentOffset = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var segmentInfo = list[i];
                if (currentOffset < begin)
                {
                    // 如果当前偏移小于请求的偏移，则跳过
                    currentOffset += segmentInfo.Length;
                    continue;
                }
                else if (currentOffset >= begin + length)
                {
                    // 如果当前偏移已经超过请求的结束偏移，则退出循环
                    break;
                }
                // 计算需要写入的长度
                var node = segmentInfo.Node;
                _nodeParent.CreateFileOperate(provider, node);
                if (node.FileOperate == null)
                {
                    currentOffset += segmentInfo.Length;
                    continue;
                }
                int writeLength = Math.Min(segmentInfo.Length, begin + length - currentOffset);
                node.FileOperate.WriteAsync(data, segmentInfo.OffsetInFile, 0, writeLength);
                segmentInfo.WritedLength += writeLength;
                currentOffset += segmentInfo.Length;
            }
        }

        /// <summary>
        /// 获取指定分片是否已完全写入
        /// </summary>
        /// <param name="pieceIndex">分片索引</param>
        /// <returns>如果指定分片已完全写入，则返回true；否则返回false</returns>
        /// <exception cref="ArgumentOutOfRangeException">如果分片索引超出范围，则抛出此异常</exception>
        /// <exception cref="InvalidOperationException">如果分片信息未初始化或为空，则抛出此异常</exception>
        public bool GetPieceWrited(int pieceIndex)
        {
            InitializeSegments();
            if (pieceIndex < 0 || pieceIndex >= Segments.Count)
                throw new ArgumentOutOfRangeException(nameof(pieceIndex), "分片索引超出范围");
            var list = Segments[pieceIndex];
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("分片信息未初始化或为空");
            // 检查所有片段是否都已写入
            foreach (var segmentInfo in list)
            {
                if (segmentInfo.WritedLength < segmentInfo.Length)
                {
                    return false; // 如果有任何片段未完全写入，则返回false
                }
            }
            return true; // 所有片段都已完全写入
        }
    }
}
