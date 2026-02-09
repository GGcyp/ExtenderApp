using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

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

        public override TimeSpan Deserialize(ref ByteBuffer buffer)
        {
            return new TimeSpan(_long.Deserialize(ref buffer));
        }

        public override void Serialize(ref ByteBuffer buffer, TimeSpan value)
        {
            _long.Serialize(ref buffer, value.Ticks);
        }

        public override long GetLength(TimeSpan value)
        {
            return _long.GetLength(value.Ticks);
        }
    }
}