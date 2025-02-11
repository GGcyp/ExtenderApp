using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatter
{
    /// <summary>
    /// BooleanFormatter 类，继承自 ExtenderFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class BooleanFormatter : ExtenderFormatter<Boolean>
    {
        public override int Count => 1;

        public BooleanFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override bool Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadBoolean(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, bool value)
        {
            _binaryWriterConvert.Write(ref writer, value);
        }
    }
}
