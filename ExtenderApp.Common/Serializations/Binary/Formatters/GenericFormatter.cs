using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
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

        public override void Serialize(AbstractBuffer<byte> buffer, T value)
        {
            _innerFormatter.Serialize(buffer, value);
        }

        public override void Serialize(ref SpanWriter<byte> writer, T value)
        {
            _innerFormatter.Serialize(ref writer, value);
        }

        public override T Deserialize(AbstractBufferReader<byte> reader)
        {
            return _innerFormatter.Deserialize(reader);
        }

        public override T Deserialize(ref SpanReader<byte> reader)
        {
            return _innerFormatter.Deserialize(ref reader);
        }

        public override long GetLength(T value)
        {
            return _innerFormatter.GetLength(value);
        }
    }
}