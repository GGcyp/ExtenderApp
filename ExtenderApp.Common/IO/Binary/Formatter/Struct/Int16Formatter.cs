using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatter
{
    /// <summary>
    /// Int16Formatter 类，继承自 ExtenderFormatter<Int16> 类，用于格式化 Int16 类型的数据。
    /// </summary>
    internal sealed class Int16Formatter : ExtenderFormatter<Int16>
    {
        public override int Count => 3;

        public Int16Formatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override short Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadInt16(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, short value)
        {
            _binaryWriterConvert.WriteInt16(ref writer, value);
        }
    }
}
