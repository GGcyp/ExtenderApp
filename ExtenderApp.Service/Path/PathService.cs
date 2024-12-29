using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.File
{
    /// <summary>
    /// 路径提供者类，实现了INAMEProvider接口
    /// </summary>
    internal class PathService : IPathService
    {
        /// <summary>
        /// 日志文件的存储路径
        /// </summary>
        private const string LOOGINGNAME = "log";

        /// <summary>
        /// 可执行文件的存储路径
        /// </summary>
        private const string BINNAME = "bin";

        /// <summary>
        /// 模块文件的存储路径
        /// </summary>
        private const string MODSNAME = "mods";

        /// <summary>
        /// 依赖包的存储路径
        /// </summary>
        private const string PACKNAME = "pack";

        /// <summary>
        /// 依赖包的存储路径
        /// </summary>
        private const string DATANAME = "data";

        public PathService(IHostEnvironment environment)
        {
            LoggingPath = ChekAndCreateFolder(environment, LOOGINGNAME);
            BinPath = ChekAndCreateFolder(environment, BINNAME);
            ModsPath = ChekAndCreateFolder(environment, MODSNAME);
            DataPath = ChekAndCreateFolder(environment, DATANAME);

            PackFolderName = PACKNAME;
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

        public string DataPath { get; }

        public string PackFolderName { get; }
    }
}
