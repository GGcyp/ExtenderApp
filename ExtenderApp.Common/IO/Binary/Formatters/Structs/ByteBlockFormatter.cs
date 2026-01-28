
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 字节块格式化器。
    /// </summary>
    internal class ByteBlockFormatter : BinaryFormatter<ByteBlock>
    {
        public override int DefaultLength => 1;
        public ByteBlockFormatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override ByteBlock Deserialize(ref ByteBuffer buffer)
        {
            if (_bufferConvert.TryReadNil(ref buffer))
            {
                return new();
            }
            var length = _bufferConvert.ReadArrayHeader(ref buffer);
            ByteBlock block = new(length);
            block.Write(buffer);
            buffer.ReadAdvance(length);
            return block;
        }

        public override void Serialize(ref ByteBuffer buffer, ByteBlock value)
        {
            if (value.IsEmpty)
            {
                _bufferConvert.WriteNil(ref buffer);
                return;
            }
            _bufferConvert.WriteMapHeader(ref buffer, value.Remaining);
            buffer.Write(value);
        }

        public override long GetLength(ByteBlock value)
        {
            if (value.IsEmpty)
            {
                return 1;
            }
            return _bufferConvert.GetByteCountMapHeader(value.Remaining) + value.Remaining;
        }
    }
}
