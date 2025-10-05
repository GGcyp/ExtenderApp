using ExtenderApp.Abstract;
using ExtenderApp.Common.Caches;
using ExtenderApp.Common.IO.FileOperates;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件存储类，用于管理文件操作的并发处理。
    /// </summary>
    internal class FileOperateProvider : EvictionCache<HashValue, IFileOperate>, IFileOperateProvider
    {
        public IFileOperate GetOperate(FileOperateInfo info)
        {
            return GetOperate(info, FileOperateType.FileStream);
        }

        public IFileOperate GetOperate(FileOperateInfo info, FileOperateType type)
        {
            var hash = info.LocalFileInfo.Hash;
            if (TryGet(hash, out var result))
                return result;

            lock (this)
            {
                if (TryGet(hash, out result))
                    return result;

                result = FileOperateFactory.Create(type, info);
                AddOrUpdate(hash, result);
            }
            return result;
        }

        public void ReleaseOperate(IFileOperate fileOperate)
        {
            ReleaseOperate(fileOperate.Info.Hash, out var value);
        }

        public void ReleaseOperate(FileOperateInfo info)
        {
            ReleaseOperate(info.LocalFileInfo.Hash, out var value);
        }

        public void ReleaseOperate(FileOperateInfo info, out IFileOperate? fileOperate)
        {
            ReleaseOperate(info.LocalFileInfo.Hash, out var value);
            fileOperate = value;
        }

        public void ReleaseOperate(HashValue id, out IFileOperate? fileOperate)
        {
            Remove(id, out fileOperate);
        }

        protected override void Evict(HashValue key, IFileOperate value)
        {
            value.Dispose();
        }
    }
}
