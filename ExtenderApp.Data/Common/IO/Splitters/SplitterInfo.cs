
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
        public readonly byte[] LoadedChunks;

        /// <summary>
        /// 实际文件长度
        /// </summary>
        public long Length { get; set; }

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
        public string FileMD5 { get; set; }

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
        /// <param name="md5HASH">文件的 MD5 哈希值</param>
        public SplitterInfo(long length, uint chunkCount, uint progress, int maxChunkSize, string targetExtensions, string md5HASH) : this(length, chunkCount, progress, maxChunkSize, targetExtensions, md5HASH, new byte[chunkCount])
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
        /// <param name="md5HASH">文件的 MD5 哈希值</param>
        /// <param name="loadedChunks">已加载的分块数据数组</param>
        public SplitterInfo(long length, uint chunkCount, uint progress, int maxChunkSize, string targetExtensions, string md5HASH, byte[] loadedChunks)
        {
            Length = length;
            ChunkCount = chunkCount;
            this.progress = progress;
            MaxChunkSize = maxChunkSize;
            TargetExtensions = targetExtensions;
            LoadedChunks = loadedChunks;
            FileMD5 = md5HASH;
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
            LoadedChunks = Array.Empty<byte>();
            FileMD5 = string.Empty;
        }

        /// <summary>
        /// 获取最后一个数据块的索引。
        /// </summary>
        /// <returns>返回最后一个数据块的索引。</returns>
        public uint GetLastChunkIndex()
        {
            for (uint i = 0; i < ChunkCount; i++)
            {
                if (LoadedChunks[i] == 0)
                {
                    return i;
                }
            }
            return Progress;
        }

        /// <summary>
        /// 获取最后一个块索引位置
        /// </summary>
        /// <returns>返回最后一个块索引位置</returns>
        public long GetLastChunkIndexPosition()
        {
            var result = GetLastChunkIndex();
            return GetPosition(result);
        }

        /// <summary>
        /// 获取最后一个数据块的大小。
        /// </summary>
        /// <returns>返回最后一个数据块的大小。</returns>
        public int GetLastChunkSize()
        {
            return (int)(Length - (ChunkCount - 1) * MaxChunkSize);
        }

        /// <summary>
        /// 根据块索引获取位置
        /// </summary>
        /// <param name="index">块索引</param>
        /// <returns>返回对应块的位置</returns>
        /// <exception cref="ArgumentOutOfRangeException">如果块索引超出范围，则抛出此异常</exception>
        public long GetPosition(uint index)
        {
            if (index >= ChunkCount || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "块索引超出范围");
            }

            return (index * MaxChunkSize);
        }

        /// <summary>
        /// 向指定位置添加一个块。
        /// </summary>
        /// <param name="index">要添加块的位置。</param>
        /// <exception cref="ArgumentOutOfRangeException">如果块索引超出范围，则抛出此异常。</exception>
        public void AddChunk(uint index)
        {
            if (index >= ChunkCount || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "块索引超出范围");
            }

            lock (LoadedChunks)
            {
                LoadedChunks[index] = 1;
                //progress++;
            }
            Interlocked.Increment(ref progress);
        }

        /// <summary>
        /// 从指定位置移除一个块。
        /// </summary>
        /// <param name="index">要移除块的位置。</param>
        /// <exception cref="ArgumentOutOfRangeException">如果块索引超出范围，则抛出此异常。</exception>
        public void RemoveChunk(uint index)
        {
            if (index >= ChunkCount || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "块索引超出范围");
            }

            lock (LoadedChunks)
            {
                LoadedChunks[index] = 0;
            }

            Interlocked.Decrement(ref progress);
        }

        /// <summary>
        /// 获取指定位置所在的块索引
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>返回指定位置所在的块索引</returns>
        public uint GetChunkIndex(long position)
        {
            return (uint)(position / MaxChunkSize);
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
