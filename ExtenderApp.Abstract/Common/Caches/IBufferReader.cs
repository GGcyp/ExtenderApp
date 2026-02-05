namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义了缓冲区读取器的通用接口，提供对数据的顺序读取和位置控制能力。
    /// </summary>
    /// <typeparam name="T">缓冲区存储的数据元素类型。</typeparam>
    public interface IBufferReader<T>
    {
        /// <summary>
        /// 获取已消耗（已读取）的数据长度。
        /// </summary>
        long Consumed { get; }

        /// <summary>
        /// 获取当前读取指针之后剩余的数据长度。
        /// </summary>
        long Remaining { get; }

        /// <summary>
        /// 获取当前位置之后未读取的数据项的只读跨度。
        /// </summary>
        public ReadOnlySpan<T> UnreadSpan { get; }

        /// <summary>
        /// 获取当前位置之后未读取的数据项的只读内存块。
        /// </summary>
        public ReadOnlyMemory<T> UnreadMemory { get; }

        /// <summary>
        /// 尝试查看当前位置的数据项而不向前推进读取指针。
        /// </summary>
        /// <param name="value">当方法返回 <see langword="true"/> 时，包含当前位置的数据项。</param>
        /// <returns>若能查看到数据则返回 <see langword="true"/>；若已到达末尾则返回 <see langword="false"/>。</returns>
        bool TryPeek(out T value);

        /// <summary>
        /// 将读取指针定位到指定的绝对位置。
        /// </summary>
        /// <param name="position">目标绝对位置。</param>
        void Seek(long position);

        /// <summary>
        /// 向前（向起始端）回退指定数量的数据项。
        /// </summary>
        /// <param name="count">要回退的数据量。</param>
        void Rewind(long count);

        /// <summary>
        /// 向后（向末尾端）跳过指定数量的数据项而不进行读取。
        /// </summary>
        /// <param name="count">要跳过的数据量。</param>
        void ReadAdvance(long count);

        /// <summary>
        /// 尝试读取当前位置的一个数据项，并向前推进指针。
        /// </summary>
        /// <param name="value">当方法返回 <see langword="true"/> 时，包含读取到的数据项。</param>
        /// <returns>若读取成功则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        bool TryRead(out T value);

        /// <summary>
        /// 将数据读取并写入到指定的跨度缓冲区中，并向前推进指针。
        /// </summary>
        /// <param name="buffer">用于接收数据的目标跨度。</param>
        void Read(scoped Span<T> buffer);

        /// <summary>
        /// 将数据读取并写入到指定的存储内存中，并向前推进指针。
        /// </summary>
        /// <param name="buffer">用于接收数据的目标存储区域。</param>
        void Read(Memory<T> buffer);
    }
}