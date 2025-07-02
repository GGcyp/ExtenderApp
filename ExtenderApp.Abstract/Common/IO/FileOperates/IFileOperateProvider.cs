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
    }
}
