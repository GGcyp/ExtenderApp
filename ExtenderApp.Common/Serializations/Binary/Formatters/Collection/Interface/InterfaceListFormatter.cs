using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 泛型接口列表格式化器类
    /// </summary>
    /// <typeparam name="T">列表中元素的类型</typeparam>
    /// <typeparam name="TList">列表的类型，必须实现IList<TLinkClient>接口并且有一个无参构造函数</typeparam>
    public class InterfaceListFormatter<T, TList> : CollectionFormatter<T, TList> where TList : class, IList<T>, new()
    {
        private readonly CollectionHelpers<TList> _helpers;

        public InterfaceListFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(resolver, convert, options)
        {
            _helpers = new CollectionHelpers<TList>();
        }

        protected override void Add(TList collection, T value)
        {
            collection.Add(value);
        }

        protected override TList Create(int count)
        {
            return _helpers.CreateCollection(count);
        }
    }
}
