using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 内部类 VersionFormatter，继承自 ResolverFormatter<Version> 类。
    /// 用于格式化 Version 类型的对象。
    /// </summary>
    internal class VersionFoematter : ResolverFormatter<Version>
    {
        private readonly IBinaryFormatter<int> _int;

        public VersionFoematter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override int DefaultLength => _int.DefaultLength * 4;

        public override Version Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return null;
            }

            var major = _int.Deserialize(ref buffer);
            var minor = _int.Deserialize(ref buffer);
            var build = _int.Deserialize(ref buffer);
            var revision = _int.Deserialize(ref buffer);

            //int major, int minor, int build, int revision
            return new Version(major, minor, build, revision);
        }

        public override void Serialize(ref ByteBuffer buffer, Version value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
                return;
            }
            _int.Serialize(ref buffer, value.Major);
            _int.Serialize(ref buffer, value.Minor);
            _int.Serialize(ref buffer, value.Build);
            _int.Serialize(ref buffer, value.Revision);
        }

        public override long GetLength(Version value)
        {
            if (value == null)
            {
                return 1;
            }

            return _int.GetLength(value.Major) +
                   _int.GetLength(value.Minor) +
                   _int.GetLength(value.Build) +
                   _int.GetLength(value.Revision);
        }
    }
}
