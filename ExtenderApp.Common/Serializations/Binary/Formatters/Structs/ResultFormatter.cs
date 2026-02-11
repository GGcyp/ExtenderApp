using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 操作结果格式转换器
    /// </summary>
    internal class ResultFormatter : ResolverFormatter<Result>
    {
        private readonly IBinaryFormatter<bool> _bool;
        private readonly IBinaryFormatter<string> _string;

        public ResultFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _bool = GetFormatter<bool>();
            _string = GetFormatter<string>();
        }

        public override void Serialize(AbstractBuffer<byte> buffer, Result value)
        {
            _bool.Serialize(buffer, value);
            _string.Serialize(buffer, value!);
        }

        public override void Serialize(ref SpanWriter<byte> writer, Result value)
        {
            _bool.Serialize(ref writer, value);
            _string.Serialize(ref writer, value!);
        }

        public override Result Deserialize(AbstractBufferReader<byte> reader)
        {
            var code = _bool.Deserialize(reader);
            var messenge = _string.Deserialize(reader);
            return new Result(code, messenge);
        }

        public override Result Deserialize(ref SpanReader<byte> reader)
        {
            var code = _bool.Deserialize(ref reader);
            var messenge = _string.Deserialize(ref reader);
            return new Result(code, messenge);
        }

        public override long GetLength(Result value)
        {
            return _bool.GetLength(value) + _string.GetLength(value!);
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ResultFormatter<T> : ResolverFormatter<Result<T>>
    {
        private readonly IBinaryFormatter<bool> _bool;
        private readonly IBinaryFormatter<string> _string;
        private readonly IBinaryFormatter<T> _value;

        public ResultFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _bool = GetFormatter<bool>();
            _string = GetFormatter<string>();
            _value = GetFormatter<T>();
        }

        public override void Serialize(AbstractBuffer<byte> buffer, Result<T> value)
        {
            _bool.Serialize(buffer, value);
            _string.Serialize(buffer, value!);
            _value.Serialize(buffer, value!);
        }

        public override void Serialize(ref SpanWriter<byte> writer, Result<T> value)
        {
            _bool.Serialize(ref writer, value);
            _string.Serialize(ref writer, value!);
            _value.Serialize(ref writer, value!);
        }

        public override Result<T> Deserialize(AbstractBufferReader<byte> reader)
        {
            var code = _bool.Deserialize(reader);
            var messenge = _string.Deserialize(reader);
            var value = _value.Deserialize(reader);
            return new Result<T>(code, value, messenge);
        }

        public override Result<T> Deserialize(ref SpanReader<byte> reader)
        {
            var code = _bool.Deserialize(ref reader);
            var messenge = _string.Deserialize(ref reader);
            var value = _value.Deserialize(ref reader);
            return new Result<T>(code, value, messenge);
        }

        public override long GetLength(Result<T> value)
        {
            return _bool.GetLength(value) + _string.GetLength(value!) + _value.GetLength(value!);
        }
    }
}