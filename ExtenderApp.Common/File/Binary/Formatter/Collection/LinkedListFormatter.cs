using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    internal class LinkedListFormatter<T> : CollectionFormatter<T, LinkedList<T>>
    {
        public LinkedListFormatter(IBinaryFormatter<T> formatter, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(formatter, binaryWriterConvert, binaryReaderConvert, options)
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
