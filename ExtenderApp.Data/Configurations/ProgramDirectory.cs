using System.Reflection;
using System.Runtime.InteropServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 程序目录工具类（静态）。
    /// 负责解析应用程序根目录并在首次访问时生成常用的程序文件夹路径（例如日志、动态库、插件、数据、pack）。
    /// 所有路径为只读静态属性，且在类型初始化时保证对应目录存在。
    /// </summary>
    public static class ProgramDirectory
    {
        #region PathName

        /// <summary>
        /// 日志文件的目录名（相对于应用根目录）。
        /// 对应的完整路径可通过 <see cref="LoggingPath"/> 获取。
        /// </summary>
        public const string LOOGINGNAME = "logs";

        /// <summary>
        /// 动态库（主程序库）的目录名（相对于应用根目录）。
        /// 对应的完整路径可通过 <see cref="LibPath"/> 获取。
        /// </summary>
        public const string LIBNAME = "lib";

        /// <summary>
        /// 插件文件的目录名（相对于应用根目录）。
        /// 对应的完整路径可通过 <see cref="PluginPath"/> 获取。
        /// </summary>
        public const string PLUGINSNAME = "plugins";

        /// <summary>
        /// 依赖包（pack）目录名（相对于应用根目录）。
        /// 对应的完整路径可通过 <see cref="PackPath"/> 获取。
        /// </summary>
        public const string PACKNAME = "pack";

        /// <summary>
        /// 数据存放目录名（相对于应用根目录），用于本地数据/持久化文件。
        /// 对应的完整路径可通过 <see cref="DataPath"/> 获取。
        /// </summary>
        public const string DATANAME = "data";

        /// <summary>
        /// 插件初始化文件名。
        /// </summary>
        public const string PLUGININITENAME = "init.json";

        #endregion PathName

        /// <summary>
        /// 应用程序根目录（解析结果）。例如可为 AppContext.BaseDirectory、EntryAssembly 目录或当前工作目录的回退值。
        /// </summary>
        public static string AppRootPath { get; }

        /// <summary>
        /// 日志目录的完整路径（已在类型初始化时确保存在）。
        /// </summary>
        public static string LoggingPath { get; }

        /// <summary>
        /// 主程序动态库目录的完整路径（已在类型初始化时确保存在）。
        /// </summary>
        public static string LibPath { get; }

        /// <summary>
        /// 插件目录的完整路径（已在类型初始化时确保存在）。
        /// </summary>
        public static string PluginPath { get; }

        /// <summary>
        /// 全局数据目录的完整路径（已在类型初始化时确保存在），用于存放本地数据文件等。
        /// </summary>
        public static string DataPath { get; }

        /// <summary>
        /// pack 目录的完整路径（已在类型初始化时确保存在），用于放置第三方/引用包等。
        /// </summary>
        public static string PackPath { get; }

        /// <summary>
        /// 静态构造函数：在类型首次访问时执行。
        /// - 解析应用根目录；
        /// - 基于常量目录名生成完整路径并确保对应目录存在（不存在则创建）。
        /// </summary>
        static ProgramDirectory()
        {
            AppRootPath = ResolveAppRootPath();
            LoggingPath = ChekAndCreateFolder(LOOGINGNAME);
            LibPath = ChekAndCreateFolder(LIBNAME);
            PluginPath = ChekAndCreateFolder(PLUGINSNAME);
            DataPath = ChekAndCreateFolder(DATANAME);
            PackPath = ChekAndCreateFolder(PACKNAME);
        }

        /// <summary>
        /// 解析应用的根目录，兼容多种运行场景。
        /// 优先级：
        /// 1. AppContext.BaseDirectory（大多数桌面/控制台/托管场景适用）
        /// 2. Entry assembly 的位置（exe 主程序集时）
        /// 3. Executing assembly 的位置（当当前程序集被另一个进程/宿主加载时）
        /// 4. 当前工作目录（回退）
        /// </summary>
        /// <returns>解析后的应用根路径（不带尾部分隔符）</returns>
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

        /// <summary>
        /// 根据给定的相对目录名（相对于 AppRootPath）返回完整路径并确保该目录存在。
        /// 注意：方法名保留原拼写 ChekAndCreateFolder。
        /// </summary>
        /// <param name="folderName">相对目录名（例如 "log"）</param>
        /// <returns>完整目录路径</returns>
        public static string ChekAndCreateFolder(string folderName)
        {
            string path = Path.Combine(AppRootPath, folderName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);
    }
}