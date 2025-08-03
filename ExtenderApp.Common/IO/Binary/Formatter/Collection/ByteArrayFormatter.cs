

using System.Buffers;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter.Collection
{
    internal class ByteArrayFormatter : ExtenderFormatter<byte[]>
    {
        public ByteArrayFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override int DefaultLength => 5;

        public override byte[] Deserialize(ref ExtenderBinaryReader reader)
        {
            if (_binaryReaderConvert.TryReadNil(ref reader))
            {
                return Array.Empty<byte>();
            }

            var len = _binaryReaderConvert.ReadArrayHeader(ref reader);
            if (len == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] array = Array.Empty<byte>();
            DepthStep(ref reader);
            try
            {
                reader.CancellationToken.ThrowIfCancellationRequested();
                array = _binaryReaderConvert.ReadRaw(ref reader, len).ToArray();
            }
            finally
            {
                reader.Depth--;
            }

            return array;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, byte[] value)
        {
            if (value == null)
            {
                _binaryWriterConvert.WriteNil(ref writer);
            }
            else
            {
                _binaryWriterConvert.WriteArrayHeader(ref writer, value.Length);
                _binaryWriterConvert.Write(ref writer, value);
            }
        }

        public override long GetLength(byte[] value)
        {
            if (value == null)
            {
                return 1;
            }

            var result = DefaultLength;
            result += value.Length;
            return result;
        }
    }
}
