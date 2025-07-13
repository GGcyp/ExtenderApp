using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    internal class FileOperateNodeForamtter<T> : FileNodeFormatter<T> where T : FIleOperateNode<T>, new()
    {
        public FileOperateNodeForamtter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
        {
        }
    }
}
