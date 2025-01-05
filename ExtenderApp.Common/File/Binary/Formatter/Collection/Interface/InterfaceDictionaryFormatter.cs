using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    /// <summary>
    /// 一个抽象类，继承自 <see cref="ExtenderFormatter{IDictionary{TKey, TValue}?}"/>，用于格式化键值对集合。
    /// </summary>
    /// <typeparam name="TKey">键的类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    public abstract class InterfaceDictionaryFormatter<TKey, TValue, TDictionary> : ExtenderFormatter<TDictionary?> where TDictionary : IDictionary<TKey, TValue>
    {
        private readonly IBinaryFormatter<TKey> _keyFormatter;
        private readonly IBinaryFormatter<TValue> _valueFormatter;

        public InterfaceDictionaryFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
            _keyFormatter = resolver.GetFormatterWithVerify<TKey>();
            _valueFormatter = resolver.GetFormatterWithVerify<TValue>();
        }

        public override TDictionary? Deserialize(ref ExtenderBinaryReader reader)
        {
            if (_binaryReaderConvert.TryReadNil(ref reader))
            {
                return default(TDictionary);
            }

            int count = _binaryReaderConvert.ReadMapHeader(ref reader);
            TDictionary dict = Create(count);
            for (int i = 0; i < count; i++)
            {
                var key = _keyFormatter.Deserialize(ref reader);
                var value = _valueFormatter.Deserialize(ref reader);
                dict.Add(key, value);
            }

            return dict;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, TDictionary? value)
        {
            if (value == null)
            {
                _binaryWriterConvert.WriteNil(ref writer);
                return;
            }

            _binaryWriterConvert.WriteMapHeader(ref writer, value.Count);
            foreach (var kvp in value)
            {
                _keyFormatter.Serialize(ref writer, kvp.Key);
                _valueFormatter.Serialize(ref writer, kvp.Value);
            }
        }

        protected abstract TDictionary Create(int count);
    }
}
