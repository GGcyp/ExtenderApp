using ExtenderApp.Common.ConcurrentOperates;
using System.IO.MemoryMappedFiles;

namespace ExtenderApp.Common.IO
{
    public class FileConcurrentOperatePolicy : FileStreamConcurrentOperatePolicy<FileConcurrentOperateData>
    {

    }

    public class FileStreamConcurrentOperatePolicy<T> : ConcurrentOperatePolicy<MemoryMappedViewAccessor, T> where T : FileConcurrentOperateData, new()
    {
        public override MemoryMappedViewAccessor Create(T data)
        {
            return MemoryMappedFile.CreateFromFile(data.OperateInfo.OpenFile(), data.OperateInfo.LocalFileInfo.FileName, data.FileLength, MemoryMappedFileAccess.ReadWrite, HandleInheritability.Inheritable, true).CreateViewAccessor();
        }
    }
}
