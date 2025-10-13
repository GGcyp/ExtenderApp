using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    internal class MemoryFormatter<T> : BinaryFormatter<Memory<T>>
    {
        private readonly IBinaryFormatter<T> _formatter;
        public override int DefaultLength => 1;
        public MemoryFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        {
            _formatter = resolver.GetFormatter<T>();
        }

        public override Memory<T> Deserialize(ref ByteBuffer buffer)
        {
            if (_bufferConvert.TryReadNil(ref buffer))
            {
                return Memory<T>.Empty;
            }

            var length = _bufferConvert.ReadArrayHeader(ref buffer);
            if (length == 0)
            {
                return Memory<T>.Empty;
            }
            var array = new T[length];
            for (var i = 0; i < length; i++)
            {
                array[i] = _formatter.Deserialize(ref buffer);
            }
            return new Memory<T>(array);
        }

        public override void Serialize(ref ByteBuffer buffer, Memory<T> value)
        {
            if (value.IsEmpty)
            {
                _bufferConvert.WriteNil(ref buffer);
                return;
            }

            _bufferConvert.WriteArrayHeader(ref buffer, value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                _formatter.Serialize(ref buffer, value.Span[i]);
            }
        }

        public override long GetLength(Memory<T> value)
        {
            if (value.IsEmpty)
                return 1;

            long length = 5;
            for (var i = 0; i < value.Length; i++)
            {
                length += _formatter.GetLength(value.Span[i]);
            }

            return length;
        }
    }
}
