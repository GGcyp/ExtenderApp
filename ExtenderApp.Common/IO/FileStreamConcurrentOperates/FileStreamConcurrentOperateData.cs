using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 表示文件流并发操作的数据类。
    /// </summary>
    public class FileStreamConcurrentOperateData : ConcurrentOperateData
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
        /// 文件操作信息释放委托。
        /// </summary>
        private Action<FileOperateInfo> releaseFileOperateInfo;

        /// <summary>
        /// 打开文件。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <param name="token">取消令牌。</param>
        /// <param name="action">文件操作信息释放委托。</param>
        public void OpenFile(FileOperateInfo info, CancellationToken token, Action<FileOperateInfo> action)
        {
            this.OperateInfo = info;
            releaseFileOperateInfo = action;
            Token = token;
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
            return base.TryReset();
        }
    }
}
