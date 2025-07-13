using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示块映射到的文件片段信息
    /// </summary>
    public class TorrentFileSegmentInfo
    {
        /// <summary>
        /// 文件在列表中的索引（对应种子文件顺序）
        /// </summary>
        public TorrentFileInfoNode? Node { get; set; }

        /// <summary>
        /// 该片段在文件内的起始偏移量（字节）
        /// </summary>
        public long OffsetInFile { get; set; }

        /// <summary>
        /// 该片段的长度（字节）
        /// </summary>
        public int Length { get; set; }
    }
}
