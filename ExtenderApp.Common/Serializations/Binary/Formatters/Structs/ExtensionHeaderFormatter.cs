using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class ExtensionHeaderFormatter : StructFormatter<ExtensionHeader>
    {
        public override int DefaultLength => 5;

        public ExtensionHeaderFormatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
        }

        public override ExtensionHeader Deserialize(ref ByteBuffer buffer)
        {
            _bufferConvert.TryExtensionHeader(ref buffer, out var header);
            return header;
        }

        public override void Serialize(ref ByteBuffer buffer, ExtensionHeader value)
        {
            _bufferConvert.WriteExtensionFormatHeader(ref buffer, value);
        }

        public override long GetLength(ExtensionHeader value)
        {
            return _bufferConvert.GetByteCountExtensionFormatHeader(value);
        }
    }
}
