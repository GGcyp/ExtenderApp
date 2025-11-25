using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 日期时间格式化类
    /// </summary>
    public sealed class DateTimeFormatter : StructFormatter<DateTime>
    {
        public override int DefaultLength => 9;

        public DateTimeFormatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override DateTime Deserialize(ref ByteBuffer buffer)
        {
            var dateData = _bufferConvert.ReadInt64(ref buffer);
            return DateTime.FromBinary(dateData);
        }

        public override void Serialize(ref ByteBuffer buffer, DateTime value)
        {
            var dateData = value.ToBinary();
            _bufferConvert.Write(ref buffer, dateData);
        }

        public override long GetLength(DateTime value)
        {
            return _bufferConvert.GetByteCount(value);
        }
    }
}