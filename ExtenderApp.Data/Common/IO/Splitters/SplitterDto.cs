using System.Buffers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个可分割的数据传输对象，实现了IDisposable接口以便释放资源。
    /// </summary>
    public struct SplitterDto : IDisposable
    {
        /// <summary>
        /// 获取数据块的索引。
        /// </summary>
        public uint ChunkIndex { get; }

        /// <summary>
        /// 获取字节数组。
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// 获取字节数组的长度。
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 初始化 SplitterDto 实例。
        /// </summary>
        /// <param name="chunkIndex">数据块的索引。</param>
        /// <param name="data">包含数据的字节数组。</param>
        /// <param name="length">字节数组的长度。</param>
        public SplitterDto(uint chunkIndex, byte[] data, int length)
        {
            ChunkIndex = chunkIndex;
            Bytes = data;
            Length = length;
        }

        /// <summary>
        /// 释放由 SplitterDto 使用的资源。
        /// </summary>
        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(Bytes);
        }
    }
}
