using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// Int64Formatter 类，继承自 StructFormatter<Int64> 类，用于格式化 Int64 类型的数据。
    /// </summary>
    internal sealed class Int64Formatter : StructFormatter<Int64>
    {
        public Int64Formatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override long Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadInt64(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, long value)
        {
            _bufferConvert.WriteInt64(ref buffer, value);
        }

        public override long GetLength(long value)
        {
            return _bufferConvert.GetByteCount(value);
        }
    }
}
