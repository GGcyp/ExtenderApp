

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示文件节点父类的基类，用于实现文件节点树的构建。
    /// </summary>
    /// <typeparam name="FileNode">文件节点类型，泛型参数用于指定文件节点的具体类型。</typeparam>
    public class FileNodeParent : FileNodeParent<FileNode>
    {

    }

    /// <summary>
    /// 表示一个文件或文件夹的父节点
    /// </summary>
    /// <typeparam name="T">文件节点类型，必须继承自FileNode&lt;T&gt;</typeparam>
    public class FileNodeParent<T> where T : FileNode<T>, new()
    {
        /// <summary>
        /// 父节点路径
        /// </summary>
        public string? ParentPath { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        public T ParentNode { get; set; } = new T();

        /// <summary>
        /// 创建文件或文件夹，并更新父节点路径和父节点
        /// </summary>
        /// <param name="parentPath">父节点路径</param>
        /// <param name="node">父节点</param>
        public void CreateFileOrFolder(string parentPath, T? node)
        {
            ParentPath = parentPath;
            ParentNode = node;

            CreateFileOrFolder();
        }

        /// <summary>
        /// 创建文件或文件夹
        /// </summary>
        /// <exception cref="InvalidOperationException">如果父节点路径为空或父节点为空，则抛出此异常</exception>
        public void CreateFileOrFolder()
        {
            if (string.IsNullOrEmpty(ParentPath))
                throw new InvalidOperationException("文件父地址不能为空");
            if (ParentNode == null)
                throw new InvalidOperationException("文件节点数据不能为空");

            ParentNode.CreateFileOrFolder(ParentPath);
        }
    }
}
