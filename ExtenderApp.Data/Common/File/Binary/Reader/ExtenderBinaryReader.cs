using System.Buffers;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 二进制读取器结构体
    /// </summary>
    public ref struct ExtenderBinaryReader
    {
        /// <summary>
        /// 私有二进制序列读取器
        /// </summary>
        private File.Binary.SequenceReader<byte> reader;

        /// <summary>
        /// 获取或设置取消令牌
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// 获取或设置读取深度
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// 获取读取的二进制序列
        /// </summary>
        public ReadOnlySequence<byte> Sequence => reader.Sequence;

        /// <summary>
        /// 获取当前未读取的字节序列。
        /// </summary>
        /// <returns>返回一个只读字节序列，表示当前未读取的字节。</returns>
        public readonly ReadOnlySpan<byte> UnreadSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => reader.UnreadSpan;
        }

        /// <summary>
        /// 获取当前读取位置
        /// </summary>
        public SequencePosition Position => reader.Position;

        /// <summary>
        /// 获取已读取的字节数
        /// </summary>
        public long Consumed => reader.Consumed;

        /// <summary>
        /// 获取是否到达序列末尾
        /// </summary>
        public bool End => reader.End;

        /// <summary>
        /// 剩余未读字节
        /// </summary>
        public long Remaining => reader.Remaining;

        /// <summary>
        /// 获取当前跨度的索引。
        /// </summary>
        /// <returns>返回当前跨度的索引。</returns>
        public int CurrentSpanIndex => reader.CurrentSpanIndex;

        /// <summary>
        /// 获取当前读取器中的只读字节序列。
        /// </summary>
        /// <returns>返回当前读取器中的只读字节序列。</returns>
        public ReadOnlySpan<byte> CurrentSpan => reader.CurrentSpan;

        /// <summary>
        /// 获取下一个字节码
        /// </summary>
        /// <exception cref="EndOfStreamException">如果已到达流末尾，则抛出此异常</exception>
        public byte NextCode
        {
            get
            {
                if (!reader.TryPeek(out byte code))
                {
                    throw new EndOfStreamException();
                }
                return code;
            }
        }

        /// <summary>
        /// 使用指定的内存块初始化 <see cref="ExtenderBinaryReader"/> 的新实例
        /// </summary>
        /// <param name="memory">要读取的内存块</param>
        public ExtenderBinaryReader(ReadOnlyMemory<byte> memory) : this()
        {
            reader = new(memory);
            Depth = 0;
        }

        /// <summary>
        /// 使用指定的只读序列初始化 <see cref="ExtenderBinaryReader"/> 的新实例
        /// </summary>
        /// <param name="readOnlySequence">要读取的只读序列</param>
        public ExtenderBinaryReader(scoped in ReadOnlySequence<byte> readOnlySequence) : this()
        {
            reader = new(readOnlySequence);
            Depth = 0;
        }

        /// <summary>
        /// 克隆当前 <see cref="ExtenderBinaryReader"/> 实例，并使用指定的只读序列进行初始化
        /// </summary>
        /// <param name="readOnlySequence">要读取的只读序列</param>
        /// <returns>返回克隆后的 <see cref="ExtenderBinaryReader"/> 实例</returns>
        public ExtenderBinaryReader Clone(scoped in ReadOnlySequence<byte> readOnlySequence) => new ExtenderBinaryReader(readOnlySequence)
        {
            CancellationToken = this.CancellationToken,
            Depth = this.Depth,
        };

        /// <summary>
        /// 创建一个当前 <see cref="ExtenderBinaryReader"/> 实例的只读副本
        /// </summary>
        /// <returns>返回当前 <see cref="ExtenderBinaryReader"/> 实例的只读副本</returns>
        public ExtenderBinaryReader CreatePeekReader() => this;

        /// <summary>
        /// 尝试向前推进指定的数量。
        /// </summary>
        /// <param name="count">要推进的数量。</param>
        /// <returns>如果成功推进了指定的数量，则返回true；否则返回false。</returns>
        public bool TryAdvance(long count)
        {
            return reader.TryAdvance(count);
        }

        /// <summary>
        /// 向前移动读取器指针。
        /// </summary>
        /// <param name="count">要移动的字节数。</param>
        public void Advance(long count)
        {
            reader.Advance(count);
        }

        /// <summary>
        /// 尝试从数据源中读取一个字节数据。
        /// </summary>
        /// <param name="value">输出参数，用于存储读取到的字节数据。</param>
        /// <returns>如果成功读取到一个字节数据，则返回true；否则返回false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead(out byte value)
        {
            return reader.TryRead(out value);
        }

        /// <summary>
        /// 将读取器的位置倒回指定的字节数。
        /// </summary>
        /// <param name="count">要倒回的字节数。</param>
        public void Rewind(long count)
        {
            reader.Rewind(count);
        }

        /// <summary>
        /// 尝试将当前数据复制到指定的字节跨度中。
        /// </summary>
        /// <param name="destination">目标字节跨度。</param>
        /// <returns>如果数据成功复制到目标字节跨度中，则返回true；否则返回false。</returns>
        public readonly bool TryCopyTo(Span<byte> destination)
        {
            return reader.TryCopyTo(destination);
        }
    }
}
