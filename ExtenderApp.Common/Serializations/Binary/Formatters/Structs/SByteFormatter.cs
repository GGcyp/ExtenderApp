using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// SByteFormatter 类，继承自 StructFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class SByteFormatter : StructFormatter<SByte>
    {
        public override int DefaultLength => 2;

        public SByteFormatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override sbyte Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadSByte(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, sbyte value)
        {
            _bufferConvert.Write(ref buffer, value);
        }

        public override long GetLength(sbyte value)
        {
            return _bufferConvert.GetByteCount(value);
        }
    }
}
