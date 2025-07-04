using System.IO;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示一个Torrent文件信息节点的类。
    /// </summary>
    public class TorrentFileInfoNode : Node<TorrentFileInfoNode>, IResettable
    {
        private static readonly ObjectPool<TorrentFileInfoNode> _pool
            = ObjectPool.CreateDefaultPool<TorrentFileInfoNode>();

        public static TorrentFileInfoNode Get() => _pool.Get();
        public static void Release(TorrentFileInfoNode node) => _pool.Release(node);

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
        /// 获取或设置一个布尔值，表示是否下载。
        /// </summary>
        /// <value>
        /// 如果为 true，则表示需要下载；如果为 false，则表示不需要下载。
        /// </value>
        public bool IsDownload { get; set; } = false;

        /// <summary>
        /// 获取或设置文件操作接口
        /// </summary>
        /// <value>文件操作接口</value>
        public IFileOperate? fileOperate { get; set; }

        /// <summary>
        /// 释放当前节点及其子节点的资源。
        /// </summary>
        public void Release()
        {
            if (HasChildNodes)
            {
                LoopChildNodes(n =>
                {
                    n.Release();
                });
                Clear();
            }
            TryReset();
        }

        public bool TryReset()
        {
            Name = string.Empty;
            Length = 0;
            fileOperate = null;
            return true;
        }

        /// <summary>
        /// 创建文件或文件夹。
        /// </summary>
        /// <param name="path">目标路径。</param>
        public void CreateFileOrFolder(string path)
        {
            // 调用子节点创建方法
            ChildHasCreate();
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
                if (IsDownload && !File.Exists(targetPath))
                {
                    // 创建文件流
                    var stream = File.Create(targetPath);
                    // 设置文件长度
                    stream.SetLength(Length);
                    // 释放文件流
                    stream.Dispose();
                }
            }
            else
            {
                // 如果不是文件且需要下载，但目录不存在
                if (!Directory.Exists(targetPath) && IsDownload)
                {
                    // 创建目录
                    Directory.CreateDirectory(targetPath);
                }

                // 遍历所有子节点，递归调用私有方法创建文件或文件夹
                LoopAllChildNodes((n, s) =>
                {
                    n.PrivateCreateFileOrFolder(s);
                }, targetPath);
            }
        }

        /// <summary>
        /// 子节点已创建。
        /// </summary>
        public void ChildHasCreate()
        {
            // 如果是文件，则直接返回
            if (IsFile)
                return;

            // 如果存在子节点
            if (HasChildNodes)
            {
                // 遍历子节点，递归调用子节点创建方法
                LoopChildNodes(t => { t.ChildHasCreate(); });
            }

            // 遍历所有节点
            for (int i = 0; i < Count; i++)
            {
                var item = this[i];
                // 如果节点是文件且需要下载
                if (item.IsFile && item.IsDownload)
                {
                    // 设置当前节点为需要下载
                    IsDownload = true;
                }
            }
        }
    }
}
