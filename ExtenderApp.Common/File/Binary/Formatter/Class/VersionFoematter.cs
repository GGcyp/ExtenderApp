using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Files.Binary.Formatter
{
    /// <summary>
    /// 内部类 VersionFormatter，继承自 ResolverFormatter<Version> 类。
    /// 用于格式化 Version 类型的对象。
    /// </summary>
    internal class VersionFoematter : ResolverFormatter<Version>
    {
        private readonly IBinaryFormatter<string> _formatter;

        public VersionFoematter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _formatter = GetFormatter<string>();
        }

        public override int Count => _formatter.Count;

        public override Version Deserialize(ref ExtenderBinaryReader reader)
        {
            //if (_binaryReaderConvert.TryReadNil(ref reader)) return null;

            //_binaryReaderConvert.TryReadStringSpan(ref reader, out ReadOnlySpan<byte> bytes);
            //var value = _binaryReaderConvert.UTF8ToString(bytes);

            var version = _formatter.Deserialize(ref reader);

            return string.IsNullOrEmpty(version) ? null : new Version(version);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, Version value)
        {
            //if (value == null)
            //{
            //    _binaryWriterConvert.WriteNil(ref writer);
            //}
            //else
            //{
            //    _binaryWriterConvert.Write(ref writer, value.ToString());
            //}
            _formatter.Serialize(ref writer, value == null ? string.Empty : value.ToString());
        }

        public override int GetCount(Version value)
        {
            return _formatter.GetCount(value == null ? string.Empty : value.ToString());
        }
    }
}
