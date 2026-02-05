namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供字节缓冲区的写入能力接口。
    /// 该接口定义了零拷贝写入的核心方法以及常用的便捷写入重载。
    /// </summary>
    public interface IBufferWriter<T>
    {
        /// <summary>
        /// 获取当前缓冲区中已写入数据的总长度。
        /// </summary>
        long WrittenCount { get; }

        /// <summary>
        /// 剩余可写入的字节数。
        /// </summary>
        long WritableBytes { get; }

        /// <summary>
        /// 通知写入器已向 <see cref="GetMemory(int)"/> 或 <see cref="GetSpan(int)"/> 提供的区域中写入了指定数量的数据，并向前推进写入位置。
        /// </summary>
        /// <param name="count">已写入的数据字节数。该值必须是非负数。</param>
        void WriteAdvance(int count);

        /// <summary>
        /// 返回用于写入数据的 <see cref="Memory{T}"/>。
        /// </summary>
        /// <param name="sizeHint">请求的最小空间大小。若为 0，则返回默认大小的非空内存。</param>
        /// <returns>一段可供连续写入的存储区域。</returns>
        Memory<T> GetMemory(int sizeHint = 0);

        /// <summary>
        /// 返回用于写入数据的 <see cref="Span{T}"/>。
        /// </summary>
        /// <param name="sizeHint">请求的最小空间大小。若为 0，则返回默认大小的非空跨度。</param>
        /// <returns>一段可供连续写入的跨度区域。</returns>
        Span<T> GetSpan(int sizeHint = 0);

        /// <summary>
        /// 向当前位置写入一个字节。
        /// </summary>
        /// <param name="value">要写入的字节。</param>
        void Write(T value);

        /// <summary>
        /// 将指定的只读字节跨度内容写入缓冲区。
        /// </summary>
        /// <param name="source">要写入的数据源跨度。</param>
        void Write(ReadOnlySpan<T> source);

        /// <summary>
        /// 将指定的只读字节内存内容写入缓冲区。
        /// </summary>
        /// <param name="source">要写入的数据源内存。</param>
        void Write(ReadOnlyMemory<T> source);

        /// <summary>
        /// 将指定的字节数组段内容写入缓冲区。
        /// </summary>
        /// <param name="segment">包含数组引用、偏移量和长度的字节数组段。</param>
        void Write(ArraySegment<T> segment);
    }
}