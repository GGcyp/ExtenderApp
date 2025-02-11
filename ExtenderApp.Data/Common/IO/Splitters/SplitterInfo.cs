
namespace ExtenderApp.Data
{
    /// <summary>
    /// FileSplitterInfo 结构体，用于表示文件分割信息
    /// </summary>
    public class SplitterInfo
    {
        /// <summary>
        /// 实际文件长度
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// 分块数量
        /// </summary>
        public uint ChunkCount { get; set; }

        /// <summary>
        /// 已装填区块进度
        /// </summary>
        public uint Progress { get; set; }

        /// <summary>
        /// 每个区块的最大大小
        /// </summary>
        public int MaxChunkSize { get; set; }

        /// <summary>
        /// 目标文件扩展名
        /// </summary>
        public string TargetExtensions { get; set; }

        /// <summary>
        /// 已加载的区块数据
        /// </summary>
        /// <remarks>
        /// 获取或设置已加载的区块数据。可以为null。
        /// </remarks>
        public byte[] LoadedChunks { get; set; }

        /// <summary>
        /// 检查当前对象是否为空。
        /// </summary>
        /// <returns>如果对象的长度为 -1，块数为 0，进度为 0，最大块大小为 0，则返回 true，否则返回 false。</returns>
        public bool IsEmpty => Length == -1 && ChunkCount == 0 && Progress == 0 && MaxChunkSize == 0;

        /// <summary>
        /// 判断是否完成
        /// </summary>
        /// <returns>如果块数等于进度，则返回true，表示完成；否则返回false，表示未完成</returns>
        public bool IsComplete => ChunkCount == Progress;

        /// <summary>
        /// 使用指定的参数初始化 FileSplitterInfo 类的新实例，并初始化 LoadedChunks 属性为新的字节数组。
        /// </summary>
        /// <param name="length">文件的总长度</param>
        /// <param name="chunkCount">文件的总块数</param>
        /// <param name="progress">当前处理进度</param>
        /// <param name="maxChunkSize">每个块的最大大小</param>
        /// <param name="targetExtensions">目标文件的扩展名</param>
        public SplitterInfo(long length, uint chunkCount, uint progress, int maxChunkSize, string targetExtensions) : this(length, chunkCount, progress, maxChunkSize, targetExtensions, new byte[chunkCount])
        {

        }

        /// <summary>
        /// 初始化 FileSplitterInfo 类的新实例。
        /// </summary>
        /// <param name="length">文件的总长度</param>
        /// <param name="chunkCount">文件的总块数</param>
        /// <param name="progress">当前处理进度</param>
        /// <param name="maxChunkSize">每个块的最大大小</param>
        /// <param name="targetExtensions">目标文件的扩展名</param>
        /// <param name="loadedChunks">已加载的块数据</param>
        public SplitterInfo(long length, uint chunkCount, uint progress, int maxChunkSize, string targetExtensions, byte[] loadedChunks)
        {
            Length = length;
            ChunkCount = chunkCount;
            Progress = progress;
            MaxChunkSize = maxChunkSize;
            TargetExtensions = targetExtensions;
            LoadedChunks = loadedChunks;
        }

        /// <summary>
        /// 初始化 FileSplitterInfo 类的新实例
        /// </summary>
        public SplitterInfo()
        {
            Length = -1;
            ChunkCount = 0;
            Progress = 0;
            MaxChunkSize = 0;
            TargetExtensions = string.Empty;
            LoadedChunks = new byte[ChunkCount];
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
