using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// ByteFormatter 类，继承自 ExtenderFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class ByteFormatter : ExtenderFormatter<Byte>
    {
        public override int Length => 2;

        public ByteFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override Byte Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadByte(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, Byte value)
        {
            _binaryWriterConvert.Write(ref writer, value);
        }

        public override long GetLength(byte value)
        {
            if (value < _binaryOptions.BinaryRang.MaxFixPositiveInt)
            {
                return 1;
            }
            return 2;
        }
    }
}
