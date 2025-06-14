using System.IO.MemoryMappedFiles;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 表示文件流并发操作的数据类。
    /// </summary>
    public class FileOperateData : ConcurrentOperateData
    {
        /// <summary>
        /// 获取或设置文件操作信息。
        /// </summary>
        public FileOperateInfo OperateInfo;

        /// <summary>
        /// 文件操作信息释放委托。
        /// </summary>
        private Action<FileOperateInfo> releaseFileOperateInfo;

        /// <summary>
        /// 文件流
        /// </summary>
        public FileStream FStream { get; private set; }

        /// <summary>
        /// 内存映射文件
        /// </summary>
        private MemoryMappedFile _mmf;

        /// <summary>
        /// 当前容量
        /// </summary>
        public long CurrentCapacity { get; private set; }

        /// <summary>
        /// 内存映射视图访问器
        /// </summary>
        public MemoryMappedViewAccessor Accessor { get; private set; }

        /// <summary>
        /// 内存映射文件访问权限
        /// </summary>
        public MemoryMappedFileAccess MMFileAccess { get; set; }

        /// <summary>
        /// 句柄继承性
        /// </summary>
        public HandleInheritability HInheritability { get; set; }

        /// <summary>
        /// 是否在关闭时保持文件打开
        /// </summary>
        public bool LeaveOpen { get; set; }

        /// <summary>
        /// 设置文件操作信息并创建内存映射。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <param name="releaseAction">释放文件操作信息的操作。</param>
        public void Set(FileOperateInfo info, Action<FileOperateInfo> releaseAction)
        {
            OperateInfo = info;
            releaseFileOperateInfo = releaseAction;
            FStream = OperateInfo.OpenFile();
            CurrentCapacity = FStream.Length;
            MMFileAccess = MemoryMappedFileAccess.ReadWrite;
            HInheritability = HandleInheritability.Inheritable;
            LeaveOpen = true;

            // 创建内存映射
            CreateMapped();
        }

        /// <summary>
        /// 释放文件操作信息。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        public void ReleaseFileOperateInfo(FileOperateInfo info)
        {
            releaseFileOperateInfo?.Invoke(info);
        }

        /// <summary>
        /// 扩展容量
        /// </summary>
        /// <param name="newCapacity">新容量</param>
        public void ExpandCapacity(long newCapacity)
        {
            if (newCapacity <= CurrentCapacity) return;

            // 释放现有资源
            Accessor.Dispose();
            _mmf.Dispose();

            // 调整文件大小
            FStream.SetLength(newCapacity);
            CurrentCapacity = newCapacity;

            // 重新创建内存映射
            CreateMapped();
        }

        /// <summary>
        /// 创建内存映射
        /// </summary>
        private void CreateMapped()
        {
            if (FStream.Length == 0 && CurrentCapacity == 0)
            {
                CurrentCapacity = 1; // 确保至少有一个字节的容量
                FStream.SetLength(CurrentCapacity);
            }
            _mmf = MemoryMappedFile.CreateFromFile(FStream, OperateInfo.LocalFileInfo.FileName, CurrentCapacity, MemoryMappedFileAccess.ReadWrite, HandleInheritability.Inheritable, true);
            Accessor = _mmf.CreateViewAccessor();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) return;

            Accessor?.Dispose();
            _mmf?.Dispose();
            FStream?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 尝试重置文件操作信息。
        /// </summary>
        /// <returns>如果重置成功，则返回 true；否则返回 false。</returns>
        public override bool TryReset()
        {
            OperateInfo = FileOperateInfo.Empty;
            releaseFileOperateInfo = null;
            return base.TryReset();
        }
    }
}
