using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    internal class ValueOrListFormatter<T> : CollectionFormatter<T, ValueOrList<T>>
    {
        public ValueOrListFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
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
