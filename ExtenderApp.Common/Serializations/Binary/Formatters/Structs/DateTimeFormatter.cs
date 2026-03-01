using ExtenderApp.Abstract;
using ExtenderApp.Buffer;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 日期时间格式化类
    /// </summary>
    public sealed class DateTimeFormatter : ResolverFormatter<DateTime>
    {
        private readonly IBinaryFormatter<long> _long;
        public override int DefaultLength => _long.DefaultLength;

        public DateTimeFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _long = GetFormatter<long>();
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, DateTime value)
        {
            var dateData = value.ToBinary();
            _long.Serialize(ref writer, dateData);
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, DateTime value)
        {
            var dateData = value.ToBinary();
            _long.Serialize(ref writer, dateData);
        }

        public override sealed DateTime Deserialize(ref BinaryReaderAdapter reader)
        {
            var dateData = _long.Deserialize(ref reader);
            return DateTime.FromBinary(dateData);
        }

        public override sealed DateTime Deserialize(ref SpanReader<byte> reader)
        {
            var dateData = _long.Deserialize(ref reader);
            return DateTime.FromBinary(dateData);
        }

        public override sealed long GetLength(DateTime value)
        {
            return _long.GetLength(value.ToBinary());
        }
    }
}