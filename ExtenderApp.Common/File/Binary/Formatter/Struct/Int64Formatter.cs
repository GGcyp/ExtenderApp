using ExtenderApp.Data;

namespace ExtenderApp.Common.Files.Binary.Formatter
{
    /// <summary>
    /// Int64Formatter 类，继承自 ExtenderFormatter<Int64> 类，用于格式化 Int64 类型的数据。
    /// </summary>
    internal sealed class Int64Formatter : ExtenderFormatter<Int64>
    {
        public override int Count => 9;

        public Int64Formatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override long Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadInt64(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, long value)
        {
            _binaryWriterConvert.WriteInt64(ref writer, value);
        }
    }
}
