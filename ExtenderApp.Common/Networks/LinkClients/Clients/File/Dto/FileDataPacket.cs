using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 文件数据包。
    /// </summary>
    internal readonly struct FileDataPacket : IDisposable
    {
        /// <summary>
        /// 文件的唯一标识符。
        /// </summary>
        public Guid FileId { get; }

        /// <summary>
        /// 数据块在文件中的起始位置。
        /// </summary>
        public long Position { get; }

        /// <summary>
        /// 数据块内容。
        /// </summary>
        public ByteBlock Data { get; }

        public FileDataPacket(Guid fileId, long position, ByteBlock data)
        {
            FileId = fileId;
            Position = position;
            Data = data;
        }

        public void Dispose()
        {
            Data.Dispose();
        }
    }
}