using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 静态可空格式化器类，用于处理可空的结构体类型的序列化和反序列化。
    /// </summary>
    /// <typeparam name="T">结构体类型</typeparam>
    public sealed class NullableFormatter<T> : ResolverFormatter<T?>
        where T : struct
    {
        /// <summary>
        /// 底层格式化器，用于处理非可空结构体类型的序列化和反序列化。
        /// </summary>
        private readonly IBinaryFormatter<T> _formatter;

        public NullableFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _formatter = GetFormatter<T>();
        }

        public override int DefaultLength => _formatter.DefaultLength;

        public override T? Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return null;
            }
            return _formatter.Deserialize(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, T? value)
        {
            if (value.HasValue)
            {
                _formatter.Serialize(ref buffer, value.Value);
            }
            else
            {
                WriteNil(ref buffer);
            }
        }

        public override long GetLength(T? value)
        {
            return value.HasValue ? _formatter.GetLength(value.Value) : 1;
        }
    }
}