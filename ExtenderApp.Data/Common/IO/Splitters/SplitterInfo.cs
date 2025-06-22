
namespace ExtenderApp.Data
{
    /// <summary>
    /// 分割器信息类
    /// </summary>
    public class SplitterInfo
    {
        public static readonly SplitterInfo Empty = new SplitterInfo();

        /// <summary>
        /// 已加载的区块数据
        /// </summary>
        /// <remarks>
        /// 获取或设置已加载的区块数据。可以为null。
        /// </remarks>
        public PieceData pieceData { get; }

        /// <summary>
        /// 实际文件长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 分块数量
        /// </summary>
        public uint ChunkCount { get; set; }

        private uint progress;
        /// <summary>
        /// 已装填区块进度
        /// </summary>
        public uint Progress
        {
            get => progress;
        }

        /// <summary>
        /// 每个区块的最大大小
        /// </summary>
        public int MaxChunkSize { get; set; }

        /// <summary>
        /// 目标文件扩展名
        /// </summary>
        public string TargetExtensions { get; set; }

        /// <summary>
        /// 获取或设置文件的 MD5 哈希值。
        /// </summary>
        public HashValue HashValue { get; set; }

        /// <summary>
        /// 检查当前对象是否为空。
        /// </summary>
        /// <returns>如果对象的长度为 -1，块数为 0，进度为 0，最大块大小为 0，则返回 true，否则返回 false。</returns>
        public bool IsEmpty => Length == -1 && ChunkCount == 0 && Progress == 0 && MaxChunkSize == 0;

        /// <summary>
        /// 判断是否完成
        /// </summary>
        /// <returns>如果块数等于进度，则返回true，表示完成；否则返回false，表示未完成</returns>
        public bool IsComplete => Progress >= ChunkCount;

        /// <summary>
        /// 使用指定的参数初始化 SplitterInfo 实例。
        /// </summary>
        /// <param name="length">文件总长度（以字节为单位）</param>
        /// <param name="chunkCount">文件分块数量</param>
        /// <param name="progress">当前处理进度</param>
        /// <param name="maxChunkSize">每个分块的最大大小（以字节为单位）</param>
        /// <param name="targetExtensions">目标文件的扩展名</param>
        /// <param name="hashValue">文件的 MD5 哈希值</param>
        public SplitterInfo(int length, uint chunkCount, uint progress, int maxChunkSize, string targetExtensions, HashValue hashValue) : this(length, chunkCount, progress, maxChunkSize, targetExtensions, hashValue, PieceData.Empty)
        {

        }

        /// <summary>
        /// 使用指定的参数初始化 SplitterInfo 实例。
        /// </summary>
        /// <param name="length">文件总长度（以字节为单位）</param>
        /// <param name="chunkCount">文件分块数量</param>
        /// <param name="progress">当前处理进度</param>
        /// <param name="maxChunkSize">每个分块的最大大小（以字节为单位）</param>
        /// <param name="targetExtensions">目标文件的扩展名</param>
        /// <param name="hashValue">文件的 MD5 哈希值</param>
        /// <param name="loadedChunks">已加载的分块数据数组</param>
        public SplitterInfo(int length, uint chunkCount, uint progress, int maxChunkSize, string targetExtensions, HashValue hashValue, PieceData pieceData)
        {
            Length = length;
            ChunkCount = chunkCount;
            this.progress = progress;
            MaxChunkSize = maxChunkSize;
            TargetExtensions = targetExtensions;
            this.pieceData = pieceData;
            HashValue = hashValue;
        }

        /// <summary>
        /// 初始化 FileSplitterInfo 类的新实例
        /// </summary>
        public SplitterInfo()
        {
            Length = -1;
            ChunkCount = 0;
            progress = 0;
            MaxChunkSize = 0;
            TargetExtensions = string.Empty;
            pieceData = PieceData.Empty;
            HashValue = HashValue.Empty;
        }

        public void LoadChunk(SplitterDto dto)
        {
            pieceData.LoadChunk(dto.ChunkIndex);
        }


        public void ULoadChunk(SplitterDto dto)
        {
            pieceData.ULoadChunk(dto.ChunkIndex);
        }

        public bool Equals(SplitterInfo other)
        {
            return Length == other.Length &&
                   ChunkCount == other.ChunkCount &&
                   Progress == other.Progress &&
                   MaxChunkSize == other.MaxChunkSize;
        }
    }
}
