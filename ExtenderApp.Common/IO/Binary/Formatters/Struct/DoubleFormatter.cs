using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// DoubleFormatter 类，继承自 StructFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class DoubleFormatter : StructFormatter<Double>
    {
        public DoubleFormatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override double Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadDouble(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, double value)
        {
            _bufferConvert.Write(ref buffer, value);
        }
    }
}
