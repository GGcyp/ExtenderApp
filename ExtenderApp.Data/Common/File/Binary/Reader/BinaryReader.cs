using System.Buffers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 二进制读取器结构体
    /// </summary>
    public ref struct BinaryReader
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
        /// 获取下一个字节码
        /// </summary>
        /// <exception cref="EndOfStreamException">如果已到达流末尾，则抛出此异常</exception>
        public byte NextCode
        {
            get
            {
                if (reader.TryPeek(out byte code))
                {
                    throw new EndOfStreamException();
                }
                return code;
            }
        }

        /// <summary>
        /// 使用指定的内存块初始化 <see cref="BinaryReader"/> 的新实例
        /// </summary>
        /// <param name="memory">要读取的内存块</param>
        public BinaryReader(ReadOnlyMemory<byte> memory) : this()
        {
            reader = new(memory);
            Depth = 0;
        }

        /// <summary>
        /// 使用指定的只读序列初始化 <see cref="BinaryReader"/> 的新实例
        /// </summary>
        /// <param name="readOnlySequence">要读取的只读序列</param>
        public BinaryReader(scoped in ReadOnlySequence<byte> readOnlySequence) : this()
        {
            reader = new(readOnlySequence);
            Depth = 0;
        }

        /// <summary>
        /// 克隆当前 <see cref="BinaryReader"/> 实例，并使用指定的只读序列进行初始化
        /// </summary>
        /// <param name="readOnlySequence">要读取的只读序列</param>
        /// <returns>返回克隆后的 <see cref="BinaryReader"/> 实例</returns>
        public BinaryReader Clone(scoped in ReadOnlySequence<byte> readOnlySequence) => new BinaryReader(readOnlySequence)
        {
            CancellationToken = this.CancellationToken,
            Depth = this.Depth,
        };

        /// <summary>
        /// 创建一个当前 <see cref="BinaryReader"/> 实例的只读副本
        /// </summary>
        /// <returns>返回当前 <see cref="BinaryReader"/> 实例的只读副本</returns>
        public BinaryReader CreatePeekReader() => this;
    }
}
