using ExtenderApp.Abstract;
using ExtenderApp.Buffer;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary> 内部类 VersionFormatter，继承自 ResolverFormatter<Version> 类。 用于格式化 Version 类型的对象。 </summary>
    internal class VersionFoematter : ResolverFormatter<Version>
    {
        private readonly IBinaryFormatter<int> _int;

        public VersionFoematter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override int DefaultLength => _int.DefaultLength * 4 + NilLength;

        public override void Serialize(AbstractBuffer<byte> buffer, Version value)
        {
            if (value == null)
            {
                WriteNil(buffer);
                return;
            }
            _int.Serialize(buffer, value.Major);
            _int.Serialize(buffer, value.Minor);
            _int.Serialize(buffer, value.Build);
            _int.Serialize(buffer, value.Revision);
        }

        public override void Serialize(ref SpanWriter<byte> writer, Version value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }
            _int.Serialize(ref writer, value.Major);
            _int.Serialize(ref writer, value.Minor);
            _int.Serialize(ref writer, value.Build);
            _int.Serialize(ref writer, value.Revision);
        }

        public override Version Deserialize(AbstractBufferReader<byte> reader)
        {
            if (TryReadNil(reader))
            {
                return null!;
            }

            var major = _int.Deserialize(reader);
            var minor = _int.Deserialize(reader);
            var build = _int.Deserialize(reader);
            var revision = _int.Deserialize(reader);

            return new Version(major, minor, build, revision);
        }

        public override Version Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return null!;
            }

            var major = _int.Deserialize(ref reader);
            var minor = _int.Deserialize(ref reader);
            var build = _int.Deserialize(ref reader);
            var revision = _int.Deserialize(ref reader);

            return new Version(major, minor, build, revision);
        }

        public override long GetLength(Version value)
        {
            if (value == null)
            {
                return NilLength;
            }

            return _int.GetLength(value.Major) +
                   _int.GetLength(value.Minor) +
                   _int.GetLength(value.Build) +
                   _int.GetLength(value.Revision);
        }
    }
}