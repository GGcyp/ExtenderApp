using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatters
{
    internal class ReadOnlyMemoryFormatter<T> : ExtenderFormatter<ReadOnlyMemory<T>>
    {
        private readonly IBinaryFormatter<T> _formatter;

        public override int DefaultLength => 1;

        public ReadOnlyMemoryFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
            _formatter = resolver.GetFormatter<T>();
        }

        public override ReadOnlyMemory<T> Deserialize(ref ExtenderBinaryReader reader)
        {
            if (_binaryReaderConvert.TryReadNil(ref reader))
            {
                return Memory<T>.Empty;
            }
            DepthStep(ref reader);
            var length = _binaryReaderConvert.ReadArrayHeader(ref reader);
            if (length == 0)
            {
                return Memory<T>.Empty;
            }
            var array = new T[length];
            for (var i = 0; i < length; i++)
            {
                array[i] = _formatter.Deserialize(ref reader);
            }
            reader.Depth--;
            return new Memory<T>(array);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, ReadOnlyMemory<T> value)
        {
            if (value.IsEmpty)
            {
                _binaryWriterConvert.WriteNil(ref writer);
                return;
            }

            _binaryWriterConvert.WriteArrayHeader(ref writer, value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                _formatter.Serialize(ref writer, value.Span[i]);
            }
        }

        public override long GetLength(ReadOnlyMemory<T> value)
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
