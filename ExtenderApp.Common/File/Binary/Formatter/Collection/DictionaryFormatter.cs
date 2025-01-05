using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    /// <summary>
    /// DictionaryFormatter 类，用于实现 InterfaceDictionaryFormatter 接口，提供对字典数据的格式化功能。
    /// </summary>
    /// <typeparam name="TKey">字典键的类型。</typeparam>
    /// <typeparam name="TValue">字典值的类型。</typeparam>
    public class DictionaryFormatter<TKey, TValue> : InterfaceDictionaryFormatter<TKey, TValue, Dictionary<TKey, TValue>>
    {
        public DictionaryFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
        {

        }

        protected override Dictionary<TKey, TValue> Create(int count)
        {
            return new Dictionary<TKey, TValue>(count);
        }
    }
}
