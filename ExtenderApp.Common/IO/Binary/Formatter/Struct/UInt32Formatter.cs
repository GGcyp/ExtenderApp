using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// UInt32Formatter 类，继承自 ExtenderFormatter<UInt32> 类，用于格式化 UInt32 类型的数据。
    /// </summary>
    internal sealed class UInt32Formatter : ExtenderFormatter<UInt32>
    {
        public override int Length => 5;

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
