using System.IO.MemoryMappedFiles;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.IO
{

    public abstract class FileOperation : ConcurrentOperation<MemoryMappedViewAccessor>
    {
        protected FileOperation(Action<IConcurrentOperation> releaseAction) : base(releaseAction)
        {
        }
    }
}
