using ExtenderApp.Abstract;
using ExtenderApp.Common.Caches;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件存储类，用于管理文件操作的并发处理。
    /// </summary>
    internal class FileOperateProvider : EvictionCache<int, FileConcurrentOperate>, IFileOperateProvider
    {
        /// <summary>
        /// 文件并发操作对象池
        /// </summary>
        private readonly ObjectPool<FileConcurrentOperate> _pool;

        /// <summary>
        /// 文件操作完成后执行的释放动作。
        /// </summary>
        private readonly Action<FileOperateInfo> _releaseAction;

        /// <summary>
        /// 初始化 FileStorage 类的新实例。
        /// </summary>
        /// <param name="operatePool">并发操作池实例。</param>
        public FileOperateProvider()
        {
            _releaseAction = ReleaseOperate;
            _pool = ObjectPool.CreateDefaultPool<FileConcurrentOperate>();
        }

        /// <summary>
        /// 根据给定的文件操作信息获取文件并发操作实例。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <returns>返回文件并发操作实例。</returns>
        public IFileOperate GetOperate(FileOperateInfo info)
        {
            int hashCode = info.GetHashCode();
            if (TryGet(hashCode, out var result))
                return result;

            lock (this)
            {
                if (TryGet(hashCode, out result))
                    return result;

                result = _pool.Get();
                FileOperateData data = new();
                data.Set(info, _releaseAction);
                result.Start(data);
                AddOrUpdate(hashCode, result);
            }
            return result;
        }

        public void ReleaseOperate(IFileOperate fileOperate)
        {
            ReleaseOperate(fileOperate.Info.GetHashCode(), out var value);
        }

        public void ReleaseOperate(FileOperateInfo info)
        {
            ReleaseOperate(info.GetHashCode(), out var value);
        }

        public void ReleaseOperate(FileOperateInfo info, out IFileOperate? fileOperate)
        {
            ReleaseOperate(info.GetHashCode(), out var value);
            fileOperate = value;
        }

        public void ReleaseOperate(int id, out IFileOperate? fileOperate)
        {
            Remove(id, out var value);
            fileOperate = value;
        }

        protected override void Evict(int id, FileConcurrentOperate value)
        {
            _pool.Release(value);
        }

        protected override bool ShouldEvict(FileConcurrentOperate value, DateTime now)
        {
            return !value.IsExecuting || value.IsHosted;
        }
    }
}
