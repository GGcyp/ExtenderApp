using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// Int32Formatter 类，继承自 StructFormatter<Int32> 类，用于格式化 Int32 类型的数据。
    /// </summary>
    internal sealed class Int32Formatter : StructFormatter<Int32>
    {
        public Int32Formatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override int Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadInt32(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, int value)
        {
            _bufferConvert.Write(ref buffer, value);
        }

        public override long GetLength(int value)
        {
            return _bufferConvert.GetByteCount(value);
        }
    }
}
