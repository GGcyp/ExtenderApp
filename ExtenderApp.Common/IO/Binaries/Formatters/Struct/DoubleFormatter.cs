using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// DoubleFormatter 类，继承自 ExtenderFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class DoubleFormatter : BinaryFormatter<Double>
    {
        public override int DefaultLength => 9;

        public DoubleFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override double Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadDouble(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, double value)
        {
            _binaryWriterConvert.Write(ref writer, value);
        }
    }
}
