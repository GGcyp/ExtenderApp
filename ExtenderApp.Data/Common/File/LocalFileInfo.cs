
namespace ExtenderApp.Data
{
    /// <summary>
    /// 本地文件信息结构体
    /// </summary>
    public struct LocalFileInfo
    {
        /// <summary>
        /// 存储文件信息的FileInfo对象
        /// </summary>
        public FileInfo FileInfo { get; }

        /// <summary>
        /// 文件的完整路径
        /// </summary>
        public string DataPath => FileInfo.FullName;

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
        }

        /// <summary>
        /// 初始化LocalDataInfo结构体
        /// </summary>
        /// <param name="fileInfo">FileInfo对象</param>
        public LocalFileInfo(string filePath)
        {
            FileInfo = new FileInfo(filePath);
        }
    }
}
