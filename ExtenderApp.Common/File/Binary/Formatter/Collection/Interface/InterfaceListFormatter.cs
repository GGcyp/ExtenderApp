using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Files.Binary.Formatter
{
    /// <summary>
    /// 泛型接口列表格式化器类
    /// </summary>
    /// <typeparam name="T">列表中元素的类型</typeparam>
    /// <typeparam name="TList">列表的类型，必须实现IList<T>接口并且有一个无参构造函数</typeparam>
    public class InterfaceListFormatter<T, TList> : CollectionFormatter<T, TList> where TList : class, IList<T>, new()
    {
        private readonly CollectionHelpers<TList> _helpers;

        public InterfaceListFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
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
