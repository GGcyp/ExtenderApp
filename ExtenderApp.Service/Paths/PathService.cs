
using System.Reflection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 路径提供者类，实现了INAMEProvider接口
    /// </summary>
    internal class PathService : IPathService
    {
        #region PathName

        /// <summary>
        /// 日志文件的存储路径
        /// </summary>
        private const string LOOGINGNAME = "log";

        /// <summary>
        /// 动态库的存储路径
        /// </summary>
        private const string LIBNAME = "lib";

        /// <summary>
        /// 插件文件的存储路径
        /// </summary>
        private const string PLUGINSNAME = "plugins";

        /// <summary>
        /// 依赖包的存储路径
        /// </summary>
        private const string PACKNAME = "pack";

        /// <summary>
        /// 依赖包的存储路径
        /// </summary>
        private const string DATANAME = "data";

        #endregion

        public PathService()
        {
            AppRootPath = ResolveAppRootPath();
            LoggingPath = ChekAndCreateFolder(LOOGINGNAME);
            LibPath = ChekAndCreateFolder(LIBNAME);
            ModsPath = ChekAndCreateFolder(PLUGINSNAME);
            DataPath = ChekAndCreateFolder(DATANAME);

            PackFolderName = PACKNAME;
        }

        public string CreateFolderPathForAppRootFolder(string folferName)
        {
            return ChekAndCreateFolder(folferName);
        }

        private string ChekAndCreateFolder(string folderName)
        {
            string path = Path.Combine(AppRootPath, folderName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// 在文件资源管理器中打开指定路径的文件夹
        /// </summary>
        /// <param name="folderPath">要打开的文件夹路径</param>
        public void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                try
                {
                    // 启动资源管理器并打开指定目录
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = folderPath,
                        UseShellExecute = true,
                        Verb = "open" // 确保以打开方式启动
                    });
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("无法打开指定的文件夹路径。", ex);
                }
            }
            else
            {
                throw new DirectoryNotFoundException("指定的文件夹路径不存在。");
            }
        }

        public string OpenFile(string filter, string? targetPath = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 解析应用根目录（不依赖微软主机），优先级：
        /// 1. AppContext.BaseDirectory
        /// 2. Entry assembly 的位置
        /// 3. Executing assembly 的位置
        /// 4. 当前工作目录
        /// </summary>
        /// <returns>程序根目录</returns>
        private static string ResolveAppRootPath()
        {
            // 1) 优先使用 AppContext.BaseDirectory（大多数托管/控制台/桌面应用正确）
            var baseDir = AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDir))
            {
                return baseDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            // 2) 使用 EntryAssembly 的位置（可用于 exe 主程序集）
            var entryLocation = Assembly.GetEntryAssembly()?.Location;
            if (!string.IsNullOrEmpty(entryLocation))
            {
                var dir = Path.GetDirectoryName(entryLocation);
                if (!string.IsNullOrEmpty(dir))
                    return dir;
            }

            // 3) 使用当前执行程序集的位置（当被加载为库时有帮助）
            var execLocation = Assembly.GetExecutingAssembly()?.Location;
            if (!string.IsNullOrEmpty(execLocation))
            {
                var dir = Path.GetDirectoryName(execLocation);
                if (!string.IsNullOrEmpty(dir))
                    return dir;
            }

            // 4) 最后回退到当前工作目录
            return Directory.GetCurrentDirectory();
        }

        public string LoggingPath { get; }

        public string LibPath { get; }

        public string ModsPath { get; }

        public string DataPath { get; }

        public string PackFolderName { get; }

        public string AppRootPath { get; }
    }
}
