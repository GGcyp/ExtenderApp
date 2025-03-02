using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter.Struct
{
    internal class TimeSpanFormatter : ExtenderFormatter<TimeSpan>
    {
        public override int Length => 9;

        public TimeSpanFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, TimeSpan value)
        {
            _binaryWriterConvert.Write(ref writer, value.Ticks);
        }

        public override TimeSpan Deserialize(ref ExtenderBinaryReader reader)
        {
            return new TimeSpan(_binaryReaderConvert.ReadInt64(ref reader));
        }
    }
}
