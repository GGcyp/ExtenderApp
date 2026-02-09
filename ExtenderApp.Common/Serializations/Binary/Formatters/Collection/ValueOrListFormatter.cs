using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class ValueOrListFormatter<T> : InterfaceCollectionFormatter<T, ValueOrList<T>>
    {
        public ValueOrListFormatter(IBinaryFormatterResolver resolver) : base(resolver)
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