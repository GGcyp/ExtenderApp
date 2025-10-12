using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// StackFormatter 类是 CollectionFormatter 类的泛型实现，用于格式化 Stack<T> 集合。
    /// </summary>
    /// <typeparam name="T">Stack<T> 集合中元素的类型。</typeparam>
    internal class StackFormatter<T> : CollectionFormatter<T, Stack<T>>
    {
        public StackFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
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
