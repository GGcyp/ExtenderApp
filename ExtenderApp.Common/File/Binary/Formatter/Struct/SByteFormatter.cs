using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    /// <summary>
    /// SByteFormatter 类，继承自 ExtenderFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class SByteFormatter : ExtenderFormatter<SByte>
    {
        public SByteFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override SByte Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadSByte(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, SByte value)
        {
            _binaryWriterConvert.Write(ref writer, value);
        }
    }
}
