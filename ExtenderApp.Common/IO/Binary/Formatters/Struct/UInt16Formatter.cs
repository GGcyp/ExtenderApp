using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatters
{
    /// <summary>
    /// UInt16Formatter 类，继承自 ExtenderFormatter<UInt16> 类，用于格式化 UInt16 类型的数据。
    /// </summary>
    internal sealed class UInt16Formatter : ExtenderFormatter<UInt16>
    {
        public override int DefaultLength => 3;

        public UInt16Formatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override ushort Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadUInt16(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, ushort value)
        {
            _binaryWriterConvert.WriteUInt16(ref writer, value);
        }
    }
}
