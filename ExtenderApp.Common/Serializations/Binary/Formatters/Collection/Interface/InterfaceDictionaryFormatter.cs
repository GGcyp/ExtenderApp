using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 一个抽象类，继承自 <see cref="BinaryFormatter{IDictionary{TKey, TValue}?}"/>，用于格式化键值对集合。
    /// </summary>
    /// <typeparam name="TKey">键的类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
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

        public override TDictionary? Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return default(TDictionary);
            }

            if (!TryReadMapHeader(ref buffer))
            {
                ThrowOperationException("数据不是映射类型。");
            }

            WriteMapHeader(ref buffer);
            int count = _int.Deserialize(ref buffer);
            TDictionary dict = Create(count);
            for (int i = 0; i < count; i++)
            {
                var key = _key.Deserialize(ref buffer);
                var value = _value.Deserialize(ref buffer);
                dict.Add(key, value);
            }

            return dict;
        }

        public override void Serialize(ref ByteBuffer buffer, TDictionary? value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
                return;
            }

            WriteMapHeader(ref buffer);
            _int.Serialize(ref buffer, value.Count);
            foreach (var kvp in value)
            {
                _key.Serialize(ref buffer, kvp.Key);
                _value.Serialize(ref buffer, kvp.Value);
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