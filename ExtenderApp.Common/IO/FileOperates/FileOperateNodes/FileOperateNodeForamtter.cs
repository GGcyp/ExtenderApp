using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    internal class FileOperateNodeForamtter<T> : FileNodeFormatter<T> where T : FIleOperateNode<T>, new()
    {
        public FileOperateNodeForamtter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(resolver, convert, options)
        {
        }
    }
}
