using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Files.Binary.Formatter
{
    internal class CustomizeDictionaryFormatter<TKey, TValue, TDictionary> : InterfaceDictionaryFormatter<TKey, TValue, TDictionary> where TDictionary : IDictionary<TKey, TValue>, new()
    {
        private CollectionHelpers<TDictionary> _collectionHelpers;

        public CustomizeDictionaryFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
        {
            _collectionHelpers = new CollectionHelpers<TDictionary>();
        }

        protected override TDictionary Create(int count)
        {
            return _collectionHelpers.CreateCollection(count);
        }
    }
}
