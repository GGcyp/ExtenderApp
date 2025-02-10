using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Files
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

        #endregion

        public PathService(IHostEnvironment environment)
        {
            _environment = environment;

            LoggingPath = ChekAndCreateFolder(LOOGINGNAME);
            LibPath = ChekAndCreateFolder(LIBNAME);
            ModsPath = ChekAndCreateFolder(MODSNAME);
            DataPath = ChekAndCreateFolder(DATANAME);

            PackFolderName = PACKNAME;
        }

        /// <summary>
        /// 检查并创建文件夹
        /// </summary>
        /// <param name="environment">主机环境</param>
        /// <param name="name">文件夹名称</param>
        /// <returns>文件夹路径</returns>
        private string ChekAndCreateFolder(string name)
        {
            string path = Path.Combine(_environment.ContentRootPath, name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public string CreateFolderPathForAppRootFolder(string folferName)
        {
            return ChekAndCreateFolder(folferName);
        }

        public string LoggingPath { get; }

        public string LibPath { get; }

        public string ModsPath { get; }

        public string DataPath { get; }

        public string PackFolderName { get; }
    }
}
