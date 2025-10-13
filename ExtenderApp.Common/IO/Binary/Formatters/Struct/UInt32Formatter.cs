using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// UInt32Formatter 类，继承自 StructFormatter<UInt32> 类，用于格式化 UInt32 类型的数据。
    /// </summary>
    internal sealed class UInt32Formatter : StructFormatter<UInt32>
    {
        public UInt32Formatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override uint Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadUInt32(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, uint value)
        {
            _bufferConvert.Write(ref buffer, value);
        }
    }
}
