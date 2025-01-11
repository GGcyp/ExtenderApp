using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    /// <summary>
    /// 一个内部密封类，继承自 <see cref="ExtenderFormatter{List<T>}"/>，用于格式化 List<T> 类型的对象。
    /// </summary>
    /// <typeparam name="T">List 中元素的类型。</typeparam>
    internal sealed class ListFormatter<T> : InterfaceListFormatter<T, List<T>>
    {
        public ListFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
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
