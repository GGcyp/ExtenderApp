using System.Collections;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示文件节点的基类，继承自泛型类FileNode<T>，其中T是FileNode<T>本身。
    /// </summary>
    public class FileNode : FileNode<FileNode>
    {

    }

    /// <summary>
    /// 表示文件节点的泛型类，继承自泛型类Node<T>。
    /// </summary>
    /// <typeparam name="T">表示FileNode<T>的类型。</typeparam>
    public class FileNode<T> : Node<T> where T : FileNode<T>
    {
        /// <summary>
        /// 获取或设置文件的名称。
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 获取或设置文件的大小（以字节为单位）。
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示当前节点是否表示一个文件。
        /// </summary>
        public bool IsFile { get; set; } = false;

        /// <summary>
        /// 获取或设置文件系统信息。
        /// </summary>
        /// <value>文件系统信息，可能为null。</value>
        public FileSystemInfo? Info { get; set; }

        /// <summary>
        /// 尝试获取本地文件信息。
        /// </summary>
        /// <param name="info">输出参数，用于接收本地文件信息。</param>
        /// <returns>如果成功获取到本地文件信息，则返回true；否则返回false。</returns>
        public bool TryGetLocalInfo(out LocalFileInfo info)
        {
            info = LocalFileInfo.Empty;
            if (IsFile && Info != null)
            {
                info = new LocalFileInfo((FileInfo)Info);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据当前节点创建文件或文件夹。
        /// </summary>
        /// <param name="path">要创建文件或文件夹的父目录路径。</param>
        /// <remarks>
        /// 如果当前节点是文件，则在指定路径下创建文件；如果当前节点是文件夹，则在指定路径下创建文件夹，并递归创建其子节点。
        /// </remarks>
        public virtual void CreateFileOrFolder(string path)
        {
            if (Name == null) return;
            string targetPath = Path.Combine(path, Name);

            if (IsFile)
            {
                if (!CanCreateFile(targetPath))
                    return;
                CreateFile(targetPath);
            }
            else
            {
                if (CanCreateDirectory(targetPath))
                    return;
                CreateDirectory(targetPath);
                LoopAllChildNodes((n, s) => n.CreateFileOrFolder(s), targetPath);
            }
        }

        /// <summary>
        /// 在指定路径下创建文件。
        /// </summary>
        /// <param name="path">要创建文件的路径。</param>
        protected void CreateFile(string path)
        {
            if (!File.Exists(path))
            {
                using (var stream = File.Create(path))
                {
                    Length = Length <= 0 ? 1024 : Length;
                    stream.SetLength(Length);
                }
            }
            Info = new FileInfo(path);
        }

        /// <summary>
        /// 判断是否可以在指定路径下创建文件。
        /// </summary>
        /// <param name="path">要创建文件的路径。</param>
        /// <returns>如果可以在指定路径下创建文件，则返回 true；否则返回 false。</returns>
        protected virtual bool CanCreateFile(string path)
        {
            return true;
        }

        /// <summary>
        /// 在指定路径下创建文件夹。
        /// </summary>
        /// <param name="path">要创建文件夹的路径。</param>
        protected void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Info = Directory.CreateDirectory(path);
                return;
            }
            Info = new DirectoryInfo(path);
        }

        /// <summary>
        /// 判断是否可以在指定路径下创建文件夹。
        /// </summary>
        /// <param name="path">要创建文件夹的路径。</param>
        /// <returns>如果可以在指定路径下创建文件夹，则返回 true；否则返回 false。</returns>
        protected virtual bool CanCreateDirectory(string path)
        {
            return true;
        }
    }
}
