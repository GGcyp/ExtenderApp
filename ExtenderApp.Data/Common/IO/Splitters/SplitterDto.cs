namespace ExtenderApp.Data
{
    /// <summary>
    /// 数据分片传输对象
    /// </summary>
    public struct SplitterDto
    {
        /// <summary>
        /// 分片文件名字
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 分片索引
        /// </summary>
        public uint ChunkIndex { get; set; }

        /// <summary>
        /// 数据内容
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// 初始化 SplitterDto 对象
        /// </summary>
        /// <param name="chunkIndex">分片索引</param>
        /// <param name="data">数据内容</param>
        public SplitterDto(string fileName, uint chunkIndex, byte[] data)
        {
            FileName = fileName;
            ChunkIndex = chunkIndex;
            Bytes = data;
        }
    }
}
