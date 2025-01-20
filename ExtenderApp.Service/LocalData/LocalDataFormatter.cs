using ExtenderApp.Abstract;
using ExtenderApp.Common.File.Binary;
using ExtenderApp.Common.File.Binary.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Service
{
    internal class LocalDataFormatter<T> : ExtenderFormatter<LocalData<T>>
    {
        private IBinaryFormatter<T> _binaryFormatter;
        private IBinaryFormatter<Version> _versionFormatter;

        public override LocalData<T> Default => new LocalData<T>(_binaryFormatter.Default, _versionFormatter.Default);

        public LocalDataFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
            _binaryFormatter = resolver.GetFormatter<T>();
            _versionFormatter = resolver.GetFormatter<Version>();
        }

        public override LocalData<T> Deserialize(ref ExtenderBinaryReader reader)
        {
            //if (_binaryReaderConvert.TryReadNil(ref reader))
            //{
            //    return null;
            //}

            var version = _versionFormatter.Deserialize(ref reader);

            var data = _binaryFormatter.Deserialize(ref reader);

            return new LocalData<T>(data, version);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, LocalData<T> value)
        {
            if (value == null)
            {
                _binaryWriterConvert.WriteNil(ref writer);
                return;
            }

            _versionFormatter.Serialize(ref writer, value.Version);

            _binaryFormatter.Serialize(ref writer, value.Data);
        }
    }
}
