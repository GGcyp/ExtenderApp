using System.Buffers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个可分割的数据传输对象，实现了IDisposable接口以便释放资源。
    /// </summary>
    public struct SplitterDto : IDisposable
    {
        public static SplitterDto Empty => new SplitterDto(0, Array.Empty<byte>(), 0);

        /// <summary>
        /// 获取数据块的索引。
        /// </summary>
        public uint ChunkIndex { get; }

        /// <summary>
        /// 文件字节数组。
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// 获取字节数组的长度。
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 判断字节数组是否为空。
        /// </summary>
        /// <returns>如果字节数组为空则返回true，否则返回false。</returns>
        public bool IsEmpty => Bytes == null || Bytes == Array.Empty<byte>() || Bytes.Length == 0;

        /// <summary>
        /// 初始化 SplitterDto 实例。
        /// </summary>
        /// <param name="chunkIndex">数据块的索引。</param>
        /// <param name="data">包含数据的字节数组。</param>
        /// <param name="length">字节数组的长度。</param>
        /// <param name="md5">文件的MD5哈希值。</param>
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
            if (Bytes == null) return;

            ArrayPool<byte>.Shared.Return(Bytes);
        }

        public static implicit operator Span<byte>(SplitterDto splitterDto) => splitterDto.Bytes.AsSpan(0, splitterDto.Length);
    }
}
