using ExtenderApp.Abstract;
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

        public override Result Deserialize(ref ByteBuffer buffer)
        {
            var code = _bool.Deserialize(ref buffer);
            var messenge = _string.Deserialize(ref buffer);
            return new Result(code, messenge);
        }

        public override void Serialize(ref ByteBuffer buffer, Result value)
        {
            _bool.Serialize(ref buffer, value);
            _string.Serialize(ref buffer, value!);
        }

        public override long GetLength(Result value)
        {
            return _bool.GetLength(value) + _string.GetLength(value!);
        }
    }

    /// <summary>
    /// 
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

        public override Result<T> Deserialize(ref ByteBuffer buffer)
        {
            var code = _bool.Deserialize(ref buffer);
            var messenge = _string.Deserialize(ref buffer);
            var value = _value.Deserialize(ref buffer);
            return new Result<T>(code, value, messenge);
        }

        public override void Serialize(ref ByteBuffer buffer, Result<T> value)
        {
            _bool.Serialize(ref buffer, value);
            _string.Serialize(ref buffer, value!);
            _value.Serialize(ref buffer, value!);
        }

        public override long GetLength(Result<T> value)
        {
            return _bool.GetLength(value) + _string.GetLength(value!) + _value.GetLength(value!);
        }
    }
}