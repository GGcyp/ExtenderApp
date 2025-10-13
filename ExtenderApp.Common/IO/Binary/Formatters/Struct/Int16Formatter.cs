using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// Int16Formatter 类，继承自 StructFormatter<Int16> 类，用于格式化 Int16 类型的数据。
    /// </summary>
    internal sealed class Int16Formatter : StructFormatter<Int16>
    {
        public Int16Formatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override short Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadInt16(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, short value)
        {
            _bufferConvert.WriteInt16(ref buffer, value);
        }
    }
}
