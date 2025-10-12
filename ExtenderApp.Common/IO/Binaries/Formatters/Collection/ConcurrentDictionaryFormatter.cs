using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters.Collection
{
    internal class ConcurrentDictionaryFormatter<TKey, TValue> : InterfaceDictionaryFormatter<TKey, TValue, ConcurrentDictionary<TKey, TValue>> where TKey : notnull
    {
        public ConcurrentDictionaryFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        protected override ConcurrentDictionary<TKey, TValue> Create(int count)
        {
            return new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, count);
        }
    }
}
