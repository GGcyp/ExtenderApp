namespace ExtenderApp.Data
{
    /// <summary>
    /// 文件数据传输对象。
    /// </summary>
    public readonly struct FileDto
    {
        /// <summary>
        /// 文件的唯一标识符。
        /// </summary>
        public Guid FileId { get; }

        /// <summary>
        /// 文件名。
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// 文件总大小（字节）。
        /// </summary>
        public long FileSize { get; }

        /// <summary>
        /// 文件被分成的块总数。
        /// </summary>
        public long ChunkCount { get; }

        /// <summary>
        /// 每个文件块的大小（字节）。
        /// </summary>
        public int ChunkSize { get; }

        /// <summary>
        /// 文件块的位字段数据，指示哪些块已被接收。
        /// </summary>
        public BitFieldData FileField { get; }

        public FileDto(Guid fileId, string fileName, long fileSize, long chunkCount, int chunkSize)
        {
            FileId = fileId;
            FileName = fileName;
            FileSize = fileSize;
            ChunkCount = chunkCount;
            ChunkSize = chunkSize;
            FileField = BitFieldData.Empty;
        }

        public FileDto(Guid fileId, string fileName, long fileSize, BitFieldData fileField, int chunkSize)
        {
            FileId = fileId;
            FileName = fileName;
            FileSize = fileSize;
            ChunkCount = fileField.Length;
            ChunkSize = chunkSize;
            FileField = fileField;
        }
    }
}