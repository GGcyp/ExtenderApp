using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class LinkedListFormatter<T> : CollectionFormatter<T, LinkedList<T>>
    {
        public LinkedListFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(resolver, convert, options)
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
