using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary;
using ExtenderApp.Common.Serializations.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class ValueOrListFormatter<T> : CollectionFormatter<T, ValueOrList<T>>
    {
        public ValueOrListFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(resolver, convert, options)
        {
        }

        protected override void Add(ValueOrList<T> collection, T value)
        {
            collection.Add(value);
        }

        protected override ValueOrList<T> Create(int count)
        {
            return new ValueOrList<T>(count);
        }
    }
}
