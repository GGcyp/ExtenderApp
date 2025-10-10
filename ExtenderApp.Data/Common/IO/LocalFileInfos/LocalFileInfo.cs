using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个本地文件的元信息快照与常用路径操作辅助。
    /// </summary>
    /// <remarks>
    /// - 内部基于 <see cref="FileInfo"/>，可指向尚不存在的文件（此时 <see cref="Exists"/> 为 false）。<br/>
    /// - 读取依赖磁盘状态的属性（如长度、时间戳）前，可调用 <see cref="UpdateFileInfo"/> 刷新缓存。<br/>
    /// - 本类型不负责创建/删除文件，仅提供路径与信息访问。
    /// </remarks>
    public struct LocalFileInfo : IEquatable<LocalFileInfo>
    {
        /// <summary>
        /// 空对象。
        /// </summary>
        public static readonly LocalFileInfo Empty = new LocalFileInfo();

        /// <summary>
        /// 文件完整路径的哈希码缓存。
        /// </summary>
        public HashValue Hash { get; }

        /// <summary>
        /// 底层 <see cref="System.IO.FileInfo"/> 实例（可能指向不存在的文件）。
        /// </summary>
        public FileInfo FileInfo { get; }

        /// <summary>
        /// 文件完整路径。
        /// </summary>
        public string FullPath => FileInfo.FullName;

        /// <summary>
        /// 文件名（包含扩展名）。
        /// </summary>
        public string FileName => FileInfo.Name;

        /// <summary>
        /// 文件所在目录的完整路径。
        /// </summary>
        public string? DirectoryName => FileInfo.DirectoryName;

        /// <summary>
        /// 不带扩展名的文件名。
        /// </summary>
        public string FileNameWithoutExtension { get; }

        /// <summary>
        /// 文件大小（字节）。文件不存在时访问会抛出异常。
        /// 等同于 <see cref="Length"/>。
        /// </summary>
        public long FileSize => FileInfo.Length;

        /// <summary>
        /// 文件创建时间。
        /// </summary>
        public DateTime CreationTime => FileInfo.CreationTime;

        /// <summary>
        /// 文件最后写入时间。
        /// </summary>
        public DateTime LastModifiedTime => FileInfo.LastWriteTime;

        /// <summary>
        /// 文件扩展名（含点）。
        /// </summary>
        public string Extension => FileInfo.Extension;

        /// <summary>
        /// 是否只读文件属性。
        /// </summary>
        public bool IsReadOnly => FileInfo.IsReadOnly;

        /// <summary>
        /// 文件当前是否存在于磁盘。
        /// </summary>
        public bool Exists => FileInfo.Exists;

        /// <summary>
        /// 文件长度（字节）。文件不存在时访问会抛出异常。
        /// </summary>
        public long Length => FileInfo.Length;

        /// <summary>
        /// 指示当前是否为“空”信息（<see cref="FileInfo"/> 为 null）。
        /// </summary>
        public bool IsEmpty => FileInfo is null;

        /// <summary>
        /// 使用现有的 <see cref="FileInfo"/> 初始化。
        /// 要求目录必须已存在；否则抛出异常。
        /// </summary>
        /// <param name="fileInfo">文件信息对象。</param>
        /// <exception cref="ArgumentNullException">当目录不存在时抛出（参数名为 <paramref name="FullPath"/>）。</exception>
        public LocalFileInfo(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            Hash = GetHash(FullPath);
        }

        /// <summary>
        /// 使用文件路径初始化。
        /// 仅包装路径，不会创建文件；目录需已存在。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        public LocalFileInfo(string filePath) : this(new FileInfo(filePath))
        {
        }

        /// <summary>
        /// 基于当前路径替换扩展名，返回新的 <see cref="LocalFileInfo"/>。
        /// 不会修改磁盘上的文件。
        /// </summary>
        /// <param name="newExtension">新的扩展名（可带“.”）。</param>
        public LocalFileInfo GetFilePathWithNewExtension(string newExtension)
        {
            return new LocalFileInfo(Path.ChangeExtension(FullPath, newExtension));
        }

        /// <summary>
        /// 创建一个文件操作信息 <see cref="FileOperateInfo"/>。
        /// 不会实际打开或创建文件。
        /// </summary>
        /// <param name="fileMode">文件打开模式，默认 <see cref="FileMode.Open"/>。</param>
        /// <param name="fileAccess">访问权限，默认 <see cref="FileAccess.Read"/>。</param>
        public FileOperateInfo CreateFileOperate(FileMode fileMode = FileMode.Open, ExtenderFileAccess fileAccess = ExtenderFileAccess.ReadWrite)
        {
            return new FileOperateInfo(this, fileMode, fileAccess);
        }

        /// <summary>
        /// 创建一个读写模式的 <see cref="FileOperateInfo"/>（OpenOrCreate + ReadWrite）。
        /// </summary>
        public FileOperateInfo CreateReadWriteOperate()
        {
            return CreateFileOperate(FileMode.OpenOrCreate, ExtenderFileAccess.ReadWrite);
        }

        /// <summary>
        /// 将扩展名修改为指定值，返回新的 <see cref="LocalFileInfo"/>。
        /// </summary>
        /// <param name="newExtension">新的扩展名。</param>
        public LocalFileInfo ChangeFileExtension(string newExtension)
        {
            var newFileInfo = GetFilePathWithNewExtension(newExtension);
            return newFileInfo;
        }

        /// <summary>
        /// 在不改变扩展名的前提下，向文件名末尾追加指定字符串。
        /// </summary>
        /// <param name="append">要追加的字符串，默认 "_1"。</param>
        public LocalFileInfo AppendFileName(string append = "_1")
        {
            return ChangeFileName(string.Concat(FileNameWithoutExtension, append));
        }

        /// <summary>
        /// 修改文件名（不含扩展名），保留原扩展名与目录。
        /// </summary>
        /// <param name="newName">新的文件名（不含扩展名）。</param>
        public LocalFileInfo ChangeFileName(string newName)
        {
            return new LocalFileInfo(Path.Combine(DirectoryName, string.Concat(newName, Extension)));
        }

        /// <summary>
        /// 基于当前目录与不带扩展名的文件名，创建“期望的”本地文件信息对象。
        /// </summary>
        public ExpectLocalFileInfo CreateExpectLocalFileInfo()
        {
            return new ExpectLocalFileInfo(DirectoryName, FileNameWithoutExtension);
        }

        /// <summary>
        /// 刷新底层 <see cref="FileInfo"/> 的缓存（例如长度、时间戳）。
        /// </summary>
        public void UpdateFileInfo()
        {
            FileInfo.Refresh();
        }

        /// <summary>
        /// 如果文件不存在则抛出 <see cref="FileNotFoundException"/>。
        /// </summary>
        /// <exception cref="InvalidOperationException">当 <see cref="IsEmpty"/> 为 true 时。</exception>
        /// <exception cref="FileNotFoundException">当 <see cref="Exists"/> 为 false 时。</exception>
        public void ThrowFileNotFound()
        {
            if (IsEmpty)
                throw new InvalidOperationException("文件信息为空");

            if (!Exists)
                throw new FileNotFoundException("文件不存在", FullPath);
        }

        /// <summary>
        /// 基于完整路径判断与另一个对象是否相等（区分大小写规则由系统决定）。
        /// </summary>
        public bool Equals(LocalFileInfo other)
        {
            return FullPath.Equals(other.FullPath);
        }

        private static HashValue GetHash(string text)
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(text ?? string.Empty);
            byte[] hash = SHA256.HashData(utf8);
            return HashValue.SHA256ComputeHash(hash);
        }

        public static bool operator ==(LocalFileInfo left, LocalFileInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LocalFileInfo left, LocalFileInfo right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 从路径隐式转换为 <see cref="LocalFileInfo"/>。
        /// </summary>
        public static implicit operator LocalFileInfo(string filePath)
        {
            return new LocalFileInfo(filePath);
        }

        /// <summary>
        /// 隐式转换为完整路径字符串。若为空对象返回空字符串。
        /// </summary>
        public static implicit operator string(LocalFileInfo localFileInfo)
        {
            return localFileInfo.IsEmpty ? string.Empty : localFileInfo.FullPath;
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is LocalFileInfo && Equals((LocalFileInfo)obj);
        }
    }
}
