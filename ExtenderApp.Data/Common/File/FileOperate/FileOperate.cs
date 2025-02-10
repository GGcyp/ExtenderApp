namespace ExtenderApp.Data
{
    /// <summary>
    /// 文件操作结构体
    /// </summary>
    public struct FileOperate : IEquatable<FileOperate>
    {
        /// <summary>
        /// 获取一个空的FileOperate实例
        /// </summary>
        /// <returns>返回一个空的FileOperate实例</returns>
        public static FileOperate Empty = new FileOperate();

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
        /// 初始化 FileOperate 类的新实例。
        /// </summary>
        /// <param name="filePath">文件的路径。</param>
        /// <param name="fileMode">文件模式，默认为 FileMode.Open。</param>
        /// <param name="fileAccess">文件访问权限，默认为 FileAccess.Read。</param>
        /// <exception cref="ArgumentNullException">当文件路径为空时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件已存在时抛出。</exception>
        public FileOperate(string filePath, FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.Read)
        {
            //if (!System.IO.File.Exists(filePath))
            //    throw new ArgumentNullException(nameof(filePath));


            FileAccess = fileAccess;
            FileMode = fileMode;
            LocalFileInfo = new LocalFileInfo(filePath);
        }

        /// <summary>
        /// 初始化 FileOperate 结构体
        /// </summary>
        /// <param name="fileAccess">文件访问权限</param>
        /// <param name="fileMode">文件模式</param>
        /// <param name="localFileInfo">本地文件信息</param>
        public FileOperate(LocalFileInfo localFileInfo, FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.Read)
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

        public bool Equals(FileOperate other)
        {
            return LocalFileInfo.Equals(other.LocalFileInfo);
        }

        public override int GetHashCode()
        {
            return LocalFileInfo.GetHashCode();
        }
    }
}
