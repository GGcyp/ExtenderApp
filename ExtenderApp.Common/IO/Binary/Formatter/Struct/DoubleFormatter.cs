﻿using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// DoubleFormatter 类，继承自 ExtenderFormatter<Bool> 类，用于格式化 Bool 类型的数据。
    /// </summary>
    internal sealed class DoubleFormatter : ExtenderFormatter<Double>
    {
        public override int Length => 9;

        public DoubleFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override double Deserialize(ref ExtenderBinaryReader reader)
        {
            return _binaryReaderConvert.ReadDouble(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, double value)
        {
            _binaryWriterConvert.Write(ref writer, value);
        }
    }
}
