using ExtenderApp.Abstract;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 抽象类 StreamOperation，表示对流执行的操作。
    /// </summary>
    public abstract class StreamOperation : DisposableObject, IStreamOperation
    {
        /// <summary>
        /// 执行对流的操作。
        /// </summary>
        /// <param name="stream">要操作的数据流。</param>
        public abstract void Execute(Stream stream);

        /// <summary>
        /// 尝试重置操作。
        /// </summary>
        /// <returns>如果操作成功重置则返回 true，否则返回 false。</returns>
        public abstract bool TryReset();
    }
}
