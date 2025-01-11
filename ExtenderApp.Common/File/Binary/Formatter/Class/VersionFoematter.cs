using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    /// <summary>
    /// 内部类 VersionFormatter，继承自 ExtenderFormatter<Version> 类。
    /// 用于格式化 Version 类型的对象。
    /// </summary>
    internal class VersionFoematter : ExtenderFormatter<Version>
    {
        public VersionFoematter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override Version Deserialize(ref ExtenderBinaryReader reader)
        {
            if (_binaryReaderConvert.TryReadNil(ref reader)) return null;

            _binaryReaderConvert.TryReadStringSpan(ref reader, out ReadOnlySpan<byte> bytes);
            var value = _binaryReaderConvert.UTF8ToString(bytes);
            return new Version(value);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, Version value)
        {
            if (value == null)
            {
                _binaryWriterConvert.WriteNil(ref writer);
            }
            else
            {
                _binaryWriterConvert.Write(ref writer, value.ToString());
            }
        }
    }
}
