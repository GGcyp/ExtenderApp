namespace ExtenderApp.Common
{
    /// <summary>
    /// 文件信息数据类
    /// </summary>
    public struct FileInfoData
    {
        /// <summary>
        /// 文件访问权限
        /// </summary>
        public FileAccess FileAccess { get; private set; }

        /// <summary>
        /// 文件模式
        /// </summary>
        public FileMode FileMode { get; private set; }

        /// <summary>
        /// 文件架构信息
        /// </summary>
        public FileArchitectureInfo ArchitectureInfo { get; private set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 文件扩展名类型
        /// </summary>
        public FileExtensionType Extension { get; private set; }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        public bool Exists => FileInfo.Exists;

        /// <summary>
        /// 文件信息
        /// </summary>
        public FileInfo FileInfo { get; }

        /// <summary>
        /// 判断文件路径是否为空
        /// </summary>
        public bool isEmpty => string.IsNullOrEmpty(Path);

        /// <summary>
        /// 通过文件名、文件扩展名类型、文件架构信息和文件访问权限初始化 FileInfoData 实例
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="fileExtensionType">文件扩展名类型</param>
        /// <param name="info">文件架构信息</param>
        /// <param name="fileAccess">文件访问权限，默认为读权限</param>
        /// <param name="fileMode">文件模式，默认为打开模式</param>
        public FileInfoData(string fileName, FileExtensionType fileExtensionType, FileArchitectureInfo info, FileAccess fileAccess = FileAccess.Read, FileMode fileMode = FileMode.Open)
        {
            if (string.IsNullOrEmpty(fileName) || fileExtensionType.IsEmpty)
            {
                throw new ArgumentNullException("the fileName and fileSuffix cannot be null");
            }

            if (fileAccess != FileAccess.Write && info.IsEmpty)
            {
                throw new ArgumentException($"Must have a complete path name：{fileName}");
            }

            FileAccess = fileAccess;
            FileMode = fileMode;
            ArchitectureInfo = info;
            FileName = fileName;
            Extension = fileExtensionType;

            Path = info.GetPath(fileName, fileExtensionType);

            FileInfo = new FileInfo(Path);
        }

        /// <summary>
        /// 通过文件路径、文件访问权限和文件模式初始化 FileInfoData 实例
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="fileAccess">文件访问权限，默认为读权限</param>
        /// <param name="fileMode">文件模式，默认为打开模式</param>
        public FileInfoData(string path, FileAccess fileAccess = FileAccess.Read, FileMode fileMode = FileMode.Open)
        {
            if (!global::System.IO.Path.IsPathRooted(path))
            {
                throw new ArgumentException("path value cannot be null for FileInfoData");
            }
            Path = path;
            FileName = global::System.IO.Path.GetFileName(path);
            Extension = new FileExtensionType(global::System.IO.Path.GetExtension(path));

            FileAccess = fileAccess;
            FileMode = fileMode;
            ArchitectureInfo = FileArchitectureInfo.Empty;

            FileInfo = new FileInfo(Path);
        }
    }
}
