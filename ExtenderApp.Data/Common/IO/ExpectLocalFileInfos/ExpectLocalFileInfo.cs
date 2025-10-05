namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示期望的本地文件信息的结构体
    /// </summary>
    public struct ExpectLocalFileInfo : IEquatable<ExpectLocalFileInfo>
    {
        public static ExpectLocalFileInfo Empty = new ExpectLocalFileInfo();

        /// <summary>
        /// 文件夹路径
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 判断文件夹路径或文件名是否为空。
        /// </summary>
        /// <returns>如果文件夹路径或文件名任意一个为空，则返回true；否则返回false。</returns>
        public bool IsEmpty => string.IsNullOrEmpty(FolderPath) || string.IsNullOrEmpty(FileName);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="fileName">文件名</param>
        public ExpectLocalFileInfo(string folderPath, string fileName)
        {
            if (!Directory.Exists(folderPath))
                throw new ArgumentNullException(nameof(folderPath));

            FolderPath = folderPath;
            FileName = fileName;
        }

        /// <summary>
        /// 创建本地文件信息对象
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        /// <returns>返回创建的本地文件信息对象</returns>
        public LocalFileInfo CreatLocalFileInfo(string extension)
        {
            return new LocalFileInfo(Path.Combine(FolderPath, string.Concat(FileName, extension)));
        }

        /// <summary>
        /// 创建一个文件操作对象。
        /// </summary>
        /// <param name="extension">文件的扩展名。</param>
        /// <param name="fileMode">文件打开模式，默认为FileMode.Open。</param>
        /// <param name="fileAccess">文件访问模式，默认为FileAccess.Read。</param>
        /// <returns>返回一个FileOperate对象。</returns>
        public FileOperateInfo CreateFileOperate(string extension, FileMode fileMode = FileMode.Open, ExtenderFileAccess fileAccess = ExtenderFileAccess.Read)
        {
            return CreatLocalFileInfo(extension).CreateFileOperate(fileMode, fileAccess);
        }

        /// <summary>
        /// 创建一个文件写入操作对象。
        /// </summary>
        /// <param name="extension">文件的扩展名。</param>
        /// <returns>返回一个FileOperate对象，用于执行文件写入操作。</returns>
        public FileOperateInfo CreateReadWriteOperate(string extension)
        {
            return CreateFileOperate(extension, FileMode.OpenOrCreate, ExtenderFileAccess.ReadWrite);
        }

        /// <summary>
        /// 判断两个ExpectLocalFileInfo对象是否相等
        /// </summary>
        /// <param name="other">要比较的另一个ExpectLocalFileInfo对象</param>
        /// <returns>如果两个对象相等，则返回true；否则返回false</returns>
        public bool Equals(ExpectLocalFileInfo other)
        {
            return FolderPath.Equals(other.FolderPath) && FileName.Equals(other.FileName);
        }

        public override int GetHashCode()
        {
            return FolderPath.GetHashCode() ^ FileName.GetHashCode();
        }
    }
}
