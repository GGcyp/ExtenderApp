using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
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
            //if(_bufferConvert.TryReadNil(ref Block))
            //{
            //    return null;
            //}

            //_bufferConvert.TryReadStringSpan(ref Block, out ReadOnlySpan<byte> bytes);
            //var Value = _bufferConvert.UTF8ToString(bytes);
            //return new Uri(Value);

            var result = _formatter.Deserialize(ref buffer);
            return new Uri(result);
        }

        public override void Serialize(ref ByteBuffer buffer, Uri value)
        {
            //if(Value == null)
            //{
            //    _bufferConvert.WriteNil(ref Block);
            //}
            //else
            //{
            //    _bufferConvert.Write(ref Block, Value.ToString());
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
