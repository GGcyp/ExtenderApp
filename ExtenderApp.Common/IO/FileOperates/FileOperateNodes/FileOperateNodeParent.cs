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
    }
}
