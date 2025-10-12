using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// UInt64Formatter 类，继承自 ExtenderFormatter<UInt64> 类，用于格式化 UInt64 类型的数据。
    /// </summary>
    internal sealed class UInt64Formatter : BinaryFormatter<UInt64>
    {
        public override int DefaultLength => 9;

        public UInt64Formatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override ulong Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadUInt64(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, ulong value)
        {
            _binaryWriterConvert.WriteUInt64(ref writer, value);
        }
    }
}
