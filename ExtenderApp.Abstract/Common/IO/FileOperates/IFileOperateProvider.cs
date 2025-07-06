using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件操作提供程序接口
    /// </summary>
    public interface IFileOperateProvider
    {
        /// <summary>
        /// 根据文件操作信息获取文件操作对象
        /// </summary>
        /// <param name="info">文件操作信息</param>
        /// <returns>文件操作对象</returns>
        IFileOperate GetOperate(FileOperateInfo info);

        /// <summary>
        /// 释放文件操作对象
        /// </summary>
        /// <param name="info">文件操作信息</param>
        void ReleaseOperate(FileOperateInfo info);

        /// <summary>
        /// 执行释放操作
        /// </summary>
        /// <param name="fileOperate">文件操作接口</param>
        void ReleaseOperate(IFileOperate fileOperate);

        /// <summary>
        /// 释放文件操作对象。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <param name="fileOperate">返回的文件操作对象，可能为 null。</param>
        void ReleaseOperate(FileOperateInfo info, out IFileOperate? fileOperate);

        /// <summary>
        /// 根据文件操作ID释放文件操作对象。
        /// </summary>
        /// <param name="id">文件操作ID。</param>
        /// <param name="fileOperate">返回的文件操作对象，可能为 null。</param>
        void ReleaseOperate(int id, out IFileOperate? fileOperate);
    }
}
