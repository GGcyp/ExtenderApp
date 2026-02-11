using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 一个抽象类，继承自 <see cref="BinaryFormatter{T}"/>，用于格式化键值对集合。
    /// </summary>
    /// <typeparam name="TKey">键的类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    /// <typeparam name="TDictionary">具体的字典类型。</typeparam>
    public abstract class InterfaceDictionaryFormatter<TKey, TValue, TDictionary> : ResolverFormatter<TDictionary?> where TDictionary : IDictionary<TKey, TValue>
    {
        private readonly IBinaryFormatter<TKey> _key;
        private readonly IBinaryFormatter<TValue> _value;
        private readonly IBinaryFormatter<int> _int;

        protected InterfaceDictionaryFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _key = GetFormatter<TKey>();
            _value = GetFormatter<TValue>();
            _int = GetFormatter<int>();
        }

        public override TDictionary? Deserialize(AbstractBufferReader<byte> reader)
        {
            if (TryReadNil(reader))
            {
                return default;
            }

            if (!TryReadMapHeader(reader))
            {
                ThrowOperationException("数据不是映射类型。");
            }

            int count = _int.Deserialize(reader);
            TDictionary dict = Create(count);
            for (int i = 0; i < count; i++)
            {
                var key = _key.Deserialize(reader);
                var value = _value.Deserialize(reader);
                dict.Add(key, value);
            }

            return dict;
        }

        public override TDictionary? Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return default;
            }

            if (!TryReadMapHeader(ref reader))
            {
                ThrowOperationException("数据不是映射类型。");
            }

            int count = _int.Deserialize(ref reader);
            TDictionary dict = Create(count);
            for (int i = 0; i < count; i++)
            {
                var key = _key.Deserialize(ref reader);
                var value = _value.Deserialize(ref reader);
                dict.Add(key, value);
            }

            return dict;
        }

        public override void Serialize(AbstractBuffer<byte> buffer, TDictionary? value)
        {
            if (value == null)
            {
                WriteNil(buffer);
                return;
            }

            WriteMapHeader(buffer);
            _int.Serialize(buffer, value.Count);
            foreach (var kvp in value)
            {
                _key.Serialize(buffer, kvp.Key);
                _value.Serialize(buffer, kvp.Value);
            }
        }

        public override void Serialize(ref SpanWriter<byte> writer, TDictionary? value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            WriteMapHeader(ref writer);
            _int.Serialize(ref writer, value.Count);
            foreach (var kvp in value)
            {
                _key.Serialize(ref writer, kvp.Key);
                _value.Serialize(ref writer, kvp.Value);
            }
        }

        public override long GetLength(TDictionary? value)
        {
            if (value == null)
            {
                return NilLength;
            }

            var result = _int.GetLength(value.Count) + 1;
            foreach (var kvp in value)
            {
                result += _key.GetLength(kvp.Key);
                result += _value.GetLength(kvp.Value);
            }
            return result;
        }

        protected abstract TDictionary Create(int count);
    }
}