

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 推送文件请求，由发送方发起，希望向接收方传输文件。
    /// </summary>
    internal readonly struct PushFileRequest
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
        /// 初始化 <see cref="PushFileRequest"/> 结构的新实例。
        /// </summary>
        /// <param name="fileId">文件的唯一标识符。</param>
        /// <param name="fileName">文件名。</param>
        /// <param name="fileSize">文件总大小（字节）。</param>
        /// <param name="chunkCount">文件被分成的块总数。</param>
        /// <param name="chunkSize">每个文件块的大小（字节）。</param>
        public PushFileRequest(Guid fileId, string fileName, long fileSize, long chunkCount, int chunkSize)
        {
            FileId = fileId;
            FileName = fileName;
            FileSize = fileSize;
            ChunkCount = chunkCount;
            ChunkSize = chunkSize;
        }
    }
}