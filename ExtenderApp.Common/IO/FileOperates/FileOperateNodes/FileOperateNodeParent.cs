using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// FileOperateNodeParent 类，继承自 FileNodeParent 类，用于处理文件操作节点
    /// </summary>
    /// <typeparam name="T">文件操作节点类型，需要继承自 FIleOperateNode 并实现其泛型参数为自身的构造函数</typeparam>
    public class FileOperateNodeParent<T> : FileNodeParent<T> where T : FIleOperateNode<T>, new()
    {
        /// <summary>
        /// 创建所有文件操作节点
        /// </summary>
        /// <param name="provider">文件操作提供者</param>
        /// <exception cref="ArgumentNullException">当 provider 为空时抛出</exception>
        /// <exception cref="ArgumentNullException">当 ParentNode 为空或 ParentNode.Name 为空时抛出</exception>
        /// <exception cref="ArgumentNullException">当 ParentPath 为空时抛出</exception>
        public void CreateAllFileOperate(IFileOperateProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider), "文件操作提供者不可为空");
            if (ParentNode == null || string.IsNullOrEmpty(ParentNode.Name))
                throw new ArgumentNullException(nameof(ParentNode), "文件根节点不能为空");
            if (string.IsNullOrEmpty(ParentPath))
                throw new ArgumentNullException(nameof(ParentPath), "文件父地址不能为空");
            if (!ParentNode.CanCreateFileOperate())
                return;

            string path = Path.Combine(ParentPath, ParentNode.Name);
            if (ParentNode.IsFile)
            {
                ParentNode.FileOperate = provider.GetOperate(path);
                return;
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    ParentNode.Info = Directory.CreateDirectory(path);
                }
            }

            ParentNode.LoopAllChildNodes((n, p) =>
            {
                if (!n.CanCreateFileOperate())
                    return;

                var path = Path.Combine(p, n.Name!);
                if (n.IsFile)
                {
                    n.FileOperate = provider.GetOperate(path);
                    n.Info = n.FileOperate.Info.FileInfo;
                }
                else if (!Directory.Exists(path))
                {
                    n.Info = Directory.CreateDirectory(path);
                }
            }, path);
        }

        /// <summary>
        /// 根据节点创建文件或目录操作
        /// </summary>
        /// <param name="provider">文件操作提供者</param>
        /// <param name="node">节点对象</param>
        /// <exception cref="ArgumentNullException">如果节点或文件操作提供者为空，则抛出此异常</exception>
        public void CreateFileOperate(IFileOperateProvider provider, T node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node), "文件节点不能为空");
            if (provider == null)
                throw new ArgumentNullException(nameof(provider), "文件操作提供者不可为空");
            if (node.FileOperate != null)
                return;

            string path = GetNodePath(node);
            if (string.IsNullOrEmpty(path))
                throw new InvalidOperationException("无法获取节点的完整路径");

            if (node.IsFile)
            {
                node.FileOperate = provider.GetOperate(path);
                node.Info = node.FileOperate.Info.FileInfo;
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    node.Info = Directory.CreateDirectory(path);
                }
            }
        }

        /// <summary>
        /// 获取节点的完整路径（从子节点到根节点拼接）
        /// </summary>
        /// <param name="node">子节点</param>
        /// <returns>节点的完整路径（包含ParentPath）</returns>
        public string? GetNodePath(T node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node), "文件节点不能为空");
            if (string.IsNullOrEmpty(ParentPath))
                throw new InvalidOperationException("文件父路径ParentPath不能为空");
            if (!node.CanCreateFileOperate())
                return string.Empty;

            // 收集从子节点到根节点的名称（逆序，子节点在前，根节点在后）
            Stack<string> nodeNames = new Stack<string>();
            T current = node;

            while (current != null)
            {
                if (string.IsNullOrEmpty(current.Name))
                    throw new InvalidOperationException("节点名称不能为空");

                nodeNames.Push(current.Name); // 入栈（后续弹出时顺序为根→子）
                current = current.ParentNode; // 向上遍历父节点
            }

            // 拼接路径：ParentPath + 根节点 + 中间节点 + 子节点
            StringBuilder pathBuilder = new StringBuilder(ParentPath);
            while (nodeNames.Count > 0)
            {
                pathBuilder.Append(System.IO.Path.DirectorySeparatorChar);
                pathBuilder.Append(nodeNames.Pop());
            }

            // 处理路径分隔符重复问题（例如ParentPath已包含结尾分隔符）
            return System.IO.Path.GetFullPath(pathBuilder.ToString());
        }

        /// <summary>
        /// 创建节点对应的目录（从子节点到根节点的完整路径）
        /// </summary>
        /// <param name="node">子节点</param>
        /// <returns>创建的目录完整路径</returns>
        public string? CreateNodeDirectory(T node)
        {
            string? fullPath = GetNodePath(node);
            if (string.IsNullOrEmpty(fullPath))
            {
                return fullPath;
            }

            // 如果是文件节点，取其所在目录路径（去掉文件名）
            if (node.IsFile)
            {
                fullPath = System.IO.Path.GetDirectoryName(fullPath)
                           ?? throw new InvalidOperationException("无法获取文件所在目录");
            }

            // 创建目录（如果不存在）
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return fullPath;
        }
    }
}
