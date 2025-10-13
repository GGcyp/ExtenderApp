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
        private readonly IBinaryFormatter<string> _string;

        public VersionFoematter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _string = GetFormatter<string>();
        }

        public override int DefaultLength => _string.DefaultLength;

        public override Version Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return null;
            }

            var version = _string.Deserialize(ref buffer);

            return new Version(version);
        }

        public override void Serialize(ref ByteBuffer buffer, Version value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
                return;
            }
            _string.Serialize(ref buffer, value.ToString());
        }

        public override long GetLength(Version value)
        {
            if (value == null)
            {
                return 1;
            }

            return _string.GetLength(value.ToString());
        }
    }
}
