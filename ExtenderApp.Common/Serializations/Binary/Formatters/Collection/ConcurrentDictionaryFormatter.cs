using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary;
using ExtenderApp.Common.Serializations.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters.Collection
{
    internal class ConcurrentDictionaryFormatter<TKey, TValue> : InterfaceDictionaryFormatter<TKey, TValue, ConcurrentDictionary<TKey, TValue>> where TKey : notnull
    {
        public ConcurrentDictionaryFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(resolver, convert, options)
        {
        }

        protected override ConcurrentDictionary<TKey, TValue> Create(int count)
        {
            return new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, count);
        }
    }
}
