using ExtenderApp.Abstract;
using ExtenderApp.Common.Files.Splitter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Splitter
{
    /// <summary>
    /// 一个抽象类，表示文件分割操作，实现了IResettable和IDisposable接口。
    /// </summary>
    internal abstract class SplitterOperation : IResettable, IDisposable
    {
        /// <summary>
        /// 获取或设置文件分割操作对象。
        /// </summary>
        protected SplitterOperate? Operation;

        /// <summary>
        /// 获取文件分割信息。
        /// </summary>
        /// <returns>文件分割信息。</returns>
        protected FileSplitterInfo SplitterInfo => Operation.SplitterInfo;

        /// <summary>
        /// 获取或设置释放操作的委托
        /// </summary>
        public Action<SplitterOperation> ReleaseAction { get; set; }

        /// <summary>
        /// 将操作注入到当前实例中
        /// </summary>
        /// <param name="operation">要注入的文件分割操作</param>
        /// <param name="action">释放操作时要执行的委托</param>
        public virtual void Inject(SplitterOperate operation)
        {
            Operation = operation;
        }

        /// <summary>
        /// 执行文件分割操作。
        /// </summary>
        /// <param name="stream">要分割的文件流。</param>
        public void Execute(FileStream stream)
        {
            ExecuteProtected(stream);
            ReleaseAction.Invoke(this);
        }

        /// <summary>
        /// 执行受保护的操作
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <remarks>这是一个受保护的方法，需要子类重写</remarks>
        protected abstract void ExecuteProtected(FileStream stream);

        /// <summary>
        /// 尝试重置对象状态。
        /// </summary>
        /// <returns>如果成功重置，则返回true；否则返回false。</returns>
        public bool TryReset()
        {
            Operation = null;
            return Reset();
        }

        /// <summary>
        /// 重置对象状态。
        /// </summary>
        /// <returns>如果成功重置，则返回true；否则返回false。</returns>
        protected abstract bool Reset();

        /// <summary>
        /// 释放对象资源。
        /// </summary>
        public void Dispose()
        {
            TryReset();
        }
    }
}
