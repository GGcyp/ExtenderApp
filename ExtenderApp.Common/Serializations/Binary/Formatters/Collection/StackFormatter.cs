using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// StackFormatter 类是 CollectionFormatter 类的泛型实现，用于格式化 Stack<TLinkClient> 集合。
    /// </summary>
    /// <typeparam name="T">Stack<TLinkClient> 集合中元素的类型。</typeparam>
    internal class StackFormatter<T> : InterfaceEnumerableFormatter<T, Stack<T>>
    {
        public StackFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override void Add(Stack<T> collection, T value)
        {
            collection.Push(value);
        }

        protected override Stack<T> Create(int count)
        {
            return new Stack<T>(count);
        }
    }
}