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
        /// 获取本地文件信息。
        /// </summary>
        public LocalFileInfo LocalFileInformation => OperateInfo.LocalFileInfo;

        /// <summary>
        /// 获取或设置文件长度（以字节为单位）。
        /// </summary>
        public long FileLength { get; private set; }

        /// <summary>
        /// 文件操作信息释放委托。
        /// </summary>
        private Action<FileOperateInfo> releaseFileOperateInfo;

        /// <summary>
        /// 获取或设置文件流
        /// </summary>
        public Stream? FileStream { get; internal set; }

        /// <summary>
        /// 获取或设置内存映射文件
        /// </summary>
        public MemoryMappedFile? FileMemoryMappedFile { get; internal set; }

        /// <summary>
        /// 打开文件的方法
        /// </summary>
        /// <param name="info">文件操作信息</param>
        /// <param name="fileLength">文件长度</param>
        /// <param name="token">取消令牌</param>
        /// <param name="action">文件操作完成后的回调</param>
        public void OpenFile(FileOperateInfo info, long fileLength, CancellationToken token, Action<FileOperateInfo> action)
        {
            this.OperateInfo = info;
            releaseFileOperateInfo = action;
            Token = token;
            FileLength = fileLength;
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
        /// 尝试重置文件操作信息。
        /// </summary>
        /// <returns>如果重置成功，则返回 true；否则返回 false。</returns>
        public override bool TryReset()
        {
            OperateInfo = FileOperateInfo.Empty;
            releaseFileOperateInfo = null;
            FileStream.Dispose();
            FileMemoryMappedFile.Dispose();
            FileStream = null;
            FileMemoryMappedFile = null;
            return base.TryReset();
        }
    }
}
