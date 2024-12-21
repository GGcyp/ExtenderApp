namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 路径提供者接口
    /// </summary>
    public interface IPathProvider
    {
        /// <summary>
        /// 获取日志路径
        /// </summary>
        string LoggingPath { get; }

        /// <summary>
        /// 获取主程序文件路径
        /// </summary>
        string BinPath { get; }

        /// <summary>
        /// 获取模组路径
        /// </summary>
        string ModsPath { get; }

        /// <summary>
        /// 引用包的文件夹名字
        /// </summary>
        string PackFolderName { get; }
    }
}
