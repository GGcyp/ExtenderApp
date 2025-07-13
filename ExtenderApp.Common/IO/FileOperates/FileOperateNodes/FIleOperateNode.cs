using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 表示一个文件操作节点的类。
    /// </summary>
    /// <typeparam name="FIleOperateNode">文件操作节点的类型。</typeparam>
    public class FIleOperateNode : FIleOperateNode<FIleOperateNode>
    {
    }

    /// <summary>
    /// 文件操作节点类，继承自Node泛型类，实现了IResettable接口。
    /// </summary>
    public class FIleOperateNode<T> : FileNode<T> where T : FIleOperateNode<T>, new()
    {
        /// <summary>
        /// 获取或设置文件操作接口
        /// </summary>
        /// <value>文件操作接口</value>
        public IFileOperate? FileOperate { get; set; }

        /// <summary>
        /// 检查是否可以执行创建文件操作
        /// </summary>
        /// <returns>如果 <see cref="Name"/> 为空或仅包含空白字符，则返回 true；否则返回 false。</returns>
        public virtual bool CanCreateFileOperate()
        {
            return !string.IsNullOrEmpty(Name);
        }
    }
}
