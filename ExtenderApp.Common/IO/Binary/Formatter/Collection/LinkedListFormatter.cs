using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    internal class LinkedListFormatter<T> : CollectionFormatter<T, LinkedList<T>>
    {
        public LinkedListFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        protected override void Add(LinkedList<T> collection, T value)
        {
            collection.AddLast(value);
        }

        protected override LinkedList<T> Create(int count)
        {
            return new LinkedList<T>();
        }
    }
}
