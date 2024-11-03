
namespace MainApp.Common
{
    /// <summary>
    /// 系统文件管理目录
    /// </summary>
    public struct FileArchitectureInfo
    {
        private static string appPath = Directory.GetCurrentDirectory();

        public string Path { get; }
        public bool IsEmpty => string.IsNullOrEmpty(Path);
        public bool IsEmptyForFolder
        {
            get
            {
                if (IsEmpty) return false;

                return !Directory.Exists(Path);
            }
        }

        public FileArchitectureInfo(string path)
        {
            Path = string.Format(appPath, "\\", path);
        }

        /// <summary>
        /// 获取文件夹下的文件目录
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public string GetPath(string folderName)
        {
            return string.Format(Path, "\\", folderName);
        }

        /// <summary>
        /// 获取文件夹下文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetPath(string fileName, FileExtensionType type)
        {
            return GetPath(string.Format(fileName, type.Extension));
        }

        public static FileArchitectureInfo Empty => new FileArchitectureInfo(string.Empty);

        private static FileArchitectureInfo _bin;
        /// <summary>
        /// 系统主要文件夹
        /// </summary>
        public static FileArchitectureInfo Bin
        {
            get
            {
                if(_bin.IsEmpty)
                {
                    _bin = new FileArchitectureInfo("bin");
                }
                return _bin;
            }
        }

        private static FileArchitectureInfo _save;
        /// <summary>
        /// 系统存储文件夹
        /// </summary>
        public static FileArchitectureInfo Save
        {
            get
            {
                if(_save.IsEmpty)
                {
                    _save = new FileArchitectureInfo("save");
                }
                return _save;
            }
        }

        private static FileArchitectureInfo _mods;
        /// <summary>
        /// 系统模组文件夹
        /// </summary>
        public static FileArchitectureInfo Mods
        {
            get
            {
                if (_mods.IsEmpty)
                {
                    _mods = new FileArchitectureInfo("mods");
                }
                return _mods;
            }
        }
    }
}
