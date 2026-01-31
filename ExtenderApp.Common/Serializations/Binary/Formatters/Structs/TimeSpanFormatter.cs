using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 时间间隔格式化器
    /// </summary>
    internal class TimeSpanFormatter : StructFormatter<TimeSpan>
    {
        public override int DefaultLength => 9;

        public TimeSpanFormatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override TimeSpan Deserialize(ref ByteBuffer buffer)
        {
            return new TimeSpan(_bufferConvert.ReadInt64(ref buffer));
        }

        public override void Serialize(ref ByteBuffer buffer, TimeSpan value)
        {
            _bufferConvert.WriteInt64(ref buffer, value.Ticks);
        }

        public override long GetLength(TimeSpan value)
        {
            return _bufferConvert.GetByteCount(value.Ticks);
        }
    }
}
