using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

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

        public override DateTime Deserialize(ref ByteBuffer buffer)
        {
            var dateData = _long.Deserialize(ref buffer);
            return DateTime.FromBinary(dateData);
        }

        public override void Serialize(ref ByteBuffer buffer, DateTime value)
        {
            var dateData = value.ToBinary();
            _long.Serialize(ref buffer, dateData);
        }

        public override long GetLength(DateTime value)
        {
            return _long.GetLength(value.ToBinary());
        }
    }
}