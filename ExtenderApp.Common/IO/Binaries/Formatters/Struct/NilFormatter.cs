using ExtenderApp.Common.IO.Binary;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    internal class NilFormatter : BinaryFormatter<Nil>
    {
        public override int DefaultLength => 1;

        public NilFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override Nil Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.TryReadNil(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, Nil value)
        {
            if (value.IsNil)
            {
                _binaryWriterConvert.WriteNil(ref writer);
            }
        }
    }
}
