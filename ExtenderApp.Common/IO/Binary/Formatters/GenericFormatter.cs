using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 一个通用的二进制格式化器，用于依赖注入中的二进制格式化器生成。
    /// </summary>
    /// <typeparam name="T">需要被二进制格式化的类型。</typeparam>
    internal class GenericFormatter<T> : ResolverFormatter<T>
    {
        private IBinaryFormatter<T> _innerFormatter;

        public GenericFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _innerFormatter = resolver.GetFormatter<T>();
        }

        public override int DefaultLength => _innerFormatter.DefaultLength;

        public override T Deserialize(ref ByteBuffer buffer)
        {
            return _innerFormatter.Deserialize(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, T value)
        {
            _innerFormatter.Serialize(ref buffer, value);
        }

        public override long GetLength(T value)
        {
            return _innerFormatter.GetLength(value);
        }
    }
}