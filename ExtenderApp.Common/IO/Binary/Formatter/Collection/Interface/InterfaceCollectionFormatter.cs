using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter.Collection
{
    /// <summary>
    /// InterfaceCollectionFormatter 类是对 CollectionFormatter 类的一个扩展，用于处理实现了 ICollection<T> 接口的集合类型。
    /// </summary>
    /// <typeparam name="T">集合中元素的类型。</typeparam>
    internal class InterfaceCollectionFormatter<T> : CollectionFormatter<T, ICollection<T>>
    {
        public InterfaceCollectionFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        protected override void Add(ICollection<T> collection, T value)
        {
            collection.Add(value);
        }

        protected override ICollection<T> Create(int count)
        {
            return new List<T>(count);
        }
    }
}
