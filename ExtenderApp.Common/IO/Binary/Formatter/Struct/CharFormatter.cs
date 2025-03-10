﻿using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// CharFormatter 类是一个内部类，继承自 ExtenderFormatter<char>，用于格式化字符类型的数据。
    /// </summary>
    internal class CharFormatter : ExtenderFormatter<char>
    {
        public override int Length => 3;

        public CharFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override char Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadChar(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, char value)
        {
            _binaryWriterConvert.Write(ref writer, value);
        }
    }
}
