using System.IO.MemoryMappedFiles;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示“打开文件/映射文件”所需的上下文信息（路径、模式、访问权限），
    /// 并提供便捷的打开方法与到 BCL 类型的转换。
    /// </summary>
    public struct FileOperateInfo : IEquatable<FileOperateInfo>
    {
        /// <summary>
        /// 空对象。
        /// </summary>
        public static FileOperateInfo Empty = new FileOperateInfo();

        /// <summary>
        /// 扩展的文件访问模式（会在需要时转换为 BCL <see cref="FileAccess"/> 或 <see cref="MemoryMappedFileAccess"/>）。
        /// </summary>
        public ExtenderFileAccess FileAccess { get; private set; }

        /// <summary>
        /// 文件打开模式（对应 <see cref="FileMode"/>）。
        /// </summary>
        public FileMode FileMode { get; private set; }

        /// <summary>
        /// 本地文件信息快照（可能指向不存在的文件；此时 <see cref="LocalFileInfo.Exists"/> 为 false）。
        /// </summary>
        public LocalFileInfo LocalFileInfo { get; private set; }

        /// <summary>
        /// 指示当前是否为“空”信息（即 <see cref="LocalFileInfo.IsEmpty"/>）。
        /// </summary>
        public bool IsEmpty => LocalFileInfo.IsEmpty;

        /// <summary>
        /// 使用文件路径初始化。
        /// 仅包装路径与打开参数，不会实际访问磁盘。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="fileMode">文件模式，默认 <see cref="FileMode.Open"/>。</param>
        /// <param name="fileAccess">扩展访问模式，默认 <see cref="ExtenderFileAccess.Read"/>。</param>
        public FileOperateInfo(string filePath, FileMode fileMode = FileMode.Open, ExtenderFileAccess fileAccess = ExtenderFileAccess.ReadWrite)
        {
            //if (!System.IO.File.Exists(filePath))
            //    throw new ArgumentNullException(nameof(filePath));

            FileAccess = fileAccess;
            FileMode = fileMode;
            LocalFileInfo = new LocalFileInfo(filePath);
        }

        /// <summary>
        /// 使用现有的 <see cref="LocalFileInfo"/> 初始化。
        /// </summary>
        /// <param name="localFileInfo">本地文件信息（不能为空）。</param>
        /// <param name="fileMode">文件模式，默认 <see cref="FileMode.Open"/>。</param>
        /// <param name="fileAccess">扩展访问模式，默认 <see cref="ExtenderFileAccess.Read"/>。</param>
        /// <exception cref="ArgumentNullException"><paramref name="localFileInfo"/> 为空时抛出。</exception>
        public FileOperateInfo(LocalFileInfo localFileInfo, FileMode fileMode = FileMode.Open, ExtenderFileAccess fileAccess = ExtenderFileAccess.ReadWrite)
        {
            if (localFileInfo.IsEmpty)
                throw new ArgumentNullException(nameof(localFileInfo));

            FileAccess = fileAccess;
            FileMode = fileMode;
            LocalFileInfo = localFileInfo;
        }

        /// <summary>
        /// 打开文件并返回 <see cref="FileStream"/>。
        /// 调用方负责关闭/释放返回的流。
        /// </summary>
        public FileStream OpenFile()
        {
            // 利用隐式转换将当前实例转换为 BCL FileAccess
            return LocalFileInfo.FileInfo.Open(FileMode, this);
        }

        /// <summary>
        /// 打开文件并在 using 作用域内执行指定操作。
        /// </summary>
        /// <param name="action">对打开的 <see cref="FileStream"/> 执行的操作。</param>
        public void OpenFile(Action<FileStream> action)
        {
            using (FileStream stream = OpenFile())
            {
                action(stream);
            }
        }

        /// <summary>
        /// 若 <see cref="IsEmpty"/> 为 true 则抛出异常。
        /// </summary>
        /// <exception cref="ArgumentNullException">当对象为空时。</exception>
        public void ThrowIsEmpty()
        {
            if (!LocalFileInfo.IsEmpty)
                return;

            throw new ArgumentNullException(LocalFileInfo.FullPath);
        }

        /// <summary>
        /// 若对象为空或文件不存在则抛出异常。
        /// </summary>
        /// <exception cref="ArgumentNullException">当对象为空时。</exception>
        /// <exception cref="FileNotFoundException">当指向的文件不存在时。</exception>
        public void ThrowFileNotFound()
        {
            ThrowIsEmpty();
            LocalFileInfo.ThrowFileNotFound();
        }

        /// <summary>
        /// 检查文件是否存在（等同于 <see cref="LocalFileInfo.Exists"/>）。
        /// </summary>
        public bool Exists()
        {
            return LocalFileInfo.Exists;
        }

        public bool Equals(FileOperateInfo other)
        {
            return LocalFileInfo.Equals(other.LocalFileInfo);
        }

        public override int GetHashCode()
        {
            return LocalFileInfo.GetHashCode();
        }

        /// <summary>
        /// 从路径隐式构造一个读写（OpenOrCreate + ReadWrite）的 <see cref="FileOperateInfo"/>。
        /// </summary>
        public static implicit operator FileOperateInfo(string filePath)
        {
            return new FileOperateInfo(filePath, FileMode.OpenOrCreate, ExtenderFileAccess.ReadWrite);
        }

        /// <summary>
        /// 从 <see cref="LocalFileInfo"/> 隐式构造一个读写（OpenOrCreate + ReadWrite）的 <see cref="FileOperateInfo"/>。
        /// </summary>
        public static implicit operator FileOperateInfo(LocalFileInfo fileInfo)
        {
            return new FileOperateInfo(fileInfo, FileMode.OpenOrCreate, ExtenderFileAccess.ReadWrite);
        }

        /// <summary>
        /// 将当前的扩展访问模式转换为 BCL <see cref="FileAccess"/>（用于打开文件）。
        /// </summary>
        public static implicit operator FileAccess(FileOperateInfo operateInfo)
        {
            return operateInfo.FileAccess.ToFileAccess();
        }

        /// <summary>
        /// 将当前的扩展访问模式转换为 <see cref="MemoryMappedFileAccess"/>（用于内存映射权限）。
        /// </summary>
        public static implicit operator MemoryMappedFileAccess(FileOperateInfo operateInfo)
        {
            return operateInfo.FileAccess.ToMemoryMappedFileAccess();
        }

        /// <summary>
        /// 将 <see cref="FileOperateInfo"/> 隐式转换为已打开的 <see cref="FileStream"/>。
        /// 调用方必须负责释放返回的流。
        /// </summary>
        /// <exception cref="NullReferenceException">当对象为空时抛出。</exception>
        public static explicit operator FileStream(FileOperateInfo operateInfo)
        {
            operateInfo.ThrowFileNotFound();

            return operateInfo.OpenFile();
        }
    }
}
