using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// ByteFormatter 类，继承自 StructFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class ByteFormatter : StructFormatter<Byte>
    {
        public ByteFormatter(ByteBufferConvert bufferConvert, BinaryOptions options) : base(bufferConvert, options)
        {
        }

        public override byte Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadByte(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, byte value)
        {
            _bufferConvert.Write(ref buffer, value);
        }

        public override long GetLength(byte value)
        {
            if (value < _binaryOptions.BinaryRang.MaxFixPositiveInt)
            {
                return 1;
            }
            return 2;
        }
    }
}
