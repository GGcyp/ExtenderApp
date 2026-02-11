using ExtenderApp.Abstract;
using ExtenderApp.Buffer;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 时间间隔格式化器
    /// </summary>
    internal class TimeSpanFormatter : ResolverFormatter<TimeSpan>
    {
        private readonly IBinaryFormatter<long> _long;
        public override int DefaultLength => _long.DefaultLength;

        public TimeSpanFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _long = GetFormatter<long>();
        }

        public override void Serialize(AbstractBuffer<byte> buffer, TimeSpan value)
        {
            _long.Serialize(buffer, value.Ticks);
        }

        public override void Serialize(ref SpanWriter<byte> writer, TimeSpan value)
        {
            _long.Serialize(ref writer, value.Ticks);
        }

        public override TimeSpan Deserialize(AbstractBufferReader<byte> reader)
        {
            return new TimeSpan(_long.Deserialize(reader));
        }

        public override TimeSpan Deserialize(ref SpanReader<byte> reader)
        {
            return new TimeSpan(_long.Deserialize(ref reader));
        }

        public override long GetLength(TimeSpan value)
        {
            return _long.GetLength(value.Ticks);
        }
    }
}