using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// UriFormatter 类，继承自 ResolverFormatter<Uri> 类。
    /// 用于格式化 Uri 类型的对象。
    /// </summary>
    internal class UriFormatter : ResolverFormatter<Uri>
    {
        private readonly IBinaryFormatter<string> _formatter;

        public UriFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _formatter = GetFormatter<string>();
        }

        public override int DefaultLength => _formatter.DefaultLength;

        public override Uri Deserialize(ref ExtenderBinaryReader reader)
        {
            //if(_binaryReaderConvert.TryReadNil(ref reader))
            //{
            //    return null;
            //}

            //_binaryReaderConvert.TryReadStringSpan(ref reader, out ReadOnlySpan<byte> bytes);
            //var value = _binaryReaderConvert.UTF8ToString(bytes);
            //return new Uri(value);

            var result = _formatter.Deserialize(ref reader);
            return new Uri(result);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, Uri value)
        {
            //if(value == null)
            //{
            //    _binaryWriterConvert.WriteNil(ref writer);
            //}
            //else
            //{
            //    _binaryWriterConvert.Write(ref writer, value.ToString());
            //}

            _formatter.Serialize(ref writer, value.ToString());
        }

        public override long GetLength(Uri value)
        {
            if(value == null)
            {
                return 1;
            }

            return _formatter.GetLength(value.ToString());
        }
    }
}
