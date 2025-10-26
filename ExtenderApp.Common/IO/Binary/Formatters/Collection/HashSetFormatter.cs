using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 表示一个内部类 HashSetFormatter，它是 CollectionFormatter 的泛型子类，用于格式化 HashSet<TLinkClient> 集合。
    /// </summary>
    /// <typeparam name="T">HashSet<TLinkClient> 中元素的类型。</typeparam>
    internal class HashSetFormatter<T> : CollectionFormatter<T, HashSet<T>>
    {
        public HashSetFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(resolver, convert, options)
        {
        }

        protected override void Add(HashSet<T> collection, T value)
        {
            collection.Add(value);
        }

        protected override HashSet<T> Create(int count)
        {
            return new HashSet<T>(count);
        }
    }
}
