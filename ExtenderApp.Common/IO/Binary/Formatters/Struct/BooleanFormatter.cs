using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// BooleanFormatter 类，继承自 StructFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class BooleanFormatter : StructFormatter<Boolean>
    {
        public override int DefaultLength => 1;

        public BooleanFormatter(ByteBufferConvert bufferConvert, BinaryOptions options) : base(bufferConvert, options)
        {
        }

        public override bool Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadBoolean(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, bool value)
        {
            _bufferConvert.Write(ref buffer, value);
        }
    }
}
