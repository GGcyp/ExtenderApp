using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Hash
{
    internal class HashValuesFormatter : ResolverFormatter<HashValues>
    {
        protected readonly IBinaryFormatter<ReadOnlyMemory<ulong>> _ulongs;
        protected readonly IBinaryFormatter<int> _int;

        public override int DefaultLength => _ulongs.DefaultLength + _int.DefaultLength;

        public HashValuesFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _ulongs = GetFormatter<ReadOnlyMemory<ulong>>();
            _int = GetFormatter<int>();
        }

        public override HashValues Deserialize(ref ExtenderBinaryReader reader)
        {
            var memory = _ulongs.Deserialize(ref reader);
            if (memory.IsEmpty)
            {
                return HashValues.Empty;
            }
            var hashLength = _int.Deserialize(ref reader);
            return new HashValues(memory, hashLength);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, HashValues value)
        {
            if (value.IsEmpty)
            {
                _ulongs.Serialize(ref writer, ReadOnlyMemory<ulong>.Empty);
                return;
            }

            _ulongs.Serialize(ref writer, value.ULongMemory);
            _int.Serialize(ref writer, value.HashLength);
        }

        public override long GetLength(HashValues value)
        {
            if (value.IsEmpty)
            {
                return 1;
            }
            return _ulongs.GetLength(value.ULongMemory) + _int.DefaultLength;
        }
    }
}
