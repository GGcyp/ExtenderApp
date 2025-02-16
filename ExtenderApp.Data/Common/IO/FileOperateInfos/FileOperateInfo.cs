namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示文件操作信息的结构体，实现了IEquatable<FileOperateInfo>接口
    /// </summary>
    public struct FileOperateInfo : IEquatable<FileOperateInfo>
    {
        /// <summary>
        /// 获取一个空的FileOperate实例
        /// </summary>
        /// <returns>返回一个空的FileOperate实例</returns>
        public static FileOperateInfo Empty = new FileOperateInfo();

        /// <summary>
        /// 文件访问权限
        /// </summary>
        public FileAccess FileAccess { get; private set; }

        /// <summary>
        /// 文件模式
        /// </summary>
        public FileMode FileMode { get; private set; }

        /// <summary>
        /// 本地文件信息
        /// </summary>
        public LocalFileInfo LocalFileInfo { get; private set; }

        /// <summary>
        /// 判断当前对象是否为空
        /// </summary>
        /// <returns>如果当前对象为空，则返回 true；否则返回 false</returns>
        public bool IsEmpty => LocalFileInfo.IsEmpty;

        /// <summary>
        /// 初始化 FileOperateInfo 类的新实例。
        /// </summary>
        /// <param name="filePath">文件的路径。</param>
        /// <param name="fileMode">文件模式，默认为 FileMode.Open。</param>
        /// <param name="fileAccess">文件访问权限，默认为 FileAccess.Read。</param>
        /// <exception cref="ArgumentNullException">当文件路径为空时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件已存在时抛出。</exception>
        public FileOperateInfo(string filePath, FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.Read)
        {
            //if (!System.IO.File.Exists(filePath))
            //    throw new ArgumentNullException(nameof(filePath));


            FileAccess = fileAccess;
            FileMode = fileMode;
            LocalFileInfo = new LocalFileInfo(filePath);
        }

        /// <summary>
        /// 初始化 FileOperateInfo 结构体
        /// </summary>
        /// <param name="fileAccess">文件访问权限</param>
        /// <param name="fileMode">文件模式</param>
        /// <param name="localFileInfo">本地文件信息</param>
        public FileOperateInfo(LocalFileInfo localFileInfo, FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.Read)
        {
            if (localFileInfo.IsEmpty)
                throw new ArgumentNullException(nameof(localFileInfo));

            FileAccess = fileAccess;
            FileMode = fileMode;
            LocalFileInfo = localFileInfo;
        }

        /// <summary>
        /// 打开文件并返回一个 FileStream 对象。
        /// </summary>
        /// <returns>返回一个 FileStream 对象。</returns>
        public FileStream OpenFile()
        {
            return LocalFileInfo.FileInfo.Open(FileMode, FileAccess);
        }

        /// <summary>
        /// 打开文件并执行指定的操作。
        /// </summary>
        /// <param name="action">要执行的操作，该操作需要一个FileStream参数。</param>
        public void OpenFile(Action<FileStream> action)
        {
            using (FileStream stream = OpenFile())
            {
                action(stream);
            }
        }

        /// <summary>
        /// 打开文件并调用指定的函数处理文件流。
        /// </summary>
        /// <typeparam name="T">返回值的类型。</typeparam>
        /// <param name="func">处理文件流的函数。</param>
        /// <returns>处理后的结果。</returns>
        public T OpenFile<T>(Func<FileStream, T> func)
        {
            T result;
            using (FileStream stream = OpenFile())
            {
                result = func(stream);
            }
            return result;
        }

        /// <summary>
        /// 抛出异常，如果 LocalFileInfo 为空。
        /// </summary>
        /// <exception cref="ArgumentNullException">当 LocalFileInfo 为空时抛出。</exception>
        public void ThrowIsEmpty()
        {
            if (!LocalFileInfo.IsEmpty)
                return;

            throw new ArgumentNullException(LocalFileInfo.FilePath);
        }

        /// <summary>
        /// 抛出异常，如果 LocalFileInfo 为空或文件不存在。
        /// </summary>
        /// <exception cref="ArgumentNullException">当 LocalFileInfo 为空时抛出。</exception>
        /// <exception cref="FileNotFoundException">当 LocalFileInfo 指向的文件不存在时抛出。</exception>
        public void ThrowFileNotFound()
        {
            ThrowIsEmpty();

            if (LocalFileInfo.Exists)
                return;

            throw new FileNotFoundException(LocalFileInfo.FilePath);
        }

        public bool Equals(FileOperateInfo other)
        {
            return LocalFileInfo.Equals(other.LocalFileInfo);
        }

        public override int GetHashCode()
        {
            return LocalFileInfo.GetHashCode();
        }
    }
}
