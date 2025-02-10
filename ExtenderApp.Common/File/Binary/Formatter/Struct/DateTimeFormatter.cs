using ExtenderApp.Data;

namespace ExtenderApp.Common.Files.Binary.Formatter
{
    /// <summary>
    /// 日期时间格式化类
    /// </summary>
    public sealed class DateTimeFormatter : ExtenderFormatter<DateTime>
    {
        public override int Count => 9;

        public DateTimeFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions binaryOptions) : base(binaryWriterConvert, binaryReaderConvert, binaryOptions)
        {
        }

        public override DateTime Deserialize(ref ExtenderBinaryReader reader)
        {
            var dateData = _binaryReaderConvert.ReadInt64(ref reader);
            return DateTime.FromBinary(dateData);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, DateTime value)
        {
            var dateData = value.ToBinary();
            _binaryWriterConvert.Write(ref writer, dateData);
        }
    }
}
