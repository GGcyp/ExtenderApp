using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 路径提供者类，实现了INAMEProvider接口
    /// </summary>
    internal class PathService : IPathService
    {
        private readonly IHostEnvironment _environment;

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

        public PathService(IHostEnvironment environment)
        {
            _environment = environment;
            AppRootPath = environment.ContentRootPath;
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
            string path = Path.Combine(_environment.ContentRootPath, folderName);
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
        public void OpenFolderInExplorer(string folderPath)
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

        public string LoggingPath { get; }

        public string LibPath { get; }

        public string ModsPath { get; }

        public string DataPath { get; }

        public string PackFolderName { get; }

        public string AppRootPath { get; }
    }
}
