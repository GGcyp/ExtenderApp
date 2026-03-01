using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 操作结果格式转换器
    /// </summary>
    internal sealed class ResultFormatter : ResolverFormatter<Result>
    {
        private readonly IBinaryFormatter<bool> _bool;
        private readonly IBinaryFormatter<string> _string;
        private readonly byte _mark;

        public ResultFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _bool = GetFormatter<bool>();
            _string = GetFormatter<string>();
            _mark = BinaryOptions.Ex4;
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, Result value)
        {
            WriteMark(ref writer, _mark);
            _bool.Serialize(ref writer, value);

            bool isDefaultOrEmptyMessage = string.IsNullOrEmpty(value) || CheckDefaultMessage(value);
            _bool.Serialize(ref writer, isDefaultOrEmptyMessage);
            if (!isDefaultOrEmptyMessage)
            {
                _string.Serialize(ref writer, value!);
            }
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, Result value)
        {
            WriteMark(ref writer, _mark);
            _bool.Serialize(ref writer, value);

            bool isDefaultOrEmptyMessage = string.IsNullOrEmpty(value) || CheckDefaultMessage(value);
            _bool.Serialize(ref writer, isDefaultOrEmptyMessage);
            if (!isDefaultOrEmptyMessage)
            {
                _string.Serialize(ref writer, value!);
            }
        }

        public override sealed Result Deserialize(ref BinaryReaderAdapter reader)
        {
            if (!TryReadMark(ref reader, _mark))
            {
                throw new InvalidOperationException("序列化格式错误");
            }

            var code = _bool.Deserialize(ref reader);

            return _bool.Deserialize(ref reader) ?
                new(code, GetDefaultMessage(code)) :
                new(code, _string.Deserialize(ref reader));
        }

        public override sealed Result Deserialize(ref SpanReader<byte> reader)
        {
            if (!TryReadMark(ref reader, _mark))
            {
                throw new InvalidOperationException("序列化格式错误");
            }

            var code = _bool.Deserialize(ref reader);

            return _bool.Deserialize(ref reader) ?
                new(code, GetDefaultMessage(code)) :
                new(code, _string.Deserialize(ref reader));
        }

        public override sealed long GetLength(Result value)
        {
            return _bool.GetLength(value) + _string.GetLength(value!);
        }

        internal static bool CheckDefaultMessage(Result result)
        {
            string? message = result.Message;
            return string.Equals(message, Result.DefaultSuccessMessage, StringComparison.Ordinal) ||
                   string.Equals(message, Result.DefaultFailureMessage, StringComparison.Ordinal);
        }

        internal static string GetDefaultMessage(bool code)
        {
            return code ? Result.DefaultSuccessMessage : Result.DefaultFailureMessage;
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ResultFormatter<T> : ResolverFormatter<Result<T>>
    {
        private readonly IBinaryFormatter<bool> _bool;
        private readonly IBinaryFormatter<string> _string;
        private readonly IBinaryFormatter<T> _value;
        private readonly byte _mark;

        public ResultFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _bool = GetFormatter<bool>();
            _string = GetFormatter<string>();
            _value = GetFormatter<T>();
            _mark = BinaryOptions.Ex8;
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, Result<T> value)
        {
            WriteMark(ref writer, _mark);
            _bool.Serialize(ref writer, value);

            bool isDefaultOrEmptyMessage = string.IsNullOrEmpty(value) || ResultFormatter.CheckDefaultMessage(value);
            _bool.Serialize(ref writer, isDefaultOrEmptyMessage);
            if (!isDefaultOrEmptyMessage)
            {
                _string.Serialize(ref writer, value!);
            }
            _value.Serialize(ref writer, value!);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, Result<T> value)
        {
            WriteMark(ref writer, _mark);
            _bool.Serialize(ref writer, value);

            bool isDefaultOrEmptyMessage = string.IsNullOrEmpty(value) || ResultFormatter.CheckDefaultMessage(value);
            _bool.Serialize(ref writer, isDefaultOrEmptyMessage);
            if (!isDefaultOrEmptyMessage)
            {
                _string.Serialize(ref writer, value!);
            }
            _value.Serialize(ref writer, value!);
        }

        public override sealed Result<T> Deserialize(ref BinaryReaderAdapter reader)
        {
            if (!TryReadMark(ref reader, _mark))
            {
                throw new InvalidOperationException("序列化格式错误");
            }

            var code = _bool.Deserialize(ref reader);

            string message = _bool.Deserialize(ref reader) ? ResultFormatter.GetDefaultMessage(code) : _string.Deserialize(ref reader);
            var value = _value.Deserialize(ref reader);
            return new(code, value, message);
        }

        public override sealed Result<T> Deserialize(ref SpanReader<byte> reader)
        {
            if (!TryReadMark(ref reader, _mark))
            {
                throw new InvalidOperationException("序列化格式错误");
            }

            var code = _bool.Deserialize(ref reader);

            string message = _bool.Deserialize(ref reader) ? ResultFormatter.GetDefaultMessage(code) : _string.Deserialize(ref reader);
            var value = _value.Deserialize(ref reader);
            return new(code, value, message);
        }

        public override sealed long GetLength(Result<T> value)
        {
            return _bool.GetLength(value) + _string.GetLength(value!) + _value.GetLength(value!);
        }
    }
}