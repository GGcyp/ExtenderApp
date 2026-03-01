using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal sealed class LinkedListFormatter<T> : InterfaceCollectionFormatter<T, LinkedList<T>>
    {
        public LinkedListFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override sealed void Add(LinkedList<T> collection, T value)
        {
            collection.AddLast(value);
        }

        protected override sealed LinkedList<T> Create(int count)
        {
            return new LinkedList<T>();
        }
    }
}