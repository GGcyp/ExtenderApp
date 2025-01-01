using System.Buffers;
using ExtenderApp.Data.File;


namespace ExtenderApp.Data
{
    /// <summary>
    /// 二进制写入
    /// </summary>
    public ref struct BinaryWriter
    {
        /// <summary>
        /// 缓冲区写入器
        /// </summary>
        private BufferWriter writer;

        /// <summary>
        /// 获取或设置取消令牌
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// 初始化 <see cref="BinaryWriter"/> 类的新实例
        /// </summary>
        /// <param name="bufferWriter">缓冲区写入器</param>
        public BinaryWriter(IBufferWriter<byte> bufferWriter) : this()
        {
            writer = new BufferWriter(bufferWriter);
        }

        /// <summary>
        /// 使用指定的序列池和字节数组初始化 <see cref="BinaryWriter"/> 类的新实例。
        /// </summary>
        /// <param name="sequencePool">用于分配和管理序列的序列池。</param>
        /// <param name="array">要写入的字节数组。</param>
        internal BinaryWriter(SequencePool sequencePool, byte[] array): this()
        {
            writer = new BufferWriter(sequencePool, array);
        }

        /// <summary>
        /// 克隆当前实例
        /// </summary>
        /// <param name="writer">缓冲区写入器</param>
        /// <returns>新的 <see cref="BinaryWriter"/> 实例</returns>
        public BinaryWriter Clone(IBufferWriter<byte> writer) => new BinaryWriter(writer)
        {
            CancellationToken = CancellationToken,
        };

        /// <summary>
        /// 提交缓冲区中的数据
        /// </summary>
        public void Flush() 
            => writer.Commit();

        /// <summary>
        /// 获取一个指定大小的Span<byte>对象。
        /// </summary>
        /// <param name="sizeHint">建议的大小。</param>
        /// <returns>返回一个Span<byte>对象。</returns>
        public Span<byte> GetSpan(int sizeHint = 0)
            => writer.GetSpan(sizeHint);

        /// <summary>
        /// 将写入器当前位置向前移动指定的字节数。
        /// </summary>
        /// <param name="count">要移动的字节数。</param>
        public void Advance(int count)
            => writer.Advance(count);

        /// <summary>
        /// 将指定的字节序列写入写入器。
        /// </summary>
        /// <param name="source">要写入的字节序列。</param>
        public void Write(ReadOnlySpan<byte> source)
            => writer.Write(source);

        /// <summary>
        /// 将缓冲区中的数据刷新到数组中，并返回该数组。
        /// </summary>
        /// <returns>包含缓冲区中数据的字节数组。</returns>
        /// <exception cref="NotSupportedException">如果当前实例不支持此操作，则抛出此异常。</exception>
        internal byte[] FlushAndGetArray()
        {
            if (writer.TryGetUncommittedSpan(out ReadOnlySpan<byte> span))
            {
                return span.ToArray();
            }
            else
            {
                if (writer.Rental.Value == null)
                {
                    throw new NotSupportedException("This instance was not initialized to support this operation.");
                }

                Flush();
                byte[] result = writer.Rental.Value.AsReadOnlySequence.ToArray();
                writer.Rental.Dispose();
                return result;
            }
        }
    }
}
