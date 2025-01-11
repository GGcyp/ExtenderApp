using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    /// <summary>
    /// UriFormatter 类，继承自 ExtenderFormatter<Uri> 类。
    /// 用于格式化 Uri 类型的对象。
    /// </summary>
    internal class UriFormatter : ExtenderFormatter<Uri>
    {
        public UriFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override Uri Deserialize(ref ExtenderBinaryReader reader)
        {
            if(_binaryReaderConvert.TryReadNil(ref reader))
            {
                return null;
            }

            _binaryReaderConvert.TryReadStringSpan(ref reader, out ReadOnlySpan<byte> bytes);
            var value = _binaryReaderConvert.UTF8ToString(bytes);
            return new Uri(value);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, Uri value)
        {
            if(value == null)
            {
                _binaryWriterConvert.WriteNil(ref writer);
            }
            else
            {
                _binaryWriterConvert.Write(ref writer, value.ToString());
            }
        }
    }
}
