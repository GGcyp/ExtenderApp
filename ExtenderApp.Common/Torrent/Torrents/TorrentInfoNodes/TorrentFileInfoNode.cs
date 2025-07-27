using System.IO;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.IO;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// 表示一个与种子文件下载信息节点相关的文件操作节点类。
    /// </summary>
    /// <remarks>
    /// TorrentFileDownInfoNode 类继承自 FIleOperateNode 类，泛型参数为 TorrentFileDownInfoNode。
    /// </remarks>
    public class TorrentFileInfoNode : FIleOperateNode<TorrentFileInfoNode>
    {
        /// <summary>
        /// 获取或设置一个布尔值，表示是否下载。
        /// </summary>
        /// <value>
        /// 如果为 true，则表示需要下载；如果为 false，则表示不需要下载。
        /// </value>
        public bool IsDownload { get; set; } = false;

        /// <summary>
        /// 获取或设置偏移量。
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// 设置种子文件信息
        /// </summary>
        /// <param name="node">文件节点</param>
        public void SetTorrentInfo(FileNode node)
        {
            var databuffer = DataBuffer<long>.GetDataBuffer();
            databuffer.Item1 = 0; // 初始化偏移量为0
            SetTorrentInfo(node, databuffer);
            databuffer.Release();
        }

        private void SetTorrentInfo(FileNode node, DataBuffer<long> dataBuffer)
        {
            Length = node.Length;
            Name = node.Name;
            IsFile = node.IsFile;
            Offset = dataBuffer.Item1;
            dataBuffer.Item1 += Length;

            if (node.HasChildNodes)
            {
                node.LoopChildNodes((fileInfoNode, downInfoNode) =>
                {
                    var node = new TorrentFileInfoNode();
                    node.SetTorrentInfo(fileInfoNode, dataBuffer);
                    Add(node);
                }, this);
            }
        }

        /// <summary>
        /// 创建文件或文件夹。
        /// </summary>
        /// <param name="path">目标路径。</param>
        public override void CreateFileOrFolder(string path)
        {
            // 调用子节点创建方法
            ChildNeedCreate();
            // 调用私有方法创建文件或文件夹
            PrivateCreateFileOrFolder(path);
        }

        /// <summary>
        /// 私有方法：创建文件或文件夹。
        /// </summary>
        /// <param name="path">目标路径。</param>
        private void PrivateCreateFileOrFolder(string path)
        {
            // 如果名称为空，则直接返回
            if (Name == null) return;
            // 拼接目标路径
            string targetPath = Path.Combine(path, Name);
            // 判断是否为文件
            if (IsFile)
            {
                // 如果是文件且需要下载，但文件不存在
                if (IsDownload)
                {
                    CreateFile(targetPath);
                }
            }
            else
            {
                // 如果不是文件且需要下载，但目录不存在
                if (IsDownload)
                {
                    // 创建目录
                    CreateDirectory(targetPath);
                }

                // 遍历所有子节点，递归调用私有方法创建文件或文件夹
                LoopAllChildNodes((n, s) =>
                {
                    n.PrivateCreateFileOrFolder(s);
                }, targetPath);
            }
        }

        public override bool CanCreateFileOperate()
        {
            ChildNeedCreate();
            return base.CanCreateFileOperate() && IsDownload;
        }

        /// <summary>
        /// 子节点是否需要创建文件夹。
        /// </summary>
        public void ChildNeedCreate()
        {
            // 如果是文件，则直接返回
            if (IsFile)
                return;

            // 如果存在子节点
            if (HasChildNodes)
            {
                // 遍历子节点，递归调用子节点创建方法
                LoopChildNodes(t => { t.ChildNeedCreate(); });
            }

            // 遍历所有节点
            for (int i = 0; i < Count; i++)
            {
                var item = this[i];
                // 如果节点是文件且需要下载
                if (item.IsDownload)
                {
                    // 设置当前节点为需要下载
                    IsDownload = true;
                    return;
                }
            }
        }
    }
}
