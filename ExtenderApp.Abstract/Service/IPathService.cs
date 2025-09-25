namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 路径提供者接口
    /// </summary>
    public interface IPathService
    {
        /// <summary>
        /// 程序所在的根目录
        /// </summary>
        string AppRootPath { get; }

        /// <summary>
        /// 获取日志路径
        /// </summary>
        string LoggingPath { get; }

        /// <summary>
        /// 获取主程序动态库文件路径
        /// </summary>
        string LibPath { get; }

        /// <summary>
        /// 获取模组路径
        /// </summary>
        string ModsPath { get; }

        /// <summary>
        /// 全局统一的数据存放地
        /// </summary>
        string DataPath { get; }

        /// <summary>
        /// 引用包的文件夹名字
        /// </summary>
        string PackFolderName { get; }

        /// <summary>
        /// 根据文件夹名称，在根目录下创建新的文件夹路径
        /// </summary>
        /// <param name="folderName">要创建的文件夹名称</param>
        /// <returns>返回新创建的文件夹路径</returns>
        string CreateFolderPathForAppRootFolder(string folferName);

        /// <summary>
        /// 在文件资源管理器中打开指定路径的文件夹
        /// </summary>
        /// <param name="folderPath">要打开的文件夹路径</param>
        void OpenFolder(string folderPath);

        /// <summary>
        /// 打开文件选择对话框，允许用户选择指定类型的文件。
        /// </summary>
        /// <param name="filter">文件筛选器，例如 "文本文件 (*.txt)|*.txt"</param>
        /// <param name="targetPath">对话框初始打开的文件夹路径，默认为空（使用系统默认路径）</param>
        /// <returns>用户选择的文件完整路径，若未选择则返回空字符串或 null</returns>
        string OpenFile(string filter, string? targetPath = null);
    }
}
