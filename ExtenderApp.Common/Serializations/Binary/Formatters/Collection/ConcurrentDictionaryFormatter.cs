using System.Collections.Concurrent;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Serializations.Binary.Formatters.Collection
{
    internal class ConcurrentDictionaryFormatter<TKey, TValue> : InterfaceDictionaryFormatter<TKey, TValue, ConcurrentDictionary<TKey, TValue>> where TKey : notnull
    {
        public ConcurrentDictionaryFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override ConcurrentDictionary<TKey, TValue> Create(int count)
        {
            return new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, count);
        }
    }
}