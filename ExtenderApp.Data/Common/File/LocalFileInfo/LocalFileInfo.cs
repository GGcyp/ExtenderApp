
namespace ExtenderApp.Data
{
    /// <summary>
    /// 本地文件信息结构体
    /// </summary>
    public struct LocalFileInfo : IEquatable<LocalFileInfo>
    {
        /// <summary>
        /// 存储文件信息的FileInfo对象
        /// </summary>
        public FileInfo FileInfo { get; }

        /// <summary>
        /// 文件的完整路径
        /// </summary>
        public string FilePath => FileInfo.FullName;

        /// <summary>
        /// 文件名(包含扩展名)
        /// </summary>
        public string FileName => FileInfo.Name;

        /// <summary>
        /// 获取文件所在的目录名称。
        /// </summary>
        /// <returns>返回文件所在的目录名称。</returns>
        public string DirectoryName => FileInfo.DirectoryName;

        /// <summary>
        /// 获取文件的名称。(不包含扩展名)
        /// </summary>
        /// <returns>返回文件的名称。</returns>
        public string FileNameWithoutExtension { get; }

        /// <summary>
        /// 文件大小（单位：字节）
        /// </summary>
        public long FileSize => FileInfo.Length;

        /// <summary>
        /// 文件的创建时间
        /// </summary>
        public DateTime CreationTime => FileInfo.CreationTime;

        /// <summary>
        /// 文件的最后修改时间
        /// </summary>
        public DateTime LastModifiedTime => FileInfo.LastWriteTime;

        /// <summary>
        /// 文件的扩展名
        /// </summary>
        public string Extension => FileInfo.Extension;

        /// <summary>
        /// 文件是否只读属性
        /// </summary>
        public bool IsReadOnly => FileInfo.IsReadOnly;

        /// <summary>
        /// 文件是否存在
        /// </summary>
        public bool Exists => FileInfo.Exists;


        /// <summary>
        /// 判断这个本地文件信息结构体是否为空
        /// </summary>
        public bool IsEmpty => FileInfo is null;

        /// <summary>
        /// 初始化LocalDataInfo结构体
        /// </summary>
        /// <param name="fileInfo">FileInfo对象</param>
        public LocalFileInfo(FileInfo fileInfo)
        {
            FileInfo = fileInfo;

            if (!Directory.Exists(DirectoryName))
                throw new ArgumentNullException(nameof(FilePath));

            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
        }

        /// <summary>
        /// 初始化LocalDataInfo结构体
        /// </summary>
        /// <param name="fileInfo">FileInfo对象</param>
        public LocalFileInfo(string filePath) : this(new FileInfo(filePath))
        {

        }

        /// <summary>
        /// 获取修改文件扩展名后的地址
        /// </summary>
        /// <param name="newExtension">新的扩展名</param>
        /// <returns>修改扩展名后的本地文件信息结构体</returns>
        public LocalFileInfo GetFilePathWithNewExtension(string newExtension)
        {
            return new LocalFileInfo(Path.ChangeExtension(FilePath, newExtension));
        }

        /// <summary>
        /// 创建一个新的 FileOperate 实例。
        /// </summary>
        /// <param name="fileMode">文件的打开模式，默认为 FileMode.Open。</param>
        /// <param name="fileAccess">文件的访问权限，默认为 FileAccess.Read。</param>
        /// <returns>返回一个新的 FileOperate 实例。</returns>
        public FileOperate CreateFileOperate(FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.Read)
        {
            return new FileOperate(FilePath, fileMode, fileAccess);
        }

        /// <summary>
        /// 修改文件的扩展名
        /// </summary>
        /// <param name="newExtension">新的文件扩展名</param>
        /// <returns>修改扩展名后的文件信息</returns>
        public LocalFileInfo ChangeFileExtension(string newExtension)
        {
            var newFileInfo = GetFilePathWithNewExtension(newExtension);
            return newFileInfo;
        }

        /// <summary>
        /// 在文件名后追加指定字符串。
        /// </summary>
        /// <param name="append">要追加的字符串，默认为 "_1"。</param>
        /// <returns>追加字符串后的文件信息。</returns>
        public LocalFileInfo AppendFileName(string append = "_1")
        {
            return ChangeFileName(string.Concat(FileNameWithoutExtension, append));
        }

        /// <summary>
        /// 更改文件名
        /// </summary>
        /// <param name="newName">新的文件名（不包含扩展名）</param>
        /// <returns>更改文件名后的本地文件信息</returns>
        public LocalFileInfo ChangeFileName(string newName)
        {
            return new LocalFileInfo(Path.Combine(DirectoryName, string.Concat(newName, Extension)));
        }

        /// <summary>
        /// 创建一个本地文件信息期望对象。
        /// </summary>
        /// <returns>返回一个新的ExpectLocalFileInfo对象。</returns>
        public ExpectLocalFileInfo CreateExpectLocalFileInfo()
        {
            return new ExpectLocalFileInfo(DirectoryName, FileName);
        }

        /// <summary>
        /// 更新文件信息。
        /// </summary>
        public void UpdateFileInfo()
        {
            FileInfo.Refresh();
        }

        public bool Equals(LocalFileInfo other)
        {
            return FileInfo.Equals(other.FileInfo);
        }

        public override int GetHashCode()
        {
            return FilePath.GetHashCode();
        }
    }
}
