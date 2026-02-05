using ExtenderApp.Abstract;
using ExtenderApp.Common.Caches;
using ExtenderApp.Common.IO.FileOperates;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件存储类，用于管理文件操作的并发处理。
    /// </summary>
    internal class FileOperateProvider : EvictionCache<int, IFileOperate>, IFileOperateProvider
    {
        public IFileOperate GetOperate(FileOperateInfo info)
        {
            return GetOperate(info, FileOperateType.FileStream);
        }

        public IFileOperate GetOperate(FileOperateInfo info, FileOperateType type)
        {
            var hash = GetInfoHashCode(info);
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
            var hash = GetInfoHashCode(fileOperate.Info);
            Remove(hash, out _);
        }

        public void ReleaseOperate(FileOperateInfo info)
        {
            var hash = GetInfoHashCode(info);
            Remove(hash, out _);
        }

        public void ReleaseOperate(FileOperateInfo info, out IFileOperate? fileOperate)
        {
            var hash = GetInfoHashCode(info);
            Remove(hash, out fileOperate);
        }

        protected override void Evict(int key, IFileOperate value)
        {
            value.Dispose();
        }

        public int GetInfoHashCode(FileOperateInfo info)
        {
            var hash = info.LocalFileInfo.FullPath.AsSpan().GetHashValue_SHA256();
            int hashCode = hash.GetHashCode();
            hash.Dispose();
            return hashCode;
        }
    }
}