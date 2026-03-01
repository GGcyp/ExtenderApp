using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 值或列表格式化器：一个特殊的集合格式化器，支持单个值或值列表的序列化和反序列化。 当集合中只有一个元素时，可以直接序列化该元素而不是列表，从而节省空间和提高性能。 适用于那些通常只包含一个值但偶尔需要多个值的场景。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ValueOrListFormatter<T> : InterfaceCollectionFormatter<T, ValueOrList<T>>
    {
        public ValueOrListFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override sealed void Add(ValueOrList<T> collection, T value)
        {
            collection.Add(value);
        }

        protected override sealed ValueOrList<T> Create(int count)
        {
            return new ValueOrList<T>(count);
        }
    }
}