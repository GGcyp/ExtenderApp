using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// UInt64Formatter 类，继承自 StructFormatter<UInt64> 类，用于格式化 UInt64 类型的数据。
    /// </summary>
    internal sealed class UInt64Formatter : StructFormatter<UInt64>
    {
        public UInt64Formatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override ulong Deserialize(ref ByteBuffer buffer)
        {
            return _bufferConvert.ReadUInt64(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, ulong value)
        {
            _bufferConvert.Write(ref buffer, value);
        }

        public override long GetLength(ulong value)
        {
            return _bufferConvert.GetByteCount(value);
        }
    }
}
