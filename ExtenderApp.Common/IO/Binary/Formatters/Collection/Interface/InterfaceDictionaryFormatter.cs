using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 一个抽象类，继承自 <see cref="BinaryFormatter{IDictionary{TKey, TValue}?}"/>，用于格式化键值对集合。
    /// </summary>
    /// <typeparam name="TKey">键的类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    public abstract class InterfaceDictionaryFormatter<TKey, TValue, TDictionary> : BinaryFormatter<TDictionary?> where TDictionary : IDictionary<TKey, TValue>
    {
        private readonly IBinaryFormatter<TKey> _keyFormatter;
        private readonly IBinaryFormatter<TValue> _valueFormatter;
        public override int DefaultLength => 1;

        protected InterfaceDictionaryFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
            _keyFormatter = resolver.GetFormatterWithVerify<TKey>();
            _valueFormatter = resolver.GetFormatterWithVerify<TValue>();
        }

        public override TDictionary? Deserialize(ref ByteBuffer buffer)
        {
            if (_bufferConvert.TryReadNil(ref buffer))
            {
                return default(TDictionary);
            }

            int count = _bufferConvert.ReadMapHeader(ref buffer);
            TDictionary dict = Create(count);
            for (int i = 0; i < count; i++)
            {
                var key = _keyFormatter.Deserialize(ref buffer);
                var value = _valueFormatter.Deserialize(ref buffer);
                dict.Add(key, value);
            }

            return dict;
        }

        public override void Serialize(ref ByteBuffer buffer, TDictionary? value)
        {
            if (value == null)
            {
                _bufferConvert.WriteNil(ref buffer);
                return;
            }

            _bufferConvert.WriteMapHeader(ref buffer, value.Count);
            foreach (var kvp in value)
            {
                _keyFormatter.Serialize(ref buffer, kvp.Key);
                _valueFormatter.Serialize(ref buffer, kvp.Value);
            }
        }

        public override long GetLength(TDictionary value)
        {
            if (value == null)
            {
                return 1;
            }

            var result = DefaultLength;
            result += (_keyFormatter.DefaultLength + _valueFormatter.DefaultLength) * value.Count;
            return result;
        }

        protected abstract TDictionary Create(int count);
    }
}