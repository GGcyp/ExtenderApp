using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatters
{
    /// <summary>
    /// Int32Formatter 类，继承自 ExtenderFormatter<Int32> 类，用于格式化 Int32 类型的数据。
    /// </summary>
    internal sealed class Int32Formatter : ExtenderFormatter<Int32>
    {
        public override int DefaultLength => 5;

        public Int32Formatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }


        public override int Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadInt32(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, int value)
        {
            _binaryWriterConvert.WriteInt32(ref writer, value);
        }
    }
}
