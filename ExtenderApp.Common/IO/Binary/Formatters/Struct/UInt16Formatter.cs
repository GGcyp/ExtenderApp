using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// UInt16Formatter 类，继承自 StructFormatter<UInt16> 类，用于格式化 UInt16 类型的数据。
    /// </summary>
    internal sealed class UInt16Formatter : StructFormatter<UInt16>
    {
        public UInt16Formatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override ushort Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadUInt16(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, ushort value)
        {
            _bufferConvert.Write(ref buffer, value);
        }
    }
}
