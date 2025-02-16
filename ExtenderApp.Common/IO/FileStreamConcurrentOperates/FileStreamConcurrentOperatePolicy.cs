using ExtenderApp.Common.ConcurrentOperates;

namespace ExtenderApp.Common.IO
{
    public class FileStreamConcurrentOperatePolicy : FileStreamConcurrentOperatePolicy<FileStreamConcurrentOperateData>
    {

    }

    public class FileStreamConcurrentOperatePolicy<T> : ConcurrentOperatePolicy<FileStream, T> where T : FileStreamConcurrentOperateData, new()
    {
        public override FileStream Create(T data)
        {
            return data.OperateInfo.OpenFile();
        }
    }
}
