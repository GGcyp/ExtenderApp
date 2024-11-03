using MainApp.Common.File;

namespace MainApp.Common
{
    /// <summary>
    /// 数据层，文件信息及需要的操作
    /// </summary>
    public struct FileInfoData
    {
        public FileAccess FileAccess { get; private set; }
        public FileMode FileMode { get; private set; }
        public FileArchitectureInfo ArchitectureInfo { get; private set; }

        public string FileName { get; private set; }
        public string Path { get; private set; }

        public FileExtensionType Extension { get; private set; }

        public bool Exists => Info.Exists;

        private FileInfo fileInfo;
        public FileInfo Info
        {
            get
            {
                if(fileInfo == null)
                {
                    if (string.IsNullOrEmpty(FileName)) return null;
                    fileInfo = new FileInfo(Path);
                }
                return fileInfo;
            }
        }

        public bool isEmpty => string.IsNullOrEmpty(Path);

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
        }

        public FileInfoData(string path, FileAccess fileAccess = FileAccess.Read, FileMode fileMode = FileMode.Open)
        {
            if(!global::System.IO.Path.IsPathRooted(path))
            {
                throw new ArgumentException("path value cannot be null for FileInfoData");
            }
            Path = path;
            FileName = global::System.IO.Path.GetFileName(path);
            Extension = new FileExtensionType(global::System.IO.Path.GetExtension(path));

            FileAccess = fileAccess;
            FileMode = fileMode;
            ArchitectureInfo = FileArchitectureInfo.Empty;
        }
    }
}
