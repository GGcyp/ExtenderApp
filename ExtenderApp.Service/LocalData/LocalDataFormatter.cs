using ExtenderApp.Abstract;
using ExtenderApp.Common.Files.Binary;
using ExtenderApp.Common.Files.Binary.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Service
{
    internal class LocalDataFormatter<T> : ResolverFormatter<LocalData<T>>
    {
        private IBinaryFormatter<T> _binaryFormatter;
        private IBinaryFormatter<Version> _versionFormatter;

        public override LocalData<T> Default => new LocalData<T>(_binaryFormatter.Default, _versionFormatter.Default);

        public override int Count => _binaryFormatter.Count + _versionFormatter.Count;

        public LocalDataFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _binaryFormatter = GetFormatter<T>();
            _versionFormatter = GetFormatter<Version>();
        }

        public override LocalData<T> Deserialize(ref ExtenderBinaryReader reader)
        {
            var version = _versionFormatter.Deserialize(ref reader);

            var data = _binaryFormatter.Deserialize(ref reader);

            return new LocalData<T>(data, version);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, LocalData<T> value)
        {
            _versionFormatter.Serialize(ref writer, value.Version);

            _binaryFormatter.Serialize(ref writer, value.Data);
        }

        public override int GetCount(LocalData<T> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return _versionFormatter.GetCount(value.Version) + _binaryFormatter.GetCount(value.Data);
        }
    }
}
