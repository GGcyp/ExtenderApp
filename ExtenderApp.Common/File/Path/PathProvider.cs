using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.File
{
    /// <summary>
    /// 路径提供者类，实现了IPathProvider接口
    /// </summary>
    internal class PathProvider : IPathProvider
    {
        /// <summary>
        /// 日志文件的存储路径
        /// </summary>
        private const string LOOGINGPATH = "log";

        /// <summary>
        /// 可执行文件的存储路径
        /// </summary>
        private const string BINPATH = "bin";

        /// <summary>
        /// 模块文件的存储路径
        /// </summary>
        private const string MODSPATH = "mods";

        /// <summary>
        /// 数据包的存储路径
        /// </summary>
        private const string PACKPATH = "pack";

        public PathProvider(IHostEnvironment environment)
        {
            LoggingPath = ChekAndCreateFolder(environment, LOOGINGPATH);
            BinPath = ChekAndCreateFolder(environment, BINPATH);
            ModsPath = ChekAndCreateFolder(environment, MODSPATH);
            PackFolderName = PACKPATH;
        }

        /// <summary>
        /// 检查并创建文件夹
        /// </summary>
        /// <param name="environment">主机环境</param>
        /// <param name="name">文件夹名称</param>
        /// <returns>文件夹路径</returns>
        private string ChekAndCreateFolder(IHostEnvironment environment, string name)
        {
            string path = Path.Combine(environment.ContentRootPath, name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public string LoggingPath { get; }

        public string BinPath { get; }

        public string ModsPath { get; }

        public string PackFolderName { get; }
    }
}
