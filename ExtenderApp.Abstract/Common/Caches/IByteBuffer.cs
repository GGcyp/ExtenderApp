namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 字节缓冲区接口，集成了读写能力。
    /// </summary>
    public interface IByteBuffer : IBufferWriter<byte>, IBufferReader<byte>
    {
        /// <summary>
        /// 获取缓冲区的当前总容量。
        /// </summary>
        long Capacity { get; }

        /// <summary>
        /// 获取一个值，该值指示缓冲区在空间不足时是否可以自动扩展。
        /// </summary>
        bool CanExpand { get; }

        /// <summary>
        /// 获取缓冲区中尚未读取的数据片段。
        /// </summary>
        ArraySegment<byte> UnreadSegment { get; }
    }
}