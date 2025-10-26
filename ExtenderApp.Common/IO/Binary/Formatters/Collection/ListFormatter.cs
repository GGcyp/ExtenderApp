using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 一个内部密封类，继承自 <see cref="BinaryFormatter{List<TLinkClient>}"/>，用于格式化 List<TLinkClient> 类型的对象。
    /// </summary>
    /// <typeparam name="T">List 中元素的类型。</typeparam>
    internal sealed class ListFormatter<T> : InterfaceListFormatter<T, List<T>>
    {
        public ListFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(resolver, convert, options)
        {

        }

        protected override void Add(List<T> collection, T value)
        {
            collection.Add(value);
        }

        protected override List<T> Create(int count)
        {
            return new List<T>(count);
        }
    }
}
