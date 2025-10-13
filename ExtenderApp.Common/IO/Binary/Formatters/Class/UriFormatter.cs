using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
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

        public override Uri Deserialize(ref ByteBuffer buffer)
        {
            //if(_bufferConvert.TryReadNil(ref buffer))
            //{
            //    return null;
            //}

            //_bufferConvert.TryReadStringSpan(ref buffer, out ReadOnlySpan<byte> bytes);
            //var value = _bufferConvert.UTF8ToString(bytes);
            //return new Uri(value);

            var result = _formatter.Deserialize(ref buffer);
            return new Uri(result);
        }

        public override void Serialize(ref ByteBuffer buffer, Uri value)
        {
            //if(value == null)
            //{
            //    _bufferConvert.WriteNil(ref buffer);
            //}
            //else
            //{
            //    _bufferConvert.Write(ref buffer, value.ToString());
            //}

            _formatter.Serialize(ref buffer, value.ToString());
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
