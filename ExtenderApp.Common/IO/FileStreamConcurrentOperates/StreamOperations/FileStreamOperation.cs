

using ExtenderApp.Abstract;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 表示一个抽象的文件流操作类，继承自ConcurrentOperation<FileStream>类。
    /// </summary>
    public abstract class FileStreamOperation : ConcurrentOperation<FileStream>
    {
        protected FileStreamOperation(Action<IConcurrentOperation> releaseAction) : base(releaseAction)
        {
        }
    }
}
