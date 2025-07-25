﻿using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatter.Struct
{
    internal class ExtensionHeaderFormatter : ExtenderFormatter<ExtensionHeader>
    {
        public override int Length => 5;
        public ExtensionHeaderFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override ExtensionHeader Deserialize(ref ExtenderBinaryReader reader)
        {
            _binaryReaderConvert.TryExtensionHeader(ref reader, out var extension);
            return extension;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, ExtensionHeader value)
        {
            _binaryWriterConvert.WriteExtensionFormatHeader(ref writer, value);
        }
    }
}
