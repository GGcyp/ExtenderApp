using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// UInt32Formatter 类，继承自 ExtenderFormatter<UInt32> 类，用于格式化 UInt32 类型的数据。
    /// </summary>
    internal sealed class UInt32Formatter : BinaryFormatter<UInt32>
    {
        public override int DefaultLength => 5;

        public UInt32Formatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override uint Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadUInt32(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, uint value)
        {
            _binaryWriterConvert.WriteUInt32(ref writer, value);
        }
    }
}
