using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.ObjectPools;
using System.IO.MemoryMappedFiles;

namespace ExtenderApp.Common.IO
{
    public class FileOperatePolicy : FileStreamConcurrentOperatePolicy<FileOperateData>
    {

    }

    public class FileStreamConcurrentOperatePolicy<T> : ConcurrentOperatePolicy<MemoryMappedViewAccessor, T> where T : FileOperateData, new()
    {
        private readonly ObjectPool<T> _pool = ObjectPool.CreateDefaultPool<T>();

        public override MemoryMappedViewAccessor Create(T data)
        {
            FileStream fileStream = data.OperateInfo.OpenFile();
            MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateFromFile(fileStream, data.OperateInfo.LocalFileInfo.FileName, data.FileLength, MemoryMappedFileAccess.ReadWrite, HandleInheritability.Inheritable, true);
            data.FileStream = fileStream;
            data.FileMemoryMappedFile = memoryMappedFile;
            return memoryMappedFile.CreateViewAccessor();
        }

        public override T GetData()
        {
            return _pool.Get();
        }

        public override void ReleaseData(T data)
        {
            _pool.Release(data);
        }
    }
}
