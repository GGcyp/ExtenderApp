using System.Collections.ObjectModel;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary;
using ExtenderApp.Common.IO.Binary.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    /// <summary>
    /// ObservableCollectionFormatter 类是对 CollectionFormatter 类的一个泛型扩展，用于处理 ObservableCollection<T> 类型的集合格式化。
    /// </summary>
    /// <typeparam name="T">ObservableCollection<T> 中元素的类型。</typeparam>
    public class ObservableCollectionFormatter<T> : CollectionFormatter<T, ObservableCollection<T>>
    {
        public ObservableCollectionFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        protected override void Add(ObservableCollection<T> collection, T value)
        {
            collection.Add(value);
        }

        protected override ObservableCollection<T> Create(int count)
        {
            return new ObservableCollection<T>();
        }
    }
}
